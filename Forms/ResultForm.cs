using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WoWForeverRandomizer.Data;
using WoWForeverRandomizer.Logic;
using WoWForeverRandomizer.UI;

namespace WoWForeverRandomizer.Forms
{
    // Slot-machine style reveal: the race reel locks first, then the class reel spins
    // through only that race's classes. Click during the spin to skip to the result.
    public partial class ResultForm : WowFormBase
    {
        private enum RollPhase
        {
            SpinningRace,
            PauseAfterRace,
            SpinningClass,
            Done
        }

        // Timing (milliseconds)
        private const int SpinDuration = 1800;            // matches the length of roll_start.wav
        private const int ClassSpinDuration = 1500;       // class reel
        private const int PauseDuration = 350;
        private const int GlowDuration = 700;

        // Layout
        private static readonly Rectangle RacePanel = new Rectangle(24, 62, 336, 180);
        private static readonly Rectangle ClassPanel = new Rectangle(24, 256, 336, 180);

        // One randomizer per screen so "no exact repeat" works across rerolls.
        private static readonly Randomizer SharedRandomizer = new Randomizer();

        private readonly FactionChoice _factionChoice;
        private readonly Stopwatch _phaseClock = new Stopwatch();

        private RollResult _result;
        private RollPhase _phase;
        private long _nextSwapAt;

        // What the reels are currently showing (random while spinning, the real result once locked)
        private WowRace _shownRace;
        private bool _shownFemale;
        private WowClass _shownClass;

        // Glow pulse when a reel locks in (1 = full glow, fades to 0)
        private long _raceLockedAt = -1;
        private long _classLockedAt = -1;
        private readonly Stopwatch _glowClock = Stopwatch.StartNew();

        public ResultForm(FactionChoice factionChoice)
        {
            InitializeComponent();
            _factionChoice = factionChoice;
            SetBackground(BackgroundFor(factionChoice));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartRoll();
        }

        #region Rolling

        private void StartRoll()
        {
            // Result is decided up front; the animation only reveals it.
            _result = SharedRandomizer.Roll(_factionChoice);

            _shownClass = null;
            _raceLockedAt = -1;
            _classLockedAt = -1;
            SetBackground(BackgroundFor(_factionChoice));

            RerollButton.Enabled = false;
            Assets.PlaySound("roll_start.wav");

            BeginPhase(RollPhase.SpinningRace);
            AnimationTimer.Start();
        }

        private void BeginPhase(RollPhase phase)
        {
            _phase = phase;
            _phaseClock.Restart();
            _nextSwapAt = 0;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            long elapsed = _phaseClock.ElapsedMilliseconds;

            switch (_phase)
            {
                case RollPhase.SpinningRace:
                    if (elapsed >= SpinDuration)
                    {
                        LockRace();
                        BeginPhase(RollPhase.PauseAfterRace);
                    }
                    else if (elapsed >= _nextSwapAt)
                    {
                        _shownRace = PickDifferent(GameData.GetRaces(_factionChoice), _shownRace);
                        _shownFemale = SharedRandomizer.PickAny(new[] { true, false });
                        _nextSwapAt = elapsed + SwapDelay(elapsed, SpinDuration);
                    }
                    break;

                case RollPhase.PauseAfterRace:
                    if (elapsed >= PauseDuration)
                    {
                        BeginPhase(RollPhase.SpinningClass);
                    }
                    break;

                case RollPhase.SpinningClass:
                    if (elapsed >= ClassSpinDuration)
                    {
                        LockClass();
                        BeginPhase(RollPhase.Done);
                    }
                    else if (elapsed >= _nextSwapAt)
                    {
                        // Only spin through classes this race can actually be.
                        _shownClass = PickDifferent(_result.Race.AllowedClasses, _shownClass);
                        _nextSwapAt = elapsed + SwapDelay(elapsed, ClassSpinDuration);
                    }
                    break;

                case RollPhase.Done:
                    // Keep ticking until the lock-in glow has faded, then stop.
                    if (_glowClock.ElapsedMilliseconds - _classLockedAt > GlowDuration)
                    {
                        AnimationTimer.Stop();
                    }
                    break;
            }

            Invalidate();
        }

        // Reel slows down near the end
        private static long SwapDelay(long elapsed, int duration)
        {
            double progress = (double)elapsed / duration;
            return (long)(45 + 280 * progress * progress * progress);
        }

