# First-wave tutorial: art review stage

Status: artwork preparation only. Tutorial scene, prefabs, and gameplay logic have not been implemented in this stage.

## Art direction

Reusable cobalt/navy dialogue panel with a rounded gold border and small cyan crystal accents. Keep the interior blank. Unity TextMeshPro will render the actual tutorial title and instructions. Keep character portraits, text, progress indicators, highlights, and buttons as separate editable objects.

Reuse the existing portraits in `Assets/UI/Final/Production/Portraits`: Dwarf, Archer, Chef, Mage, Paladin, and Witch. Do not paint new versions into the production dialogue frame. The same frame should support all guide characters.

## Proposed first-run flow

1. Guardian introduction: protect the soul crystal; show a short welcome before the wave timer starts.
2. Select a guardian: highlight the existing guardian card and wait for that selection.
3. Place a guardian: highlight a valid center-lane patch and wait for successful placement. Do not advance on an invalid tap or failed purchase.
4. Archer guidance: explain lane coverage and soul costs; allow the first scout to approach and continue from real combat/placement events.
5. Expand defense: show a short prompt for the adjacent lane before its scheduled threat. Keep the tutorial panel away from cards and placement targets.
6. Clear wave one: wait for the actual wave-complete event. A timer or Next button must not count as a clear.
7. Completion: record tutorial completion and transition into the existing endless run. Offer Skip and a replay entry in How To Play; skipping must restore normal game/input state.

All step prompts are editable TMP text, with bounded autosizing and safe margins. Tutorial pauses must release cleanly on skip, scene changes, retry, and completion. Store tutorial completion only after successful completion or an intentional skip.

## Current project findings

- Existing scene has `useProceduralProgression: 1` and `unlimitedWaves: 1` on GameManager.
- GameplayHUD already displays `WAVE n` when total waves is negative.
- Wave one has six scouts: center at 7 seconds, adjacent lanes at 19 and 30 seconds, then a center/adjacent finish at 40, 46, and 46.8 seconds.
- Integration still needs a first-run tutorial controller, event wiring, editable prefab authoring, replay/skip handling, and Play Mode verification past wave 20.

## Generation

Generated using the built-in image generation tool. The exact production frame prompt is saved alongside this document. The border PNG is an art asset; any illustrated review mockup is a concept, not a Unity screenshot or functional UI.
