# Production art review — 21 October 2026

Five alternate assets, generated with the built-in image tool from the current game artwork. They are intentionally not assigned to the live scenes: compare them before replacing the approved art.

1. `01-Sunstone-Path.png`: warm sandstone, yellow highlights and coral/red seams.
2. `02-Moss-Slate-Path.png`: blue-gray slate with emerald moss and restrained sunlit edges.
3. `03-Amethyst-Path.png`: muted purple paving with dark-pink edge lighting.
4. `04-Soul-Defence-Home-1080x1920.png`: refined moonlit home illustration; same cast, castle, elephant, train and crystal composition.
5. `05-Battlefield-1080x1920.png`: full-height battlefield background with five portals, open lawn and the existing base location. Ground patches remain separate sprites.

The three path sprites preserve the broad rectangular footprint and irregular paving idea. They have genuine alpha around the perimeter. They are individual placement patches, not mathematically seamless repeat textures; narrow grass/grout between cells is intentional. Point filtering preserves the pixel clusters. Home art uses bilinear filtering. Raw portrait generations are retained under Sources; portrait delivery exports are exactly 1080 x 1920.

When selecting a path option, replace only the sprite on each existing StonePatch renderer, then retain its authored world size of 1.29 x 0.94 and center it on the existing grid cell. Do not resize or move the gameplay cells, portals, spawn positions or attack ranges to fit a new texture. The home illustration and battlefield backdrop require their own preview before scene assignment because generated art may have small composition differences.

Open `Review.html` for an interactive five-lane comparison of the old path and the three new alternatives. The portrait art can also be opened at full size there.

Exact prompts and generation mode: `Generation-Prompts.json`. Dimensions and formats: `Art-Manifest.json`. Unity imports use uncompressed editor previews and an iOS ASTC 4x4 override at maximum 2048 pixels. The original PNG deliveries retain full quality.

Sound polish is implemented in `Assets/SHAMSA/Scripts/Systems/SoundManager.cs`: effects gain 0.65 and music gain 0.85 multiply the player's saved settings. Procedural fallback sounds use a short smooth attack/release and a 2.8 kHz low-pass stage. Repeated identical sounds are spaced by 60 ms and dense action receives gentle gain reduction. Volume/mute choices remain under player control.

Editor findings and limits are recorded in `ArtReview/ProductionArt/Verification.md`. These assets and editor checks do not establish App Store submission readiness; final iOS build, device framing/touch, speaker/headphone listening and submission checks remain necessary.