        private void LockRace()
        {
            _shownRace = _result.Race;
            _shownFemale = _result.IsFemalePortrait;
            _raceLockedAt = _glowClock.ElapsedMilliseconds;
            SetBackground(BackgroundFor(_result.Race.Faction));
            Assets.PlaySound("race_lock.wav");   // WoW "Strike" sound
        }

        private void LockClass()
        {
            _shownClass = _result.Class;
            _classLockedAt = _glowClock.ElapsedMilliseconds;
            RerollButton.Enabled = true;
            RerollButton.Focus();
            // Final reveal: the worker voice line for the rolled race's faction.
            string voiceLine = _result.Race.Faction == Faction.Horde ? "horde_work_complete.wav" : "alliance_jobs_done.wav";
            Assets.PlaySound(voiceLine);
        }

        private void SkipAnimation()
        {
            if (_phase == RollPhase.Done)
            {
                return;
            }

            if (_phase == RollPhase.SpinningRace)
            {
                LockRace();
            }

            LockClass();
            BeginPhase(RollPhase.Done);
            Invalidate();
        }

        private T PickDifferent<T>(IReadOnlyList<T> items, T current) where T : class
        {
            if (items.Count < 2)
            {
                return items[0];
            }

            T pick;
            do
            {
                pick = SharedRandomizer.PickAny(items);
            } while (pick == current);

            return pick;
        }

        private float GlowStrength(long lockedAt)
        {
            if (lockedAt < 0)
            {
                return 0f;
            }

            float t = (_glowClock.ElapsedMilliseconds - lockedAt) / (float)GlowDuration;
            return t >= 1f ? 0.35f : 1f - 0.65f * t;   // flash, then settle on a soft glow
        }

        private static string BackgroundFor(FactionChoice choice)
        {
            switch (choice)
            {
                case FactionChoice.Alliance:
                    return "Images/bg_alliance.png";
                case FactionChoice.Horde:
                    return "Images/bg_horde.png";
                default:
                    return "Images/bg_faction.png";
            }
        }

        private static string BackgroundFor(Faction faction)
        {
            return faction == Faction.Alliance ? "Images/bg_alliance.png" : "Images/bg_horde.png";
        }

        #endregion

        #region Buttons & input

        private void RerollButton_Click(object sender, EventArgs e)
        {
            StartRoll();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            AnimationTimer.Stop();
            Navigator.GoTo(this, new FactionForm());
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            SkipAnimation();
            base.OnMouseClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                BackButton_Click(this, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (_phase == RollPhase.Done)
                {
                    StartRoll();
                }
                else
                {
                    SkipAnimation();
                }

                // Handled here - don't also let the focused button react to the key.
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AnimationTimer.Stop();
            base.OnFormClosed(e);
        }

        #endregion

        #region Drawing

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DesignMode)
            {
                return;
            }

            Graphics g = e.Graphics;
            WowPainter.SetHighQuality(g);

            string header = _phase == RollPhase.Done ? "FATE HAS CHOSEN" : "ROLLING...";
            WowPainter.DrawGoldText(g, header, Assets.BodyFont, 23f, new RectangleF(0, 14, ClientSize.Width, 36));
            WowPainter.DrawDivider(g, ClientSize.Width / 2, 50, 300);

            DrawRacePanel(g);
            DrawClassPanel(g);
            DrawClassStrip(g);
        }

        private void DrawRacePanel(Graphics g)
        {
            WowPainter.DrawPanel(g, RacePanel);
            DrawPanelLabel(g, RacePanel, "RACE");

            if (_shownRace == null)
            {
                return;
            }

            bool locked = _raceLockedAt >= 0;

            // Faction crest in the corner
            string crest = _shownRace.Faction == Faction.Alliance ? "Images/crest_alliance.png" : "Images/crest_horde.png";
            g.DrawImage(Assets.GetImage(crest), new Rectangle(RacePanel.Right - 46, RacePanel.Top + 10, 36, 36));

            var portraitRect = new Rectangle(RacePanel.Left + RacePanel.Width / 2 - 44, RacePanel.Top + 28, 88, 88);
            Image portrait = Assets.GetImage(_shownRace.GetPortraitPath(_shownFemale));
            Color glowColor = _shownRace.Faction == Faction.Alliance ? Color.FromArgb(90, 150, 255) : Color.FromArgb(255, 70, 40);
            WowPainter.DrawRingedPortrait(g, portrait, portraitRect, glowColor, GlowStrength(_raceLockedAt));

            var nameRect = new RectangleF(RacePanel.Left, RacePanel.Top + 120, RacePanel.Width, 34);
            WowPainter.DrawGoldText(g, _shownRace.Name.ToUpperInvariant(), Assets.BodyFont, locked ? 28f : 25f, nameRect);

            string factionName = _shownRace.Faction.ToString();
            string subtitle = _shownRace.Subtitle == null ? factionName : _shownRace.Subtitle + "  •  " + factionName;
            WowPainter.DrawText(g, subtitle, Assets.BodyFont, 13f, new RectangleF(RacePanel.Left, RacePanel.Top + 152, RacePanel.Width, 20),
                WowPainter.Parchment, Color.FromArgb(190, 175, 140), 2f);
        }

