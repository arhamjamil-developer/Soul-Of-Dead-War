# Soul Crystal Defense — Gameplay Data & Antigravity Handoff

This document is the implementation contract for the first playable version of the game. Build from this document and the referenced assets. Do not invent a different camera direction, board orientation, or core loop.

## Product direction

Build a polished 2D portrait mobile lane-defense game inspired by the immediate readability of classic lane-defense games, but with the identity of **Soul Of Dead War**: painted dark-fantasy warriors, soul energy, crystal progression, portals, ruined environments, and escalating waves.

The player protects a Soul Crystal at the bottom of the screen. Enemies enter from portals at the top and move downward. The player spends Soul Energy to place defenders on fixed tiles. Defenders face upward and automatically attack enemies in their lane or range.

The first implementation must be a complete, playable vertical slice, not a static mockup.

## Non-negotiable camera and board rules

- Portrait orientation only for the gameplay scene.
- Target reference resolution: 1080 x 1920, aspect 9:16.
- Five vertical lanes, numbered left-to-right `0, 1, 2, 3, 4`.
- Eight visible board rows, numbered bottom-to-top `0..7`.
- Player crystal and defender tray are at the bottom.
- Enemy portals and enemy spawn points are at the top.
- Enemies travel from top to bottom.
- Player attacks and projectiles travel from bottom to top.
- Keep the gameplay board centered and fully visible on every phone and iPad.
- Never crop the top portal area or bottom crystal area.
- Use the generated board art as a visual target; gameplay-critical objects must be separate Unity objects.

### Responsive layout contract

Do not stretch the entire 9:16 artwork to every display.

Use these layers:

1. A stretchable dark ground/backdrop that fills the device.
2. A gameplay-board container that preserves its aspect ratio and fits inside the safe area.
3. Separate left/right environment decoration that fills wider iPad side space.
4. Separate top portal objects and bottom crystal objects anchored to the board.
5. HUD anchored to the safe area, not to hard-coded screen pixels.

For uGUI use a Canvas Scaler with Reference Resolution `1080 x 1920`, Scale With Screen Size, Match Width Or Height around `0.5`, and a Safe Area panel driven by `Screen.safeArea`.

For a world-space orthographic board, fit the complete board rather than cropping it. Extra space on tall iPhones or wide iPads should show dark decorative extension art.

Test at minimum:

- 1080 x 1920 (reference phone)
- 1170 x 2532 or equivalent 19.5:9 iPhone
- 1290 x 2796 or equivalent tall iPhone
- 2048 x 2732 iPad portrait

## Asset reference map

### Existing project art

All existing artwork is under `Assets/SHAMSA/Assets`.

Main menu and style references:

- `Assets/SHAMSA/Assets/UI/MainMenu/MainMockup.jpeg`
- `Assets/SHAMSA/Assets/UI/MainMenu/Background.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/LOGO.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/Play Button.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/How to Play Banner.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/SettingsButtons.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/Coin Counter HUD.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/Fire Braziers.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/Ruin Foreground Overlay.png`
- `Assets/SHAMSA/Assets/UI/MainMenu/Rocky Foreground Overlay.png`

Player/defender references:

- Dwarf/Guardian source: `Assets/SHAMSA/Assets/Tortured_manSprites/Barbarian_Ideal.png`, `Barbarian_Move.png`, `Barbarian_Attack.png`
- Chef source: `Assets/SHAMSA/Assets/Chef/Chef_Ideal.png`, `Chef_Move.png`, `Chef_Attack.png`
- Train ultimate source: `Assets/SHAMSA/Assets/Train/Train_Ideal.png`, `Train_Move.png`, `Train_Attack.png`, `train_rotation_sheet_transparent.png`

Boss reference:

- `Assets/SHAMSA/Assets/Elephant/Armored Royal Elephant Rotation Sheet.png`
- `Assets/SHAMSA/Assets/Elephant/Elephant_Ideal.png`
- `Assets/SHAMSA/Assets/Elephant/Elephant_Move.png`
- `Assets/SHAMSA/Assets/Elephant/Elephant_Attack.png`

New enemy sheets:

- `Assets/SHAMSA/Assets/Enemies/BoneRaider/BoneRaider_Down_Animations.png`
- `Assets/SHAMSA/Assets/Enemies/SoulReaper/SoulReaper_Down_Animations.png`
- Import instructions: `Assets/SHAMSA/Assets/Enemies/SpriteSheetLayout.md`

