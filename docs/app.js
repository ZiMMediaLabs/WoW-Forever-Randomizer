// WoW Forever Randomizer (web version)

"use strict";

// Game data (WoW Forever beta roster)

const CLASSES = {
  Druid:   { name: "Druid",   color: "#FF7C0A" },
  Hunter:  { name: "Hunter",  color: "#AAD372" },
  Mage:    { name: "Mage",    color: "#3FC7EB" },
  Paladin: { name: "Paladin", color: "#F48CBA" },
  Priest:  { name: "Priest",  color: "#FFFFFF" },
  Rogue:   { name: "Rogue",   color: "#FFF468" },
  Shaman:  { name: "Shaman",  color: "#0070DD" },
  Warlock: { name: "Warlock", color: "#8788EE" },
  Warrior: { name: "Warrior", color: "#C69B6D" },
};

// Skyborne is split by faction: High Order (Alliance) gets Mage, Windshaper (Horde) gets Shaman.
const RACES = [
  // Alliance
  race("Human",     null,         "Alliance", "Human",    "Hunter Mage Paladin Priest Rogue Warlock Warrior"),
  race("Dwarf",     null,         "Alliance", "Dwarf",    "Hunter Paladin Priest Rogue Shaman Warrior"),
  race("Night Elf", null,         "Alliance", "NightElf", "Druid Hunter Priest Rogue Warrior"),
  race("Gnome",     null,         "Alliance", "Gnome",    "Mage Priest Rogue Warlock Warrior"),
  race("Skyborne",  "High Order", "Alliance", "Skyborne", "Druid Hunter Mage Rogue Warrior"),
  // Horde
  race("Orc",       null,         "Horde",    "Orc",      "Hunter Mage Rogue Shaman Warlock Warrior"),
  race("Undead",    null,         "Horde",    "Undead",   "Mage Paladin Priest Rogue Warlock Warrior"),
  race("Tauren",    null,         "Horde",    "Tauren",   "Druid Hunter Shaman Warrior"),
  race("Troll",     null,         "Horde",    "Troll",    "Hunter Mage Priest Rogue Shaman Warlock Warrior"),
  race("Skyborne",  "Windshaper", "Horde",    "Skyborne", "Druid Hunter Rogue Shaman Warrior"),
];

function race(name, subtitle, faction, portrait, classList) {
  return {
    name,
    subtitle,
    faction,
    portrait,
    classes: classList.split(" ").map((key) => CLASSES[key]),
  };
}

function racesFor(choice) {
  return choice === "Either" ? RACES : RACES.filter((r) => r.faction === choice);
}

const portraitPath = (r, female) => `assets/images/races/${r.portrait}_${female ? "Female" : "Male"}.png`;
const classIconPath = (c) => `assets/images/classes/${c.name}.png`;

// Randomizer: race first, then a class that race can play.
// Never gives the exact same race + class twice in a row.

const pickAny = (items) => items[Math.floor(Math.random() * items.length)];

let previousRoll = null;

function roll(choice) {
  const pool = racesFor(choice);

  for (let attempt = 0; attempt < 50; attempt++) {
    const pickedRace = pickRace(pool, choice);
    const pickedClass = pickAny(pickedRace.classes);
    const isRepeat = previousRoll && previousRoll.race === pickedRace && previousRoll.cls === pickedClass;

    if (!isRepeat) {
      previousRoll = { race: pickedRace, cls: pickedClass, female: Math.random() < 0.5 };
      return previousRoll;
    }
  }

  return previousRoll;
}

function pickRace(pool, choice) {
  if (choice !== "Either") {
    return pickAny(pool);
  }

  // "Either": every race NAME gets an equal chance (Skyborne isn't counted twice),
  // then if that race exists on both sides we pick which side.
  const names = [...new Set(pool.map((r) => r.name))];
  const pickedName = pickAny(names);
  return pickAny(pool.filter((r) => r.name === pickedName));
}

function pickDifferent(items, current) {
  if (items.length < 2) return items[0];
  let next;
  do {
    next = pickAny(items);
  } while (next === current);
  return next;
}

// Sound (Web Audio so sounds can play on a timer, even on phones).
// A new sound stops the one before it.

