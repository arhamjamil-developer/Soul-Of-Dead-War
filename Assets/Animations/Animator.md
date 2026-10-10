# Soul of Dead War — Top-Down Animation Master Specification
**Document**: `Animator.md`  
**Location**: `Assets/Animations/Animator.md`  
**Role**: Lead Unity 2D Animator & Technical Artist  
**Engine & Pipeline**: Unity 6 (6000.3.x) — Universal Render Pipeline (URP 2D)  
**Perspective**: Pure 2D Top-Down View ("Top View" Standard)  
**Master Source Asset**: `Assets/Animations/MasterSpriteMatrix.jpg`  

---

## 1. Master Sprite Architecture & Quality Standard

Based on the master production matrix (`MasterSpriteMatrix.jpg`), all four central combat archetypes—**Train**, **Barbarian**, **Elephant**, and **Chef**—are rendered in pure top-down perspective with high-end painted dark fantasy aesthetics: volumetric lighting, golden brass highlights, deep cobalt shadows, and visceral blood decals.

```
       +-------------------------------------------------------------------------+
       |               MASTER TOP-DOWN PRODUCTION MATRIX OVERVIEW                |
       |  Viewing Angle: Pure Top-Down Overhead View                             |
       |  Render Pipeline: Universal Render Pipeline (URP 2D Lit / Unlit)        |
       +-------------------------------------------------------------------------+
                                            |
        +-----------------------------------+-----------------------------------+
        |                                                                       |
  [ CORE ANIMATION STRIP ]                                             [ DEDICATED VFX OVERLAYS ]
  - IDLE: 1 Hero Frame                                                 - TRAIN: Fire Jet & Smoke Plume
  - WALK: 4 Balanced Frames (8 FPS)                                    - BARBARIAN: Slash Arc & Impact Spurt
  - RUN: 4 Sprint Frames (12 FPS)                                      - ELEPHANT: Ground Smash & Dust Wave
  - ATTACK: 4 Strike Frames (10 FPS)                                   - CHEF: Fire Skillet Swing & Sparks
  - HURT: 2 Recoil Frames (8 FPS)
  - DEAD: 4 Collapse Frames (6 FPS)
```

---

## 2. Character Matrix Breakdown

### 2.1 Character 1: The Iron Juggernaut (Train)
* **Visual Profile**: Cobalt-blue armored steam locomotive chassis with brass rivets, gold crown badge, glowing round boiler headlight, smokestack billowing dense smoke.
* **Idle (1 Frame)**: Top-down vertical locomotive view with steady smoke plume.
* **Walk (4 Frames | 8 FPS)**: Piston stroke cycle with gentle forward chassis pulse.
* **Run (4 Frames | 12 FPS)**: High-speed vibration with exhaust trails and spinning wheels.
* **Attack (4 Frames | 10 FPS)**: Boiler charge surge with front cowcatcher ram thrust.
  * **Attack VFX**: `Train Fire` (concentrated forward flame jet).
* **Hurt (2 Frames | 8 FPS)**: Chassis lateral derail shudder with white hit-flash.
  * **Hurt VFX**: `Train Smoke` (concussive black-and-grey steam blowout).
* **Dead (4 Frames | 6 FPS)**: Catastrophic boiler breach, wheel derailment, and charred smoking wreckage.

---

### 2.2 Character 2: The Hammer Berserker (Barbarian)
* **Visual Profile**: Muscular warrior with spiky red hair and beard, eyepatch bandages, fur collar, leather harness, crimson war kilt, two-handed stone war-hammer.
* **Idle (1 Frame)**: Grounded stance with hammer resting forward, ready to strike.
* **Walk (4 Frames | 8 FPS)**: 4-phase footwork with hammer weight balance and shoulder sway.
* **Run (4 Frames | 12 FPS)**: Forward-charging sprint with hammer trailing and red hair flowing.
* **Attack (4 Frames | 10 FPS)**: Overhead hammer hoist, downward smash, and ground crater strike.
  * **Attack VFX**: `Barbarian Slash` (crescent golden arc trail).
* **Hurt (2 Frames | 8 FPS)**: Violent torso recoil with head snap and blood flash.
  * **Hurt VFX**: `Barbarian Impact` (crimson electric blood splatter).