Portrait environment references:

- `Assets/SHAMSA/Concept/Portrait/01-ForgottenGraveyard-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/02-EmberWasteland-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/03-FrozenTombs-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/04-RoyalSoulCitadel-CleanBoard.png`
- Populated gameplay target: `Assets/SHAMSA/Concept/Portrait/PortraitGameplay-VisualTarget.png`

The clean board images are concept/reference backdrops. For final mobile performance, separate the background, board surface, edge decoration, portal strip, and crystal platform rather than making all gameplay geometry one baked image.

## Enemy sheet import contract

Both new enemy PNGs are genuinely transparent and are designed as a `2048 x 768` sheet:

- 8 columns x 3 rows
- 256 x 256 cell size
- Visual top row: Walk Down, 8 frames
- Visual middle row: Attack Down, 8 frames
- Visual bottom row: Death Down, 8 frames
- Pivot: Bottom Center, approximately `(0.5, 0.1)`
- No automatic transparency slicing; automatic slicing creates irregular rectangles and breaks animation alignment.

Use Sprite Editor > Slice > Grid By Cell Size > `256 x 256`. Name the sprites deterministically:

- `BoneRaider_WalkDown_00..07`
- `BoneRaider_AttackDown_00..07`
- `BoneRaider_DeathDown_00..07`
- `SoulReaper_WalkDown_00..07`
- `SoulReaper_AttackDown_00..07`
- `SoulReaper_DeathDown_00..07`

Animation rates:

- Walk: 10–12 FPS, loop
- Attack: 12–16 FPS, no loop
- Death: 10–12 FPS, no loop, hold final frame

## Unit contracts

### Dwarf Guardian defender

Use the existing red-haired hammer warrior as the player’s first blocker.

- Cost: 100 Soul Energy
- Health: 220
- Damage: 24
- Attack cooldown: 0.9 seconds
- Range: same lane, short range approximately 0.9 board units
- Role: stops one lane and knocks enemies back slightly on a heavy hit
- Visual: use the existing Barbarian source; keep the character’s painted top-down silhouette and red cloak

### Blood Chef defender

Use the existing bloodstained chef as the first ranged/splash defender.

- Cost: 125 Soul Energy
- Health: 75
- Damage: 18 per hit
- Attack cooldown: 1.2 seconds
- Range: same lane, approximately 4 board units
- Splash radius: approximately 0.75 board units
- Role: clears groups and punishes enemies that bunch up
- Visual: use the existing Chef source; fire and pan impact should use warm orange effects against the blue/cyan scene

### Bone Raider enemy

Use `BoneRaider_Down_Animations.png`.

- Health: 60
- Movement speed: 0.55 board units/second
- Contact damage to defender: 9
- Attack cooldown: 1.2 seconds
- Attack range: 0.65
- Crystal breach damage: 2
- Reward: 15 Soul Energy
- Role: basic lane pressure; easy to read and easy to defeat

### Soul Reaper enemy

Use `SoulReaper_Down_Animations.png`.

- Health: 95
- Movement speed: 0.42 board units/second
- Ranged damage: 12
- Attack cooldown: 1.5 seconds
- Attack range: 2.0
- Crystal breach damage: 3
- Reward: 25 Soul Energy
- Role: teaches the player that a blocker alone is not enough; introduce it late in Wave 1

### Armored Elephant boss

Do not spawn it in Wave 1. Reserve it for the first boss level.

- Use the existing blue-and-gold elephant source.
- It should charge, damage the first defender, and visually dominate a lane.
- It must be data-driven so later waves can enable it without changing the scene.

### War Train ultimate

Do not place it in the Wave 1 battlefield. Show it as a locked future card or reserve ability. Later it can clear one selected lane from bottom to top using the existing train art.

## Level and wave data model

Create one reusable gameplay scene, for example `Assets/Scenes/SoulCrystalDefenseGameplay.unity`.

Create data assets, not duplicated scene copies:

- `Assets/SHAMSA/GameplayData/Level01_ForgottenGraveyard.asset`
- `Assets/SHAMSA/GameplayData/Wave01_TutorialPressure.asset`

The scene should reference a level data object containing:

- Background/biome reference
- Number of lanes and rows
- Crystal health
- Starting energy
- Passive energy interval and amount
- Defender unlocks
- Wave list
- Reward fragment count
- Star conditions