const Sound = {
  context: null,
  buffers: {},
  current: null,
  names: ["click", "roll_start", "race_lock", "alliance_jobs_done", "horde_work_complete"],

  // Phones only allow audio after the user taps something, so this runs on the first tap.
  unlock() {
    if (this.context) {
      if (this.context.state === "suspended") this.context.resume();
      return;
    }

    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) return;

    this.context = new AudioCtx();
    this.names.forEach((name) => {
      fetch(`assets/sounds/${name}.mp3`)
        .then((response) => response.arrayBuffer())
        .then((data) => new Promise((resolve, reject) => this.context.decodeAudioData(data, resolve, reject)))
        .then((buffer) => { this.buffers[name] = buffer; })
        .catch(() => {});
    });
  },

  play(name) {
    const buffer = this.buffers[name];
    if (!this.context || !buffer) return;

    if (this.current) {
      try { this.current.stop(); } catch (e) { /* already finished */ }
    }

    const source = this.context.createBufferSource();
    source.buffer = buffer;
    source.connect(this.context.destination);
    source.start();
    this.current = source;
  },
};

// Screens

const $ = (id) => document.getElementById(id);
const stage = $("stage");
let currentScreen = null;

function showScreen(id) {
  document.querySelectorAll(".screen").forEach((screen) => { screen.hidden = screen.id !== id; });
  currentScreen = id;
  setBackground($(id).dataset.bg);
}

function setBackground(path) {
  stage.style.backgroundImage = `url("${path}")`;
  $("backdrop").style.backgroundImage = `url("${path}")`;
}

// Sets text on an outlined text element (the outline copy reads data-text).
function setText(element, text) {
  element.textContent = text;
  element.dataset.text = text;
}

// Result screen: slot-machine reveal (race reel, then class reel)

const RACE_SPIN_MS = 1800;   // matches the length of roll_start
const PAUSE_MS = 350;
const CLASS_SPIN_MS = 1500;
const GLOW_MS = 700;

const result = {
  choice: null,
  roll: null,
  phase: "done",          // spinningRace | pause | spinningClass | done
  phaseStart: 0,
  nextSwapAt: 0,
  shownRace: null,
  shownFemale: false,
  shownClass: null,
  raceLockedAt: -1,
  classLockedAt: -1,
  frame: 0,
};

function startRoll() {
  result.roll = roll(result.choice);
  result.shownClass = null;
  result.raceLockedAt = -1;
  result.classLockedAt = -1;
  setBackground($("screen-result").dataset.bg);
  $("btn-reroll").disabled = true;

  Sound.play("roll_start");
  beginPhase("spinningRace");
  cancelAnimationFrame(result.frame);
  result.frame = requestAnimationFrame(tick);
}

function beginPhase(phase) {
  result.phase = phase;
  result.phaseStart = performance.now();
  result.nextSwapAt = 0;
}

// Name swaps slow down near the end of the spin.
function swapDelay(elapsed, duration) {
  const progress = elapsed / duration;
  return 45 + 280 * progress * progress * progress;
}

function tick(now) {
  const elapsed = now - result.phaseStart;

  switch (result.phase) {
    case "spinningRace":
      if (elapsed >= RACE_SPIN_MS) {
        lockRace();
        beginPhase("pause");
      } else if (elapsed >= result.nextSwapAt) {
        result.shownRace = pickDifferent(racesFor(result.choice), result.shownRace);
        result.shownFemale = Math.random() < 0.5;
        result.nextSwapAt = elapsed + swapDelay(elapsed, RACE_SPIN_MS);
      }
      break;

    case "pause":
      if (elapsed >= PAUSE_MS) beginPhase("spinningClass");
      break;

    case "spinningClass":
      if (elapsed >= CLASS_SPIN_MS) {
        lockClass();
        beginPhase("done");
      } else if (elapsed >= result.nextSwapAt) {
        // Only spin through classes this race can actually be.
        result.shownClass = pickDifferent(result.roll.race.classes, result.shownClass);
        result.nextSwapAt = elapsed + swapDelay(elapsed, CLASS_SPIN_MS);
      }
      break;
  }

  render();

  // Keep animating until the lock-in glow has faded.
  const glowDone = result.phase === "done" && now - result.classLockedAt > GLOW_MS;
  if (!glowDone) result.frame = requestAnimationFrame(tick);
}

