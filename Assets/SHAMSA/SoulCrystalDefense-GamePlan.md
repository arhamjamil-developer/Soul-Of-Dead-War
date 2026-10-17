# Soul Crystal Defense — Game Plan

## Core idea

A polished portrait 2D top-down lane-defense game. Enemies enter through five portals at the top of the screen and walk downward toward the Soul Crystal at the bottom. The player spends energy to place defenders on clear square tiles; defenders face and attack upward. A level contains multiple waves and ends with a boss or special enemy.

The inspiration is the immediate readability of a classic lane-defense game, but the identity is dark-fantasy warriors, soul crystals, active combat abilities, and permanent crystal progression between levels.

## One-minute gameplay loop

1. A short preview shows which portals and enemy types will be active.
2. The player earns Soul Energy automatically and from defeated enemies.
3. Tap a defender card, then tap a free tile to deploy it.
4. Defenders automatically attack enemies in their lane or range.
5. Survive all waves before enemies destroy the finish crystal.
6. Earn coins, crystal fragments, and one upgrade choice.
7. Use the completed crystal to unlock the next level and a new mechanic.

## Board rules

- Portrait 9:16 orientation with a near-vertical top-down camera.
- Five vertical lanes and approximately eight placement rows.
- Enemy start: one portal at the top of each lane.
- Player finish: one shared Soul Crystal shrine at the bottom.
- Player attacks and abilities travel from bottom to top; enemies advance from top to bottom.
- Fixed placement tiles create clean touch controls and predictable balance.
- Enemies normally stay in their lane; special enemies can jump, charge, summon, or disable a tile.
- If an enemy reaches the finish, it damages the Soul Crystal. Zero crystal health loses the level.

## Starting characters from the existing sprite sheets

- **Dwarf Guardian:** high health, short range, blocks enemies, hammer knockback ability.
- **Blood Chef:** medium range, throws fire or burning oil, splash damage, lower health.
- **Armored Elephant:** boss enemy with high health; charges through the first blocker and damages nearby tiles.
- **War Train:** later unlockable ultimate card; crosses one selected lane once and damages every enemy in it.

Additional basic enemies and defenders will need their own sprite sheets, but they should match the existing painted top-down style.

## Crystal progression

Crystals are permanent chapter rewards, not ordinary spending currency.

- Each completed level awards one crystal fragment.
- Three fragments forge one full crystal.
- A forged crystal unlocks a new gameplay system for the following levels.
- Crystals remain visible in the player's Crystal Hall so progression feels collectible.

Suggested crystal sequence:

1. **Azure Crystal:** unlocks the second defender slot and a small starting-energy bonus.
2. **Ember Crystal:** unlocks burn effects and the Chef's upgraded attack.
3. **Iron Crystal:** unlocks armor, barricades, and the Dwarf's stronger block.
4. **Storm Crystal:** unlocks faster cooldowns and chain-lightning units.
5. **Crown Crystal:** unlocks the War Train ultimate and the final chapter.

## Level structure

Each chapter contains five normal levels and one boss level. A level should last about three to five minutes and contain four to six waves.

- **Chapter 1 — Forgotten Graveyard:** teaches placement, blockers, and basic melee enemies.
- **Chapter 2 — Ember Fields:** burning ground tiles and ranged enemies.
- **Chapter 3 — Frozen Tombs:** slow effects and armored enemies.
- **Chapter 4 — Royal Ruins:** multiple active portals and enemies that change lanes.
- **Chapter 5 — Soul Citadel:** combined mechanics, elite waves, and final bosses.

Ground art changes per chapter, while the five-lane board silhouette and placement grid remain consistent.

## Wave variety

- Rush wave: many weak enemies.
- Shield wave: slow armored enemies.
- Ranged wave: enemies attack defenders from a distance.
- Hazard wave: selected tiles become temporarily unusable.
- Split wave: only two or three portals activate, then the pattern changes.
- Boss wave: one large enemy supported by smaller units.

## Meta progression

- Coins upgrade defender statistics within safe limits.
- Crystal milestones unlock new mechanics and chapters.
- Stars reward clean victories: finish health, speed, and optional challenge.
- Three stars on a level grant a bonus fragment or cosmetic reward.
- Earlier levels can be replayed for coins, stars, and challenge objectives, but crystal fragments are first-clear rewards to prevent grinding from breaking progression.

## Visual production breakdown

### 1. Battlefield packs

- Five-lane ground board for each chapter.
- Start portals, finish shrine, placement-tile states, lane borders, and ground hazards.
- Separate background, board, decoration, and foreground-overlay layers for parallax and reuse.

### 2. Gameplay sprites

- Defender idle, attack, hit, and defeat sheets.
- Enemy walk, attack, hit, and defeat sheets.
- Boss entrance and special-attack sheets.
- Projectiles, impacts, status effects, portal effects, and crystal-damage effects.

### 3. Gameplay UI

- Crystal health, wave progress, energy counter, pause button.
- Defender cards: available, selected, cooldown, unaffordable, locked, upgraded.
- Placement highlight, invalid placement, unit range preview, and sell/upgrade popover.
- Victory, defeat, pause, tutorial, and reward panels.

### 4. Progression UI

- Level map with stars and chapter gates.
- Crystal-forging screen and Crystal Hall.
- Defender collection and upgrade screen.
- Daily reward, settings, and shop only after the core game is fun.

## First playable scope

Build one complete vertical slice before producing all chapters:

- One graveyard board.
- Five lanes by eight columns.
- Dwarf and Chef defenders.
- Two basic enemies plus the Elephant boss.
- Five waves.
- Energy, placement, combat, crystal health, victory, and defeat.
- One crystal-fragment reward screen.

The vertical slice is successful when it is fun and readable on a phone without zooming. After that, produce the remaining visual packs screen by screen.

## Approved visual target

Primary portrait target:

`Assets/SHAMSA/Concept/Portrait/PortraitGameplay-VisualTarget.png`

Clean environment targets:

- `Assets/SHAMSA/Concept/Portrait/01-ForgottenGraveyard-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/02-EmberWasteland-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/03-FrozenTombs-CleanBoard.png`
- `Assets/SHAMSA/Concept/Portrait/04-RoyalSoulCitadel-CleanBoard.png`

The concept establishes layout and visual direction only. Production assets must be delivered as separate transparent sprites, tileable boards, VFX sheets, and UI elements suitable for Unity.