Each spawn entry should contain:

- Spawn time in seconds
- Lane index `0..4`
- Enemy type
- Optional elite/boss flag
- Optional spawn VFX or portal index

Do not hard-code Wave 1 timing inside enemy scripts.

## Wave 1: Forgotten Graveyard tutorial

Wave 1 should teach the board without being punishing. It should become tense only in the final third.

Board:

- Biome: Forgotten Graveyard
- Crystal health: 20
- Starting Soul Energy: 250
- Passive energy: +25 every 4 seconds
- Defenders available: Dwarf Guardian and Blood Chef
- Enemy types: Bone Raider first, Soul Reaper late
- Expected duration: approximately 40–45 seconds after the countdown
- First-clear reward: 1 crystal fragment, coins, and stars

Lane numbering is left-to-right. Spawn times are measured from the end of the 2-second countdown.

| Time | Lane | Enemy | Purpose |
|---:|---:|---|---|
| 3s | 2 | Bone Raider | First threat; center-lane tutorial |
| 8s | 1 | Bone Raider | Teaches a second lane |
| 12s | 3 | Bone Raider | Teaches symmetrical pressure |
| 18s | 2 | Bone Raider | Tests the first placement |
| 22s | 0 | Bone Raider | Introduces an outer lane |
| 25s | 4 | Bone Raider | Completes outer-lane pressure |
| 31s | 2 | Soul Reaper | Late ranged threat; signals escalation |
| 35s | 1 | Bone Raider | Small final pressure |
| 35.5s | 3 | Bone Raider | Small final pressure |

The player should be able to win with three sensible placements and good lane choices. A player who places badly may lose some crystal health but should still have time to recover. Wave 1 should not require perfect reaction speed.

Recommended victory conditions:

- All nine enemies defeated
- Crystal health greater than zero
- Three stars: finish with at least 16 health, finish before 55 seconds, and deploy no more than five defenders

Recommended defeat conditions:

- Crystal health reaches zero
- Do not instantly reload the scene; show a retry panel and preserve the run result

## Economy and progression

Soul Energy is the in-wave resource. Crystal fragments are the long-term progression resource.

- Enemies reward Soul Energy only after they die.
- Energy is not spent when a tile selection is cancelled.
- The player cannot place on occupied tiles.
- A completed level awards one crystal fragment on first clear.
- Three fragments forge a permanent crystal.
- Crystals unlock new mechanics for later levels, not only raw damage.

Suggested progression:

1. Azure Crystal: second defender slot and starting-energy bonus.
2. Ember Crystal: Chef burn effect.
3. Iron Crystal: barricade/armor mechanics.
4. Storm Crystal: cooldown and chain-lightning mechanics.
5. Crown Crystal: War Train ultimate and final chapter.

## Required gameplay systems

Implement these as separate, testable components:

- `LevelData` / `WaveData` ScriptableObjects
- `WaveDirector` that schedules spawns from data
- `BoardGrid` with five lanes and eight rows
- `PlacementCell` with selected/occupied/invalid visual states
- `DefenderUnit` base class plus Guardian and Chef configurations
- `EnemyUnit` base class plus Bone Raider and Soul Reaper configurations
- `SoulCrystal` health and breach response
- `SoulEnergyWallet`
- `Projectile` and splash-damage logic
- `WaveHUD` and `DefenderCard` UI
- `SafeAreaLayout` and portrait-responsive camera
- `VictoryDefeatController`
- `CrystalRewardPanel`

Use object pooling for enemies, projectiles, and impact effects once the prototype is visually verified. Keep movement and combat contracts independent from the art so new sprites do not change balance.

## Visual requirements

The first playable scene should visually communicate the loop before the player taps anything:

- Blue Soul Crystal platform visibly anchors the bottom.
- Five top portals are visible and readable.
- Lane dividers and placement tiles are visible but subtle.
- The center field remains uncluttered enough for touch selection.
- Dwarf and Chef have strong silhouettes against the graveyard ground.
- Bone Raider uses a restrained red health bar; Soul Reaper uses cyan soul accents.
- Attacks have compact impact flashes and do not cover neighboring lanes.
- The HUD uses blue/cyan soul energy with controlled gold/orange accents.
- Avoid baked UI in the background art.
- Avoid gore, tiny text, and decorative props over placement cells.