function lockRace() {
  result.shownRace = result.roll.race;
  result.shownFemale = result.roll.female;
  result.raceLockedAt = performance.now();
  setBackground(`assets/images/bg_${result.roll.race.faction.toLowerCase()}.png`);
  Sound.play("race_lock");
}

function lockClass() {
  result.shownClass = result.roll.cls;
  result.classLockedAt = performance.now();
  $("btn-reroll").disabled = false;
  Sound.play(result.roll.race.faction === "Horde" ? "horde_work_complete" : "alliance_jobs_done");
}

// Skip the animation and show the result immediately.
function skipAnimation() {
  if (result.phase === "done") return;
  if (result.phase === "spinningRace") lockRace();
  lockClass();
  beginPhase("done");
  render();
}

// Bright flash when something locks in, then a soft steady glow.
function glowStrength(lockedAt) {
  if (lockedAt < 0) return 0;
  const t = (performance.now() - lockedAt) / GLOW_MS;
  return t >= 1 ? 0.35 : 1 - 0.65 * t;
}

// Shrinks a text element's font until it fits its width (long names).
function fitText(element, baseSize) {
  element.style.fontSize = baseSize + "px";
  const maxWidth = element.clientWidth - 8;
  const range = document.createRange();
  range.selectNodeContents(element);
  const width = range.getBoundingClientRect().width / currentScale;
  if (width > maxWidth && width > 0) {
    element.style.fontSize = (baseSize * maxWidth / width).toFixed(1) + "px";
  }
}

function render() {
  const done = result.phase === "done";
  setText($("result-header"), done ? "FATE HAS CHOSEN" : "ROLLING...");

  renderRace();
  renderClass();
  renderStrip();
}

function renderRace() {
  const r = result.shownRace;
  const crest = $("race-crest");
  const portrait = $("race-portrait");
  const nameEl = $("race-name");
  const subEl = $("race-sub");

  if (!r) {
    crest.removeAttribute("src");
    portrait.classList.remove("has-image");
    setText(nameEl, "");
    setText(subEl, "");
    return;
  }

  const locked = result.raceLockedAt >= 0;
  crest.src = `assets/images/crest_${r.faction.toLowerCase()}.png`;

  const img = portrait.querySelector("img");
  const src = portraitPath(r, result.shownFemale);
  if (img.getAttribute("src") !== src) img.src = src;
  portrait.classList.add("has-image");
  portrait.style.setProperty("--glow-color", r.faction === "Alliance" ? "rgba(90,150,255,0.9)" : "rgba(255,70,40,0.9)");
  portrait.style.setProperty("--glow", (glowStrength(result.raceLockedAt) * 22).toFixed(1) + "px");

  nameEl.classList.toggle("locked", locked);
  setText(nameEl, r.name.toUpperCase());
  fitText(nameEl, locked ? 28 : 25);

  setText(subEl, r.subtitle ? `${r.subtitle}  •  ${r.faction}` : r.faction);
}

function renderClass() {
  const c = result.shownClass;
  const question = $("class-question");
  const icon = $("class-icon");
  const nameEl = $("class-name");
  const subEl = $("class-sub");

  question.hidden = !!c;
  if (!c) {
    icon.classList.remove("has-image");
    setText(nameEl, "");
    setText(subEl, "");
    return;
  }

  const locked = result.classLockedAt >= 0;
  const img = icon.querySelector("img");
  const src = classIconPath(c);
  if (img.getAttribute("src") !== src) img.src = src;
  icon.classList.add("has-image");
  icon.style.setProperty("--glow-color", c.color);
  icon.style.setProperty("--glow", (glowStrength(result.classLockedAt) * 22).toFixed(1) + "px");

  // Class names use the official in-game class colours.
  nameEl.style.setProperty("--top", c.color);
  nameEl.style.setProperty("--bottom", `color-mix(in srgb, ${c.color} 82%, #000)`);
  nameEl.classList.toggle("locked", locked);
  setText(nameEl, c.name.toUpperCase());
  fitText(nameEl, locked ? 28 : 25);

  setText(subEl, locked && result.shownRace ? `${result.shownRace.name} ${c.name}` : "");
}

