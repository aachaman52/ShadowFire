# ShadowFire

A 3D wave-based FPS survival game developed under **Aachman Studios**.

---

## Overview

**ShadowFire** is a high-intensity 3D first-person survival shooter built with Unity's Universal Render Pipeline (URP). Players must endure endless scaling enemy hordes in a fortified industrial arena, level up to acquire game-changing upgrades, wield a modular five-weapon arsenal, and defeat multi-phase boss titans.

---

## Unity Version

* **Unity 6.2 LTS** (Universal Render Pipeline)
* **Scripting Runtime**: C# (.NET Standard 2.1 / Unity 6)
* **Input**: Unity New Input System (with legacy input fallback)
* **AI & Navigation**: Unity AI Navigation (NavMesh)
* **UI**: TextMeshPro & Unity UI (uGUI)

---

## Main Gameplay Features

* **Responsive 6-DOF Player Controller**: WASD movement, sprint with stamina drain & regeneration, smooth crouch height interpolation, jumping physics, mouse look with sensitivity adjustment, dynamic head bobbing, and gait-synced footstep audio.
* **Modular Combat System**: Comprehensive weapon framework handling hitscan raycasting, bullet penetration, multi-pellet spread cones, physics projectiles with explosive splash damage, recoil recovery, procedural sway, muzzle flashes, and bullet tracers.
* **Intelligent NavMesh AI**: State machine-driven enemy AI (Idle, Chase, Attack, Dead) with additive non-blocking flinch feedback.
* **Dynamic Visual & Audio Polish**: Trauma-based camera shake, dynamic crosshair spread bloom, hitmarkers (with kill confirmation pulse), 3D floating damage numbers, screen damage flashes, and low-health vignette heartbeat.
* **Persistent Save System**: JSON save storage tracking High Score, Highest Wave, Total Kills, and custom player settings.

---

## Weapons

1. **Assault Rifle**: Automatic kinetic rifle with steady recoil, high reliability, and balanced damage.
2. **Viper SMG**: Rapid-fire machine pistol tailored for high-volume close-range swarms.
3. **Apex Sniper**: Heavy caliber bolt-action rifle featuring scoped zoom FOV and high penetration (pierces up to 4 enemies).
4. **Breaker Shotgun**: 8-pellet cone spread weapon dealing heavy close-range damage and kinetic knockback.
5. **Havoc Rocket Launcher**: Heavy projectile launcher firing explosive rockets with 7.5m radius splash damage and physics impulse.

---

## Enemy Types

* **Zombie**: Standard swarming melee infantry.
* **Shadow Runner**: Agile, high-speed flanker with lunging leap attacks.
* **Goliath Tank**: Armored juggernaut with massive health, ground stomp shockwaves, and 90% knockback resistance.
* **Shadow Spitter**: Tactical ranged combatant that maintains standoff distance while firing projectile bursts.
* **Shadow Overlord (Boss)**: Colossal boss encountering players every 5 waves with charge attacks, 360° ground slams, homing barrage attacks, and an **Enrage Phase** (<30% HP) with increased speed and attack rates.

---

## Wave System

* Waves progressively increase in enemy count, enemy health, and movement speed.
* Every 5th wave (Wave 5, 10, 15...) triggers a dedicated **Boss Encounter** accompanied by minion escorts.
* 5-second tactical countdown between waves with audio countdown cues.

---

## Upgrade System

* Enemies drop XP gems that fill the player's XP bar.
* Leveling up pauses the game and presents 3 randomized upgrade cards from a 10-card deck:
  1. **Heavy Caliber**: +20% Damage
  2. **Quick Mags**: +25% Faster Reload Speed
  3. **Drum Magazine**: +30% Magazine Capacity
  4. **Adrenaline Rush**: +20% Sprint Speed
  5. **Vitality Matrix**: +25 Max HP & instant heal
  6. **Titanium Plates**: +15 Armor (damage reduction)
  7. **Hollow Point**: +15% Critical Hit Chance
  8. **Overclock Firing**: +20% Fire Rate
  9. **Shock Shells**: Explosive kinetic rounds
  10. **Vampiric Leech**: +10% Lifesteal

---

## Controls

| Action | Input |
|---|---|
| Move | `W` `A` `S` `D` |
| Look | `Mouse Movement` |
| Fire | `Left Mouse Button` |
| Scope / Zoom | `Right Mouse Button` |
| Sprint | `Left Shift` |
| Jump | `Space` |
| Crouch | `Left Ctrl` / `C` |
| Reload | `R` |
| Switch Weapon | `1` `2` `3` `4` `5` or `Mouse Scroll Wheel` |
| Pause Menu | `Escape` |

---

## How to Open the Project

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select the repository root folder (`temp1`).
4. Ensure the editor version is set to **Unity 6.2 LTS** (or compatible Unity 6).
5. Open the project.

---

## How to Play

1. In the Unity Project window, navigate to `Assets/Scenes/`.
2. Open `ShadowFireArena.unity`.
3. Click the **Play** button (▶) in the Unity Editor toolbar.
4. Survive waves, pick up loot, level up upgrades, and eliminate the Shadow Overlord!

---

## Current Development Status

* **Status**: Complete Playable Core Engine & Prototype.
* **Architecture**: Fully modular, zero missing serialized references, zero placeholder assets.
* **Ready for expansion**: Content packs, new arenas, and multiplayer networking can build directly on top of the established `IDamageable`, `Weapon`, and `EnemyBase` abstractions.
