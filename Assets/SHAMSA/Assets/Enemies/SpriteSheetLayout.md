# Enemy Downward Animation Sheets

Both sheets are transparent `2048 x 768` PNG files arranged as an `8 x 3` grid.

## Unity slicing

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Multiple`
- Mesh Type: `Full Rect`
- Filter Mode: `Bilinear`
- Compression: choose the appropriate mobile format for the target platform
- Sprite Editor > Slice > Grid By Cell Size
- Pixel Size: `256 x 256`
- Offset: `0 x 0`
- Padding: `0 x 0`
- Pivot: `Bottom Center`

## Row map

Unity's texture coordinates begin at the bottom, so visual rows and imported ordering may appear reversed in the Sprite Editor.

- Visual top row: downward walk loop, 8 frames
- Visual middle row: downward attack sequence, 8 frames
- Visual bottom row: downward death sequence, 8 frames

Recommended playback:

- Walk: loop, 10-12 frames per second
- Attack: no loop, 12-16 frames per second
- Death: no loop, 10-12 frames per second; hold the final frame

## Files

- `BoneRaider/BoneRaider_Down_Animations.png`
- `SoulReaper/SoulReaper_Down_Animations.png`

These sprites face screen-bottom because enemies travel from the top of the portrait board toward the player's crystal at the bottom.
