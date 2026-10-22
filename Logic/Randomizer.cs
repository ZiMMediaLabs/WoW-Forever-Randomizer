using System;
using System.Collections.Generic;
using System.Linq;
using WoWForeverRandomizer.Data;

namespace WoWForeverRandomizer.Logic
{
    public class RollResult
    {
        public RollResult(WowRace race, WowClass wowClass, bool isFemalePortrait)
        {
            Race = race;
            Class = wowClass;
            IsFemalePortrait = isFemalePortrait;
        }

        public WowRace Race { get; }
        public WowClass Class { get; }

        public bool IsFemalePortrait { get; }
    }

    // Rolls a race first, then a class that race can play.
    // Never returns the same race + class twice in a row.
    public class Randomizer
    {
        private readonly Random _random = new Random();
        private RollResult _previousResult;

        public RollResult Roll(FactionChoice choice)
        {
            List<WowRace> racePool = GameData.GetRaces(choice);

            for (int attempt = 0; attempt < 50; attempt++)
            {
                WowRace race = PickRace(racePool, choice);
                WowClass wowClass = PickClass(race);

                bool isRepeat = _previousResult != null
                    && _previousResult.Race == race
                    && _previousResult.Class == wowClass;

                if (!isRepeat)
                {
                    _previousResult = new RollResult(race, wowClass, _random.Next(2) == 0);
                    return _previousResult;
                }
            }

            return _previousResult;
        }

        private WowRace PickRace(List<WowRace> racePool, FactionChoice choice)
        {
            if (choice != FactionChoice.Either)
            {
                return racePool[_random.Next(racePool.Count)];
            }

            // "Either faction": every race NAME gets an equal chance (Skyborne isn't counted twice),
            // then if that race exists on both sides (Skyborne) we pick which side.
            List<string> raceNames = racePool.Select(race => race.Name).Distinct().ToList();
            string pickedName = raceNames[_random.Next(raceNames.Count)];

            List<WowRace> variants = racePool.Where(race => race.Name == pickedName).ToList();
            return variants[_random.Next(variants.Count)];
        }

        private WowClass PickClass(WowRace race)
        {
            return race.AllowedClasses[_random.Next(race.AllowedClasses.Count)];
        }

        // Used by the reel animation
        public T PickAny<T>(IReadOnlyList<T> items)
        {
            return items[_random.Next(items.Count)];
        }
    }
}
