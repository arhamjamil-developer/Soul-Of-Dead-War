# Neighbor homes and collapsible battle UI

Installed in `Assets/Scenes/SoulCrystalDefenseGameplay.unity`.

## Art

- `Guardian-Cottage.png`: decorative home to the left of the battlefield.
- `Chef-Cottage.png`: decorative home to the right, with an outdoor cooking nook.
- `../Collapse UI/Arrow-Up.png` and `Arrow-Down.png`: transparent blue-and-gold arrow plates, with separate pressed sprites.
- `Generation-Prompts.json`: exact prompts for the built-in image generator; reference-based generation. No Runway used.

Neighbor sprites are ordinary editable SpriteRenderers under `NeighborHomes`. Each spans the same 9.5 by 17.65 world-unit footprint as the current backdrop. They create no deployment cells. The camera confiner includes all three views. Background edges retain independent illustration detail, rather than being mathematically tiled textures.

## Interaction

The top arrow hides/restores the top HUD; the bottom arrow independently hides/restores the tray. Both slide and fade over 0.38 seconds, using unscaled time. Restore handles remain at the safe canvas edges. Floating pause and a crystal BATTLE return control remain available while the corresponding panels are hidden. These controls yield to pause/settings/how-to-play overlays and the first-wave tutorial.

A horizontal swipe across at least 18% of the viewport, with horizontal movement greater than 1.4 times vertical movement, changes one neighboring page. Smaller drags retain existing free camera panning. Swipe from open world space with no guardian selected. Selecting a guardian while viewing a neighbor returns to the battlefield. The BATTLE crystal also returns immediately. Combat continues while exploring.

## Editable layout

`HUDCanvas / SafeAreaRoot / GameplayComposition / HUDExploreControls` contains saved GameObjects and native Buttons, Images and PAYBACK TextMeshPro labels. All references are inspector-editable on `CollapsibleBattleHUD`.

At the 1080 by 1920 reference layout, the top parent is approximately 232 units high and the bottom tray is 580. The top artwork's children extend below the parent; full descendant bounds determine travel (282 top, 604 bottom), including a 24-unit clearance. Bounds recalculate when the safe canvas changes size. Each button has a 180 by 136 logical-unit hit area and 168 by 76 visible arrow artwork.

The authoring menu is `SoulCrystalDefense / Presentation / Install Collapse HUD and Neighbors`. It recreates these new controls and neighbor sprites, so use it deliberately after custom edits. Runtime scripts do not construct UI.

Focused runtime evidence is saved under `ArtReview/CollapsibleHUD`. Native touch feel and device safe areas still need an iPhone/iPad device pass before release.