## Required UI

Top safe-area HUD:

- Crystal health on the left
- `WAVE 1/1` or current wave text centered
- Soul Energy count on the right
- Pause button on the far right

Bottom safe-area tray:

- Dwarf card with cost 100
- Chef card with cost 125
- Locked future card or elephant preview
- Locked War Train ultimate card
- Clear selected, cooldown, and unaffordable states

Interaction feedback:

- Tap a card, then tap a valid tile to place.
- Selected card follows the pointer as a translucent preview.
- Invalid tiles tint red; valid tiles tint cyan.
- Tap the selected card again or tap empty UI space to cancel.
- New wave/enemy entrance gets a small, non-blocking telegraph.

## Implementation order for Antigravity

1. Correct/import the two enemy sheets as exact 256 x 256 grids.
2. Create the portrait camera, safe-area root, board grid, and graveyard background.
3. Create the crystal and portal visual objects as separate layers.
4. Create the Level and Wave ScriptableObjects with the exact Wave 1 table above.
5. Create enemy prefabs with movement, health, animation rows, and crystal breach logic.
6. Create defender prefabs with placement, target selection, attack, and health.
7. Create energy, wave, crystal-health, and victory/defeat systems.
8. Add bottom card UI and touch placement.
9. Add compact VFX, hit flashes, health bars, and wave telegraphing.
10. Run the full wave on portrait reference resolution, then test iPhone and iPad aspect ratios.
11. Only after Wave 1 is fun and readable, duplicate the data asset for later levels and swap background/enemy mixes.

## Acceptance checklist

The implementation is complete only when all of these are true:

- The scene runs in portrait without manual camera adjustment.
- The entire board, top portals, and bottom crystal remain visible on iPhone and iPad test ratios.
- Player can select Dwarf or Chef and place it on a free tile.
- Energy costs and rewards are correct.
- Bone Raiders walk, attack, take damage, die, and reward energy.
- Soul Reaper appears only in the late part of Wave 1 and uses its ranged attack.
- Enemies stop at defenders instead of passing through them.
- Enemies that reach the crystal damage it and can cause defeat.
- Wave completes only after every scheduled spawn is dead or has breached.
- Victory awards one crystal fragment and shows stars.
- Defeat offers retry without corrupting the level data.
- No sprite automatic-slicing irregularities remain.
- No gameplay-critical object is baked into the background image.
- The board can be reused for later levels by changing data and background references.

## Visual and gameplay references

The original concept and board plan remain in:

- `Assets/SHAMSA/SoulCrystalDefense-GamePlan.md`
- `Assets/SHAMSA/Concept/SoulCrystalDefense-GameplayConcept.png`

This file takes precedence for the portrait orientation, bottom-to-top flow, Wave 1 balance, and implementation order.

## Four-hour vertical-slice production sprint

The following is a focused four-hour build plan for Antigravity. The goal is a complete, playable, visually coherent vertical slice that can be reviewed on a phone. It is not a promise that a full 999-wave commercial game can be authored in four hours; the four-hour result is the foundation and the data pipeline that makes the later content scalable.

### 00:00–00:20 — Lock the contract and import the art

- Confirm the project is using portrait gameplay and the URP 2D renderer.
- Import the graveyard board and the two enemy sheets.
- Repair enemy sheet slicing to exact `256 x 256` cells.
- Create folders for `GameplayData`, `Prefabs`, `Materials`, `Shaders`, `VFX`, `UI`, and `Audio`.
- Set mobile texture compression, disable unnecessary mipmaps on UI/sprites, and verify all sprites have readable pivots.
- Create a single `SoulCrystalDefenseGameplay` scene and one `Level01_ForgottenGraveyard` data asset.

Deliverable: the scene opens in portrait with the board, five lanes, top portal anchors, and bottom crystal anchor visible.

### 00:20–01:15 — Build the complete game loop

- Implement the five-lane/eight-row board grid.
- Implement touch selection, placement preview, valid/invalid tile states, and occupied-tile protection.
- Implement Soul Energy, defender costs, enemy rewards, and crystal health.
- Implement enemy movement from top to bottom and defender targeting from bottom to top.
- Implement damage, attack cooldowns, death, crystal breach, victory, defeat, and retry.
- Use pooled enemies/projectiles only after the first interaction path is working.