* **Dead (4 Frames | 6 FPS)**: Dropping hammer, knees buckling, and collapsing face-down into the dirt.

---

### 2.3 Character 3: The Royal War Behemoth (Elephant)
* **Visual Profile**: Massive dark blue elephant adorned in royal blue heraldic armor bards, gold crown sigils, blood-soaked wrapped tusks, gold toe guards, swinging trunk.
* **Idle (1 Frame)**: Imposing overhead bulk with trunk swaying forward.
* **Walk (4 Frames | 8 FPS)**: Lumbering 4-footed heavy gait with swinging gold tassels.
* **Run (4 Frames | 12 FPS)**: War stampede charge with flared ears and raised head.
* **Attack (4 Frames | 10 FPS)**: Head plunge, tusk goring into the earth, and wide trunk sweep.
  * **Attack VFX**: `Elephant Ground Smash` (massive rock-erupting crater).
* **Hurt (2 Frames | 8 FPS)**: Armored flinch with trembling golden caparison.
  * **Hurt VFX**: `Elephant Dust` (dense beige impact shockwave).
* **Dead (4 Frames | 6 FPS)**: Colossal side tilt, collapse, and static behemoth barrier.

---

### 2.4 Character 4: The Mad Battle Gourmet (Chef)
* **Visual Profile**: Dwarven proportions, white toque blanche, scarred face with bandages, bloody white apron, blue trousers, cast-iron skillet, alchemical potion flask.
* **Idle (1 Frame)**: Bouncing ready stance with pan raised and potion bottle at hip.
* **Walk (4 Frames | 8 FPS)**: Wobbly brisk stride with bobbing chef hat.
* **Run (4 Frames | 12 FPS)**: Low manic scurrying sprint with flapping apron.
* **Attack (4 Frames | 10 FPS)**: Potion splash toss, 360 skillet swing, and gong impact slam.
  * **Attack VFX**: `Chef Fire Swing` (blazing fiery frying pan streak).
* **Hurt (2 Frames | 8 FPS)**: Hat pops up in surprise, backward stagger with pan clatter.
  * **Hurt VFX**: `Chef Sparks` (explosive yellow-orange chemical sparks).
* **Dead (4 Frames | 6 FPS)**: Flask shatter, mid-air spin, and prone faceplant on the ground.

---

## 3. Dedicated VFX Elements Library

Located in `Assets/Animations/MasterClips/VFX_Elements/`:
1. `Train_Fire.png`: Horizontal flame jet for train ram attack.
2. `Train_Smoke.png`: Heavy billowing puff for boiler damage.
3. `Barbarian_Slash.png`: Wide gold crescent slash trail for hammer swings.
4. `Barbarian_Impact.png`: Radial crimson burst for heavy blunt hits.
5. `Elephant_Smash.png`: Concussive debris eruption for ground slams.
6. `Elephant_Dust.png`: Expanding dust ring for stampedes.
7. `Chef_Fire_Swing.png`: Flaming arc with embers for frying pan attacks.
8. `Chef_Sparks.png`: Radial alchemical flash for potion impacts.

---

## 4. Unity Playable Demo Architecture

The interactive playtest environment is managed by `PlayableDemoManager.cs`:

### 4.1 Controls & Shortcuts
* **`[W, A, S, D]` / `[Arrow Keys]`**: 8-way top-down directional movement.
* **`[Left Shift]`**: Hold to switch from Walk ($3.2\text{ m/s}$) to Run ($6.4\text{ m/s}$).
* **`[Space]` / `[Mouse 0]`**: Trigger Attack animation + spawn dynamic character-specific Attack VFX.
* **`[H]`**: Trigger Hurt animation + Hit-Flash + Hurt VFX impact.
* **`[K]`**: Trigger fatal collapse (Dead) or revive back to Idle.
* **`[O]`**: Toggle golden silhouette outline on/off.
* **`[1, 2, 3, 4]` or `[Tab]`**: Instantly switch active character (Train, Barbarian, Elephant, Chef).

### 4.2 Camera & Rendering
* **Smooth Tracking**: Main Camera tracks the active character with a soft lerp ($8.0\text{ speed}$).
* **URP 2D Light**: Global Light 2D illuminates painted textures with zero dark crushing.
* **Sorting**: Pivot base sorting prevents foreground overlap artifacts.