        private void DrawClassPanel(Graphics g)
        {
            WowPainter.DrawPanel(g, ClassPanel);
            DrawPanelLabel(g, ClassPanel, "CLASS");

            if (_shownClass == null)
            {
                // Still waiting on the race - show a "?" placeholder.
                WowPainter.DrawText(g, "?", Assets.TitleFont, 64f,
                    new RectangleF(ClassPanel.Left, ClassPanel.Top + 30, ClassPanel.Width, 100),
                    Color.FromArgb(120, 110, 90), Color.FromArgb(60, 55, 45), 2f);
                return;
            }

            bool locked = _classLockedAt >= 0;
            var iconRect = new Rectangle(ClassPanel.Left + ClassPanel.Width / 2 - 42, ClassPanel.Top + 30, 84, 84);
            WowPainter.DrawFramedIcon(g, Assets.GetImage(_shownClass.IconPath), iconRect, _shownClass.ClassColor, GlowStrength(_classLockedAt));

            // Class names use the official in-game class colours.
            Color classColor = _shownClass.ClassColor;
            Color darker = ControlPaint.Dark(classColor, 0.15f);
            var nameRect = new RectangleF(ClassPanel.Left, ClassPanel.Top + 120, ClassPanel.Width, 34);
            WowPainter.DrawText(g, _shownClass.Name.ToUpperInvariant(), Assets.BodyFont, locked ? 28f : 25f, nameRect, classColor, darker, 3f);

            if (locked && _shownRace != null)
            {
                string combo = _shownRace.Name + " " + _shownClass.Name;
                WowPainter.DrawText(g, combo, Assets.BodyFont, 13f, new RectangleF(ClassPanel.Left, ClassPanel.Top + 152, ClassPanel.Width, 20),
                    WowPainter.Parchment, Color.FromArgb(190, 175, 140), 2f);
            }
        }

        // Row of every class the race can play, current one highlighted
        private void DrawClassStrip(Graphics g)
        {
            if (_raceLockedAt < 0)
            {
                return;
            }

            WowRace race = _result.Race;
            WowPainter.DrawText(g, "Possible classes for " + race.Name, Assets.BodyFont, 12f,
                new RectangleF(0, 446, ClientSize.Width, 18), WowPainter.Parchment, Color.FromArgb(190, 175, 140), 1.5f);

            const int iconSize = 30;
            const int gap = 8;
            int count = race.AllowedClasses.Count;
            int totalWidth = count * iconSize + (count - 1) * gap;
            int x = (ClientSize.Width - totalWidth) / 2;

            foreach (WowClass wowClass in race.AllowedClasses)
            {
                var rect = new Rectangle(x, 474, iconSize, iconSize);
                bool isCurrent = wowClass == _shownClass;
                Image icon = Assets.GetImage(wowClass.IconPath);

                if (isCurrent)
                {
                    WowPainter.DrawFramedIcon(g, icon, rect, wowClass.ClassColor, 0.9f);
                }
                else
                {
                    WowPainter.DrawFramedIcon(g, icon, rect, Color.Black, 0f, 0.45f);
                }

                x += iconSize + gap;
            }
        }

        private static void DrawPanelLabel(Graphics g, Rectangle panel, string text)
        {
            WowPainter.DrawText(g, text, Assets.BodyFont, 12f, new RectangleF(panel.Left, panel.Top + 6, panel.Width, 18),
                Color.FromArgb(200, 190, 160), Color.FromArgb(150, 135, 100), 1.5f);
        }

        #endregion
    }
}