Deliverable: a player can place a Dwarf or Chef, defend the crystal, win, lose, and retry without leaving the scene.

### 01:15–02:10 — Implement Wave 1 and progression hooks

- Create the exact Wave 1 spawn table in this document.
- Add the two-second countdown, wave progress, spawn telegraphs, and late Soul Reaper introduction.
- Add first-clear reward: one crystal fragment, coins, and stars.
- Save progress locally with a versioned save object; never store progress only in scene state.
- Add the disabled hooks for later Elephant boss and War Train ultimate cards.

Deliverable: a complete 40–45 second tutorial wave with a clear beginning, escalation, victory/defeat result, and persistent reward.

### 02:10–03:00 — Apply the visual package

- Add the graveyard background as a separate backdrop from the gameplay-critical objects.
- Add five portal objects, the bottom Soul Crystal shrine, lane markers, tile highlights, health bars, and compact hit effects.
- Add the Dwarf, Chef, Bone Raider, and Soul Reaper animations.
- Add the materials and shaders listed below.
- Add a restrained screen pulse on crystal damage and a short cyan reward burst on enemy death.

Deliverable: the first playthrough reads as a polished dark-fantasy game rather than debug geometry.

### 03:00–03:35 — Finish the mobile UI and reward flow

- Safe-area HUD: crystal health, `WAVE 1/1`, Soul Energy, pause.
- Bottom card tray: Dwarf, Chef, locked Elephant preview, locked Train ultimate.
- Add card cooldown, unaffordable, selected, and locked states.
- Add victory panel with stars, fragment reward, replay, and next-level button.
- Add defeat panel with retry and a short, useful failure hint.

Deliverable: the vertical slice is understandable without development labels or inspector interaction.

### 03:35–04:00 — QA, evidence, and packaging

- Run Wave 1 from a clean save.
- Test reference phone, tall iPhone, and iPad portrait layouts.
- Test placing in every lane, cancelling placement, rapid taps, pause/resume, retry, and app background/foreground.
- Confirm there are no missing sprites, console exceptions, or broken animation rows.
- Capture a Play Mode screenshot at wave start, mid-wave, Soul Reaper spawn, victory, and defeat.
- Record frame rate, memory, and texture warnings on the target device or simulator.

Deliverable: a reviewable build plus evidence that the core contract works.

## Complete 2D production package

### Runtime asset inventory

The final package should contain these asset categories:

- Four biome board packs: Forgotten Graveyard, Ember Wasteland, Frozen Tombs, Royal Soul Citadel.
- Five portal variants and portal open/spawn/close effects.
- Soul Crystal shrine, crystal damage states, crystal charge, crystal break, and reward burst.
- Dwarf, Chef, Bone Raider, and Soul Reaper animation sheets.
- Elephant boss animation sheet and charge effect.
- Train ultimate animation and lane sweep effect.
- Soul Energy icons, crystal fragment icons, star states, pause, settings, replay, and next-level icons.
- Defender cards with locked, available, selected, cooldown, and upgraded states.
- Tile highlight, invalid tile, attack range, target marker, health bar, hit flash, death dissolve, and damage-number styles.
- Five chapter foreground overlays and reusable edge decorations.

### Background construction rules

Every biome must be layered so the board remains responsive:

1. Repeatable or stretchable ground fill.
2. Five-lane board surface and subtle row seams.
3. Left and right edge decoration strips.
4. Top portal threshold strip.
5. Bottom shrine platform.
6. Optional foreground overlay with no collision or gameplay information.

Never bake portals, units, HUD, health bars, or placement highlights into a background texture.

### Mobile-friendly materials

Use URP 2D-compatible materials with as few texture samples as possible:

- Opaque background material for board art.
- Alpha-blended sprite material for units and effects.
- Additive material only for cyan soul flame, portal energy, and crystal glow.
- Use premultiplied alpha for soft glow sprites when edge halos appear.
- Avoid per-pixel screen-space effects on every unit.
- Prefer Sprite-Lit-Default for units that need the global 2D light and Sprite-Unlit-Default for UI and controlled glow.
- Keep materials shared; do not create one material instance per enemy.

### Required lightweight shaders

Create these shaders as small URP-compatible shaders or Shader Graphs. Each shader must have a simple fallback material.