function renderStrip() {
  const label = $("strip-label");
  const strip = $("class-strip");

  if (result.raceLockedAt < 0) {
    setText(label, "");
    strip.replaceChildren();
    return;
  }

  const r = result.roll.race;
  setText(label, `Possible classes for ${r.name}`);

  if (strip.dataset.race !== r.name + r.faction) {
    strip.replaceChildren(...r.classes.map((c) => {
      const img = document.createElement("img");
      img.src = classIconPath(c);
      img.alt = c.name;
      img.title = c.name;
      img.dataset.cls = c.name;
      img.style.setProperty("--glow-color", c.color);
      return img;
    }));
    strip.dataset.race = r.name + r.faction;
  }

  strip.querySelectorAll("img").forEach((img) => {
    img.classList.toggle("current", !!result.shownClass && img.dataset.cls === result.shownClass.name);
  });
}

// Wiring: buttons, taps and keyboard

function goTitle() {
  cancelAnimationFrame(result.frame);
  showScreen("screen-title");
}

function goFaction() {
  cancelAnimationFrame(result.frame);
  showScreen("screen-faction");
}

function goResult(choice) {
  result.choice = choice;
  result.shownRace = null;
  result.shownClass = null;
  $("class-strip").dataset.race = "";
  showScreen("screen-result");
  startRoll();
}

$("btn-start").addEventListener("click", () => { Sound.play("click"); goFaction(); });
$("btn-faction-back").addEventListener("click", () => { Sound.play("click"); goTitle(); });
$("btn-result-back").addEventListener("click", () => { Sound.play("click"); goFaction(); });
$("btn-reroll").addEventListener("click", startRoll);

document.querySelectorAll(".faction-tile").forEach((tile) => {
  tile.addEventListener("click", () => goResult(tile.dataset.choice));
});

// Tap anywhere on the results screen during the spin to skip to the result.
$("screen-result").addEventListener("click", (e) => {
  if (!e.target.closest("button")) skipAnimation();
});

// Audio can only start after the first tap or key press.
["pointerdown", "keydown", "touchstart"].forEach((type) => {
  window.addEventListener(type, () => Sound.unlock(), { capture: true, passive: true });
});

window.addEventListener("keydown", (e) => {
  if (currentScreen === "screen-title" && (e.key === "Enter" || e.key === " ")) {
    e.preventDefault();
    $("btn-start").click();
  } else if (currentScreen === "screen-faction" && e.key === "Escape") {
    $("btn-faction-back").click();
  } else if (currentScreen === "screen-result") {
    if (e.key === "Escape") {
      $("btn-result-back").click();
    } else if (e.key === " " || e.key === "Enter") {
      e.preventDefault();
      if (result.phase === "done") startRoll();
      else skipAnimation();
    }
  }
});

// Fit the 384 x 661 stage to the screen

let currentScale = 1;

function fitStage() {
  const scale = Math.min(window.innerWidth / 384, window.innerHeight / 661, 1.6);
  currentScale = scale;
  stage.style.transform = `scale(${scale})`;
}

window.addEventListener("resize", fitStage);
window.addEventListener("orientationchange", fitStage);

// Start up

document.querySelectorAll(".panel").forEach((panel) => {
  ["tl", "tr", "bl", "br"].forEach((corner) => {
    const stud = document.createElement("i");
    stud.className = "stud " + corner;
    panel.appendChild(stud);
  });
});

// Preload artwork so nothing pops in mid-spin.
[
  ...RACES.flatMap((r) => [portraitPath(r, false), portraitPath(r, true)]),
  ...Object.values(CLASSES).map(classIconPath),
  "assets/images/bg_alliance.png", "assets/images/bg_horde.png", "assets/images/bg_faction.png",
  "assets/images/crest_alliance.png", "assets/images/crest_horde.png",
  "assets/images/btn_hover.png", "assets/images/btn_down.png",
].forEach((src) => { new Image().src = src; });

fitStage();
showScreen("screen-title");

// Service worker (offline + install)
if ("serviceWorker" in navigator) {
  window.addEventListener("load", () => navigator.serviceWorker.register("service-worker.js").catch(() => {}));
}
