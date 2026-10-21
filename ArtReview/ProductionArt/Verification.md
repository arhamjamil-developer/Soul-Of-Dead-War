# Production art and soft-audio pass

21 October 2026. Art is a review package, not a replacement already deployed in the live scenes.

## Delivered and inspected

- Five generated images saved under `Assets/UI/Production art`; all five outputs visually inspected.
- Three transparent path sprites with crisp pixel-style texture, preserving the existing irregular paving idea.
- Paths have RGBA alpha spanning 0 to 255. Home and battlefield deliveries are exactly 1080 x 1920 RGB, with no transparent outer frame or baked interface.
- Source portrait generations retained; exact prompts saved. Exports only resampled for the requested portrait dimensions.
- Comparison gallery preserves five lanes and eight rows for artwork review. It is an illustrative composition, not a gameplay screenshot.
- Original scene patch sprite references and their 1.29 x 0.94 world footprint were inspected and retained. Active scene artwork has not been switched to the alternatives.
- Point filtering for pixel-style paths/backdrop, bilinear filtering for home art. iOS ASTC 4x4 overrides saved for the five optional assets.

## Audio verified in Unity

- Compilation passed with zero errors.
- Fixed missing music/effects AudioSource references. Proper editable sources saved in the gameplay scene, plus safe runtime fallback for scenes without them.
- Play Mode measured effects volume 0.65 at saved effects setting 1.0, and music volume 0.595 at saved music setting 0.7, matching the intended 0.65 / 0.85 production gains.
- Procedural fallback waveforms now have an 8 ms smooth attack, 25 ms release, low-pass smoothing at 2.8 kHz and reduced amplitude.
- Repeated identical cues have a 60 ms minimum spacing; dense cue playback receives gradual gain reduction.
- Final console check returned zero errors after the source fix. Saved player volume preferences were retained.

## Release limits

The new artwork is ready to compare and select. It has not yet been assigned to the live menu or battlefield, so a seamless installed game view using these alternatives is not claimed. Individual path sprites use intentional grassy/grout gaps; they are not mathematically repeating seamless textures.

The editor returned to edit mode during longer gameplay verification, so no new final gameplay screenshot or physical device pass is claimed here. The long-running Guardian combat check was not completed in this pass. Listen to the softer mix on actual phone speakers/headphones before release.

Still required before claiming App Store readiness: selected-art binding and camera alignment checks, actual iPhone/iPad touch/safe-area/performance verification, final signed iOS build testing, and submission review. No deployment or store submission was performed.