1. **SoulGlow** — cyan rim/glow from sprite alpha, intensity parameter, optional pulse speed.
2. **PortalPulse** — red-to-black portal gradient with radial pulse and spawn flash.
3. **CrystalCharge** — scrolling cyan band and emission pulse based on crystal health/energy.
4. **DamageFlash** — short white/red tint controlled by a `_FlashAmount` property.
5. **DissolveDeath** — alpha cutoff driven by `_DissolveAmount` plus cyan ember edge.
6. **TileHighlight** — cyan valid state, red invalid state, and soft selected pulse.
7. **UIProgressFill** — clipped crystal health and wave progress with no mesh generation at runtime.

Shader quality rules:

- No shader should be required for gameplay correctness.
- Expose intensity, color, speed, and cutoff so level artists can tune without editing code.
- Use unscaled time for UI pulses and scaled time for gameplay VFX.
- Clamp emission values to avoid blooming the entire portrait screen.

### VFX package

- Portal spawn: 0.25-second red pulse, small smoke puff, one audio hook.
- Bone hit: tiny orange/cyan spark, maximum 0.15 seconds.
- Soul Reaper hit: cyan slash fragment, maximum 0.2 seconds.
- Enemy death: eight-particle soul burst, then dissolve; no gore.
- Crystal damage: short camera-safe pulse, crystal shake under 0.08 world units, no screen shake on every hit.
- Victory: three-stage crystal rise, star reveal, fragment fly-to-counter animation.
- Defeat: crystal dim, controlled vignette, clear retry CTA.

Use particle pooling and cap simultaneous particles. VFX must never hide the lane or a touch target.

### Audio hooks

The first slice may use placeholder clips, but the code must expose hooks for:

- Portal spawn
- Defender placement
- Dwarf impact
- Chef fire impact
- Bone death
- Soul Reaper cast
- Crystal damage
- Wave start
- Victory
- Defeat
- Crystal forge

Use short readable cues, a low-intensity combat loop, and a stronger musical cadence at wave milestones. Do not use loud random sounds on every hit.

## Core loop and ethical dopamine pacing

The intended emotional rhythm is **prepare → place → stabilize → pressure → clutch response → reward → improve**.

Use positive feedback for skill and planning, not compulsion:

- First placement gets a clear confirmation and a small cyan ring.
- The first enemy defeat gives an immediate small energy reward.
- Every wave milestone gives a visible but brief reward beat.
- Victory grants stars for clear skill conditions, not random loot.
- Crystal fragments are deterministic first-clear rewards.
- Avoid loot boxes, gambling-style randomization, manipulative timers, forced ads, and punitive energy limits.
- Never block the player from replaying a level because a timer expired.
- Reward clean play with cosmetics, stars, or optional challenge rewards rather than overwhelming power spikes.

Target pacing for a normal level:

- First meaningful decision: within 3 seconds.
- First enemy defeated: within 10–15 seconds.
- First pressure spike: around 55–65% of the level.
- Reward reveal: immediately after victory.
- New progression choice: within 20 seconds of the result screen.

## Scaling from Wave 1 to Wave 999

Do not author 999 waves by hand. Generate wave data deterministically from a versioned seed and a set of hand-authored chapter rules. A generated wave must be previewable and overridable by a small exception table for special boss or story waves.

### Wave bands

- Waves 1–5: onboarding and basic Bone Raiders.
- Waves 6–20: add Soul Reapers, lane pressure, and mild tile hazards.
- Waves 21–100: introduce elite variants, split portals, and first Elephant bosses.
- Waves 101–300: combine enemy roles and chapter modifiers.
- Waves 301–700: endless mastery range with rotating biome rules and elite compositions.
- Waves 701–998: high-skill survival, capped stat growth, stronger pattern complexity.
- Wave 999: designed finale with a fixed multi-phase boss scenario, not a random roll.

### Budget-based generator

Each enemy has a cost and threat profile:

- Bone Raider cost `1`
- Soul Reaper cost `2`
- Armored/elite variant cost `4`
- Elephant boss cost `12`

For wave index `n`, calculate a threat budget using a slow curve rather than exponential health inflation:

`budget(n) = round(8 + 2.5 * pow(n, 0.62))`

Apply caps and pacing rules:

- Maximum health multiplier: `4.5x` base health.
- Maximum normal movement multiplier: `1.6x` base speed.
- Maximum simultaneous active enemies: tuned per device and board width.
- Use additional budget for composition variety, not only larger numbers.
- Every fifth wave may add an elite modifier.
- Every twentieth wave may use a boss or special rule after Wave 20.
- Never combine a boss, fastest speed modifier, and lane-disable hazard for the first time in the same wave.

