# Crimson Settings and How to Play

These are two visual design references, generated with the built-in image generator from the approved character combat art, main-menu background and title treatment. Each delivery is 1080 x 1920. Original generated images are retained in `Sources/`; exact prompts are in `Generation-Prompts.json`.

## Current panel audit

The current unsaved MainMenu scene was inspected without switching, saving or editing it. Both panel roots are 1080 x 1920. Their existing surface is 900 x 1450, title region 800 x 120 and close button 700 x 130. Current Settings sliders are 700 x 80 with separate labels. How to Play currently uses one 760 x 920 instruction text region.

Gameplay's saved UI uses a 900 x 1290 Settings frame with three 700 x 58 sliders, and a 960 x 1510 tutorial frame. These fixed portrait measurements need proportional anchoring and footer spacing for the shorter logical canvas on a portrait iPad. A full-size phone mockup alone does not validate the iPad scene layout.

## Designs

- `Settings-Design-1080x1920.png`: Dwarf/Chef character header; charcoal stone and gold trim; three distinct Master, Music and Sound Effects rows; red/orange fill and round ivory/gold slider knobs; saved-preference hint and Back button.
- `HowToPlay-Design-1080x1920.png`: Archer/Paladin header; four short illustrated steps for choosing a card, deploying to a tile, automatic attacks and defending the crystal; regeneration hint and Back button.

The references intentionally retain the existing functions. Header combat art provides the requested character atmosphere; matte dark areas keep functional text legible.

## Separate art needed for editable Unity implementation

| Element | Use |
| --- | --- |
| Plain squared stone/gold panel frame, with a calm empty center | Shared nine-sliced modal surface |
| Two text-free character header strips | Settings Dwarf/Chef and How to Play Archer/Paladin |
| Thin gold/dark row frame | Audio rows and instruction steps |
| Slider track, separate red/orange fill, separate round gold/ivory knob | Native interactive Unity Sliders |
| Speaker, music-note and effects icons | Audio row identification |
| Blank crimson/gold rectangular button | Existing Back/Close callbacks |
| Plain numbered badge base | Four independent TMP step numbers |
| Four text-free gameplay vignettes | Card selection, placement, attack and crystal defense |

All labels, percentages, headings, step numbers and button text should remain separate TextMeshPro objects in the eventual scene, using the prepared PAYBACK font and auto-sizing. The text shown in these images is part of the visual reference and should not become a single baked interactive panel.

Use saved RectTransform anchors and editable children for phone/iPad fitting. Do not introduce a runtime panel-construction or layout MonoBehaviour. Existing button, slider and navigation references must be retained. These references have not been assigned over the user's current scene.
