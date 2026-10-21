# North and south world art

Two reference-based images generated with the built-in image generator:

- `North-Royal-Garden.png`: 2172 x 724 royal garden and blue castle courtyard above the battlefield.
- `South-Village-Approach.png`: 2164 x 727 village approach, open meadow and stone paths below it.

Exact prompts and source reference are saved in `Generation-Prompts.json`. The source battlefield image and current grid textures remain assigned.

Both images are bound to independent SpriteRenderers under `NorthSouthWorldArt` in `SoulCrystalDefenseGameplay.unity`. They span all three horizontal world pages. The camera confiner includes both extensions and the two neighboring homes. The joins use a 0.9-world-unit overlap and editable `WorldArtJoin` materials with a soft alpha edge. These are separate illustrations with soft joins, rather than mathematically seamless tile textures.

`BattlefieldCameraFraming` on the Cinemachine camera computes a battle envelope from the five-by-eight grid, enemy entry and crystal. It measures actual HUD bounds in screen space and fits this envelope into the remaining safe viewport. Hiding panels smoothly permits a closer full-grid view. The grid itself does not move or scale.

Dragging, zooming and visiting neighbors release automatic battle framing. A selected guardian or the BATTLE crystal restores it. Neighbor visits use a closer exploration zoom. Top/bottom arrows are docked to the right to avoid covering the central lane. Existing combat continues during exploration.

Editor authoring menu: `SoulCrystalDefense / Presentation / Install North South Art and Grid Framing`. This deliberately rebuilds the two extension sprites and rebinds the camera component; avoid rerunning after custom scene edits unless intended.

Verification evidence is in `ArtReview/WorldExtensions`: actual Game View screenshots, full-board viewport checks at 1080 x 1920 and 1536 x 2048 with panels open and hidden, return-to-battle checks, and north/south camera checks. Native iOS touch and device safe-area validation remain a device testing step.
