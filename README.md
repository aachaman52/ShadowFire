# ShadowFire

A 3D wave-based FPS survival game developed under **Aachman Studios**.

---

## Overview

**ShadowFire** is a production-quality 3D first-person survival shooter built with Unity's Universal Render Pipeline (URP). Players survive endless scaling enemy hordes in multiple thematic environments, acquire game-changing deck upgrades, wield a modular multi-part 3D arsenal with mechanical recoil and reload animations, and eliminate multi-phase boss titans.

---

## Unity Version & Architecture

* **Engine**: Unity 6.2 LTS (Universal Render Pipeline)
* **Scripting Runtime**: C# (.NET Standard 2.1 / Unity 6)
* **Input**: Unity New Input System (with legacy input fallback)
* **AI & Navigation**: Unity AI Navigation (NavMesh)
* **UI**: TextMeshPro & Unity UI (uGUI)

---

## 4 Core Pillars

### 1. Articulated Characters & Procedural Skeletal Animation
* **Procedural Humanoid Rig**: Multi-joint skeletal hierarchy (`ProceduralCharacterAnimator.cs` & `CharacterModelBuilder.cs`) driving Pelvis, Spine, Chest, Head, Shoulders, Upper Arms, Forearms, Hands, Thighs, Shins, and Feet.
* **Locomotion & Combat States**:
  * Realtime joint rotation driving breathing, walk/run stride cycling, spine lean, and head bobbing.
  * Melee double-swipe attacks, leaping lunges, two-handed overhead ground smashes, and spitter bio-recoil.
  * Dynamic impact flinches and physics-inspired collapse death sequences.

### 2. High-Detail 3D Weapons & Dynamic Mechanical Animations
* **Modular Multi-Part 3D Models** (`DetailedWeaponMeshBuilder.cs`):
  * **Assault Rifle**: Receiver, picatinny top rail, fluted barrel, muzzle compensator, tactical stock, pistol grip, holographic sight with glowing center reticle, and removable curved magazine.
  * **Viper SMG**: Compact submachine frame, vertical foregrip, threaded suppressor shroud, wire stock, reflex sight, and extended 45-round stick magazine.
  * **Apex Sniper**: Heavy chassis, long fluted barrel, high-caliber muzzle brake, precision stock with cheek rest, dual-lens illuminated telescopic scope, foldable bipod, and animated bolt handle.
  * **Breaker Shotgun**: Heavy ribbed barrel, under-barrel tubular magazine, grooved pump slide, top heat shield, and tactical grip.
  * **Havoc Rocket Launcher**: Quad-vented exhaust tube, dual firing handles, targeting computer display with glowing HUD screen, and loaded rocket warhead with aerodynamic fins.
* **Mechanical Combat Animations** (`WeaponAnimationController.cs`):
  * Dynamic viewmodel recoil kickback and rotation recovery.
  * Slide blowback on Rifle/SMG, bolt-action cycling sequence on Sniper (lift $\rightarrow$ pull back $\rightarrow$ push forward $\rightarrow$ lock down), pump slide racking on Shotgun, and magazine drop/snap-in reload sequences.

### 3. Multi-Layered Sound Design & Dynamic Adaptive Music Engine
* **Multi-Layer Acoustic Synthesis** (`MultiLayerSoundSynthesizer.cs`):
  * Gunshots generated with high-frequency transient snaps, low-mid acoustic punch, mechanical slide clanking, and spatial reverb tails.
  * Explosions with sub-bass rumbles, blast noise, and shockwave roars.
  * Titan demonic roars and organic impact audio.
* **Dynamic Adaptive Soundtrack** (`DynamicMusicSystem.cs`):
  * **Ambient Tension**: Dark atmospheric synth pad and suspense pulses during countdowns.
  * **Horde Wave Combat**: 130 BPM arpeggiated synthwave with 4-on-the-floor kick, rolling 16th-note bassline, and lead synths during active waves.
  * **Boss Battle Theme**: 150 BPM industrial darksynth with heavy sub-bass drops, industrial snares, and sirens with Enrage pitch shifts.

### 4. Multi-Map System & 3 Game Modes
* **3 Maps** (`MapDataSO.cs`, `MapBuilders.cs`, `MapManager.cs`):
  1. **Outpost Ruin**: Fortified industrial night fortress with upper catwalks, ramps, searchlights, and defensive barricades.
  2. **Toxic Biolab**: Subterranean research laboratory with glowing acid hazard pools, containment pods, and overhead metal catwalks.
  3. **Inferno Crater**: Volcanic caldera with glowing magma rifts, ancient obsidian obelisks, and molten ash atmosphere.
* **3 Game Modes** (`GameModes.cs`, `ModeManager.cs`):
  1. **Endless Survival**: Endless scaling waves with boss titans every 5 waves.
  2. **Extraction Protocol**: Survive to Wave 10 $\rightarrow$ activate extraction beacon $\rightarrow$ defend landing zone under intense horde assault $\rightarrow$ evac victory!
  3. **Boss Titan Rush**: Sequential titan boss battles with accelerated upgrade drafts after every victory.

---

## Upgrade Deck (10 Cards)

1. **Heavy Caliber**: +20% Damage
2. **Quick Mags**: +25% Faster Reload Speed
3. **Drum Magazine**: +30% Magazine Capacity
4. **Adrenaline Rush**: +20% Sprint Speed
5. **Vitality Matrix**: +25 Max HP & instant 25 HP heal
6. **Titanium Plates**: +15 Armor (damage reduction)
7. **Hollow Point**: +15% Critical Hit Chance (x2 Damage)
8. **Overclock Firing**: +20% Fire Rate
9. **Shock Shells**: Explosive kinetic bullet impacts
10. **Vampiric Leech**: 10% of damage dealt converted to HP

---

## Controls

| Action | Key / Input |
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

## How to Open & Play

1. Open **Unity Hub**.
2. Click **Add** $\rightarrow$ **Add project from disk**.
3. Select this folder (`temp1`).
4. Open `Assets/Scenes/ShadowFireArena.unity`.
5. Press **Play** (▶) in the Unity Editor to enter the arena!