### Composition rules

The generator should choose from a small number of readable patterns:

- Rush: many Bone Raiders, low individual threat.
- Cross-lane: two lanes receive alternating spawns.
- Ranged pressure: Soul Reapers appear behind slower blockers.
- Split portals: only two or three portal lanes activate.
- Guarded boss: one boss with a predictable support group.
- Recovery wave: lower threat after a difficult wave, with better reward clarity.

The generator must enforce a minimum warning interval between new enemy types and must not spawn an uncounterable composition based on the currently unlocked defender set.

### Dynamic “brain” balancing method

The game should adapt the next wave by a small amount using local telemetry. Do not change an active wave after it starts.

Track per run:

- Crystal health at wave end
- Number of defenders placed and their lanes
- Enemy leaks and time-to-first-leak
- Soul Energy spent, earned, and unspent
- Damage dealt by each defender type
- Retry count and wave reached
- Pause/background interruptions
- Frame time and device performance

Calculate a bounded player-performance score:

`performance = 0.45 * crystalHealthPercent + 0.20 * clearSpeedScore + 0.15 * placementEfficiency + 0.10 * damageEfficiency + 0.10 * recoveryScore`

Then keep an exponential moving average across recent waves:

`skillEma = lerp(previousSkillEma, performance, 0.18)`

Choose the next budget multiplier only inside a safe band:

- If skill EMA is above `0.82` for two waves, increase next budget by 3–5%.
- If skill EMA is below `0.42` or the player leaked more than 35% crystal health, reduce next budget by 5–8%.
- Otherwise keep the planned budget.
- Never reduce a wave below its authored tutorial floor.
- Never increase more than 5% from one wave to the next.
- Persist the seed and applied multiplier so replays are reproducible.

Balance targets after playtesting:

- Tutorial Wave 1 completion: 80–90% of first-time players.
- Normal wave completion: 65–85%.
- Average retries before first chapter completion: 1–3.
- Average crystal health at normal victory: 35–70%.
- Immediate quit after a defeat: under 25%.
- Frame rate: stable target-device frame pacing with no VFX spikes.

If metrics leave these bands, adjust enemy composition, telegraph time, rewards, or defender clarity before increasing raw enemy health.

## App Store polish and release gate

The concept can be polished enough for internal App Store review, but no document can guarantee Apple or Google approval. Final approval depends on actual builds, metadata, privacy declarations, device behavior, and platform review.

Before submission, require:

- No placeholder debug labels, missing sprites, console exceptions, or development menus.
- Correct portrait orientation declarations for iPhone and iPad.
- Safe-area behavior for notches, Dynamic Island, home indicators, and iPad multitasking sizes.
- No cropped portals, crystal, cards, or touch targets at supported aspect ratios.
- Readable text at the smallest supported phone size.
- Pause/resume and background/foreground recovery.
- Deterministic local save with version migration and no data loss on update.
- Offline core gameplay unless a clearly disclosed online feature is added later.
- Appropriate privacy manifest, permission prompts, privacy policy, age rating, and data-safety declarations.
- No misleading store screenshots or claims that are not present in the build.
- No gambling-like monetization or pay-to-win progression in the first slice.
- Texture compression and memory review on a real older supported device.
- Cold launch, scene transition, and retry performance checks.
- Accessibility review for text contrast, touch target size, motion intensity, and color-only feedback.
- Store screenshots in portrait showing real gameplay, not only concept art.

## Final four-hour handoff output

At the end of the sprint, Antigravity should return:

- One playable portrait gameplay scene.
- One `Level01_ForgottenGraveyard` data asset.
- One deterministic `Wave01_TutorialPressure` data asset.
- Two correctly sliced enemy sheets and prefabs.
- Dwarf and Chef defender prefabs.
- Crystal, portal, board, HUD, cards, victory, and defeat visuals.
- Materials/shaders with safe fallbacks.
- Pooled or pool-ready VFX hooks.
- Local save/reward hook for one crystal fragment.
- A short QA report with screenshots at wave start, mid-wave, Soul Reaper spawn, victory, defeat, iPhone ratio, and iPad ratio.
- A list of known visual or balance issues; do not hide unfinished work behind the concept image.
