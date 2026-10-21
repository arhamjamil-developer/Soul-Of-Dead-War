# Complete-grid camera and world extensions

The gameplay scene now has separate north and south scenery sprites, an expanded world confiner, and an inspector-editable `BattlefieldCameraFraming` component on the Cinemachine camera. The original five-by-eight grid and existing gameplay coordinates remain unchanged.

The focused Play Mode check projects the battle envelope through the actual rendered camera and compares it against the screen region between the live HUD panels. It covers:

- 1536 x 2048 portrait iPad, both panels open and both hidden.
- 1080 x 1920 portrait iPhone, both panels open and both hidden.
- North and south camera destinations, with rendered screenshots.
- Returning to battle after exploring, and retention of all 40 deployment cells.

`Runtime-QA.json` contains the latest results. Screenshots named `iPad-Grid-*` and `iPhone-Grid-*` show the actual layouts. `North-Royal-Garden-Ingame.png` and `South-Village-Ingame.png` show the extension artwork through the game camera. Source art and prompts are under `Assets/UI/Production art/World Extensions`.

The automated run temporarily suspends Unity Input System devices to prevent unrelated pointer or keyboard movement from changing its destinations, then restores them. It verifies camera navigation programmatically; physical iOS swipe feel and device safe-area behavior require device QA.

The new backgrounds use separate illustrations with soft overlapping alpha joins. They are not mathematically tiled textures or an App Store certification claim.
