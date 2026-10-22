// Offline cache. Bump CACHE_VERSION when files change.

const CACHE_VERSION = "wow-randomizer-v1";

const FILES = [
  "./",
  "index.html",
  "style.css",
  "app.js",
  "manifest.json",
  "assets/images/bg_alliance.png",
  "assets/images/bg_faction.png",
  "assets/images/bg_horde.png",
  "assets/images/bg_title.png",
  "assets/images/btn_down.png",
  "assets/images/btn_hover.png",
  "assets/images/btn_up.png",
  "assets/images/crest_alliance.png",
  "assets/images/crest_horde.png",
  "assets/images/dice.png",
  "assets/images/tile_alliance.png",
  "assets/images/tile_any.png",
  "assets/images/tile_horde.png",
  "assets/images/wow_logo.png",
  "assets/images/classes/Druid.png",
  "assets/images/classes/Hunter.png",
  "assets/images/classes/Mage.png",
  "assets/images/classes/Paladin.png",
  "assets/images/classes/Priest.png",
  "assets/images/classes/Rogue.png",
  "assets/images/classes/Shaman.png",
  "assets/images/classes/Warlock.png",
  "assets/images/classes/Warrior.png",
  "assets/images/races/Dwarf_Female.png",
  "assets/images/races/Dwarf_Male.png",
  "assets/images/races/Gnome_Female.png",
  "assets/images/races/Gnome_Male.png",
  "assets/images/races/Human_Female.png",
  "assets/images/races/Human_Male.png",
  "assets/images/races/NightElf_Female.png",
  "assets/images/races/NightElf_Male.png",
  "assets/images/races/Orc_Female.png",
  "assets/images/races/Orc_Male.png",
  "assets/images/races/Skyborne_Female.png",
  "assets/images/races/Skyborne_Male.png",
  "assets/images/races/Tauren_Female.png",
  "assets/images/races/Tauren_Male.png",
  "assets/images/races/Troll_Female.png",
  "assets/images/races/Troll_Male.png",
  "assets/images/races/Undead_Female.png",
  "assets/images/races/Undead_Male.png",
  "assets/icons/icon-180.png",
  "assets/icons/icon-192.png",
  "assets/icons/icon-512.png",
  "assets/icons/icon-maskable-512.png",
  "assets/sounds/alliance_jobs_done.mp3",
  "assets/sounds/click.mp3",
  "assets/sounds/horde_work_complete.mp3",
  "assets/sounds/race_lock.mp3",
  "assets/sounds/roll_start.mp3",
  "assets/fonts/Cinzel-Bold.ttf",
  "assets/fonts/CinzelDecorative-Bold.ttf"
];

self.addEventListener("install", (event) => {
  event.waitUntil(caches.open(CACHE_VERSION).then((cache) => cache.addAll(FILES)));
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  // Delete caches from older versions.
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((key) => key !== CACHE_VERSION).map((key) => caches.delete(key)))
    )
  );
  self.clients.claim();
});

self.addEventListener("fetch", (event) => {
  if (event.request.method !== "GET") return;
  event.respondWith(
    caches.match(event.request).then((cached) => cached || fetch(event.request))
  );
});
