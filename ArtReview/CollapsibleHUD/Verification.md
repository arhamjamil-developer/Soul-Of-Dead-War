# HUD collapse and neighbor art verification

Verified in Unity 6000.3.13f1, gameplay scene reloaded from disk before Play Mode.

| Layout | Actual Game View | Result |
| --- | --- | --- |
| Portrait phone | 1080 x 1920 | 16/16 focused runtime checks passed |
| Portrait iPad | 1536 x 2048 | 16/16 focused runtime checks passed |

Evidence is stored separately in `Phone/` and `Tablet/`: runtime JSON, hidden HUD, left cottage, right cottage, restored HUD screenshots. The root screenshots and Runtime-QA.json contain the most recent tablet pass.

The checks cover saved references, measured panel clearance, button event callbacks, restored arrow sprites, hidden raycast release, retained pause/return controls, animation during pause, pause-menu layering, both camera destinations, unchanged 40-cell deployment grid, selection returning to battle, rapid toggles, and restored input. Actual EventSystem raycasts at the phone handle centers also returned `TopHUDHandle` and `BottomTrayHandle` respectively.

The focused camera run temporarily suspends Unity Input System devices to prevent unrelated live pointer/keyboard input from changing automated destinations. It restores them on completion, interruption, or failure. It exercises saved button callbacks and camera navigation, not a physical touchscreen swipe.

New scene objects are editable under `NeighborHomes` and `HUDCanvas/SafeAreaRoot/GameplayComposition/HUDExploreControls`. The original board art remains assigned. Neighbor illustrations are decorative and contain no placement cells.

Physical iPhone/iPad touch feel and native safe areas remain device QA items. This evidence does not certify App Store submission readiness.
