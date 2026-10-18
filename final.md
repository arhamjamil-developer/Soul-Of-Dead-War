# Soul Crystal Defense — authored visual and gameplay specification

Updated 19 October 2026. This file describes the saved Unity implementation and remaining release work. Earlier claims of complete production readiness are superseded by the verification notes below. Backup: `ArtReview/PremiumVisuals/final-before.md`.

## Visual direction

Match the supplied portrait gameplay reference: blue soul crystal, dark graveyard, warm gold frames, cyan magic, purple enemy portals, and readable five-lane combat. Preserve clear playable space between the elaborate borders. The supplied reference is saved at `Assets/UI/Extra Ui Images/Gameplay-Visual-Reference.jpeg`.

The actual game uses separate sprites and editable UI objects. Text stays editable; portraits do not replace animated battlefield characters. Archer and Mage now use rear-facing miniature animation frames, consistent with allies advancing toward the portals. Enemies face the crystal. Artwork is generated concept/production-candidate art and requires final visual review at device size.

## Saved assets and manual editing

- `Assets/UI/Premium/Frames`: six separate panel, banner, button, card, slider and selection sprites. Frames use sliced Images.
- `Assets/UI/Premium/Icons`: sixteen separate icons.
- `Assets/UI/Premium/Portraits`: six guardian portraits.
- `Assets/UI/Premium/Supports`: elephant/train portraits and battlefield sprites.
- `Assets/UI/AnimationSheets`: six characters, each with eighteen transparent frames, plus original sheets and a manifest. Allies: Archer, Mage, Paladin, Witch. Enemies: Skeleton Runner, Shield Bearer. Each has six idle/walk, six attack and six death frames.
- `Assets/UI/Premium/VFX`: twenty-four transparent frames across summon, impact, arcane and ice effects.
- `Assets/UI/Extra Ui Images/Sprite Sources`: original generated sheets for future reslicing.

There are 164 new individual PNG sprites. Original sheets are additional source assets. No packed UI screenshot is used as a functional screen.

The MainMenu scene has a saved `MainMenuCanvas/SafeArea/MenuComposition` hierarchy. Main menu, collection, settings, tutorial and exit confirmation are individual authored panels. Their button frames, icons, text and slider parts are individual children. The gameplay scene retains its combat managers, board and placement systems. `HUDCanvas`, `TopHUD`, `BottomTray`, `GuardianScroll`, and the screen panels remain editable in the Inspector. Use the saved hierarchy for manual adjustments. Authoring scripts are editor tools; the game does not need an automatic UI installer at startup.

Reference composition is 1080 × 1920 portrait. Safe-area fitting and a centered portrait composition adapt the layout to tall phones and iPads. Gameplay camera framing reserves space for HUD and bottom controls. Device testing remains mandatory; a desktop aspect-ratio preview does not establish support for every iPhone/iPad.

## Battle contract

Five lanes lead from the enemy portals to the soul crystal. Select a guardian, then tap an empty tile; vertical card drag places a guardian and horizontal drag scrolls the six-card tray. Occupied cells and paused battles must reject deployment without charging energy. Costs and combat statistics are authored in `Assets/SHAMSA/GameplayData`.

Six normal guardians retain their distinct combat roles: Dwarf front-line defense, Chef melee damage, Archer piercing shots, Mage cross-lane chain magic, Paladin durable defense and Witch slowing magic. Preserve existing placement, economy, fusion, upgrade and wave behavior when polishing presentation. Validate their balance through complete runs rather than assuming the new art proves gameplay quality.

Crystal Arts remain Soul Pulse (knockback and purification, 18-second cooldown), Crystal Storm (36 damage, 28-second cooldown), and Divine Barrier (temporary breach protection, 35-second cooldown). Corruption affects two cells for ten seconds and reduces attack speed by 35 percent. All cooldowns and temporary effects follow scaled game time and pause with battle.

### Reinforcement choices

Royal Bulwark is the friendly blue elephant. Once per battle, spend 200 soul and choose an empty tile. It blocks and attacks for up to twenty seconds, has 500 health and cannot be upgraded. Its arrival is a tactical rescue, not a seventh permanent guardian card.

Soul Express is the friendly train. Once per battle, spend 150 soul and choose a lane. It travels toward the portals and strikes each enemy once for 90 damage with knockback. It leaves the battlefield after its sweep. Both choices are visible on the gameplay tray and explained in collection/tutorial screens.

The hostile elephant remains an enemy boss and is displayed as Corrupted Colossus with a purple tint, distinguishing it from the friendly reinforcement. Existing boss statistics are preserved.

### More enemy and realm variety

Skeleton Runners join procedural waves from wave 3; they are fast, fragile attackers. Shield Bearers join from wave 7; they move slowly and reduce incoming damage by 35 percent. Existing elite and boss entries remain intact. These additions use actual enemy prefabs, data and animation frames.

The current twenty-wave run changes atmosphere in five-wave bands: Forgotten Graveyard, Frozen Tombs, Ember Wasteland, Royal Soul Citadel. These reuse the project's existing clean battlefield artwork. This is presentation progression within the current run, not a claim that four separate campaigns or 999 authored waves exist.

## Screens and controls

Main menu offers Play, collection/shop, Settings, How To Play and Exit confirmation. Collection describes six guardians and the elephant/train support choices; it does not imply unimplemented purchases. Settings provide persistent master, music and effects volume. Gameplay pause offers Resume, Settings, How To Play and Main Menu. Tutorial explains selecting, placing, defending and using Crystal Arts, plus support roles.

Preserve text contrast, button hit areas and crystal health visibility. Keep selection outlines, costs, cooldowns, unavailable states and support-used states readable without relying only on color. UI artwork should not intercept battlefield input.

## Verification and release gates

Editor compilation, runtime console checks, placement/combat acceptance checks, screen navigation and actual Game View captures are recorded under `ArtReview/PremiumVisuals`. Treat a test as passed only when its report exists. Generated artwork and source inspection alone do not establish that the game is ready to publish.

Before release: complete twenty-wave playthroughs and defeat/retry paths; test upgrades/fusion, all six guardians, both reinforcements and each boss; check audio interruption and background/resume; profile frame time and memory on lower-end supported hardware; test real safe areas and touch gestures on iPhone and iPad; verify all displayed content and screenshots match the build; complete icons, signing, supported orientations, content rating, privacy disclosures and any third-party SDK disclosures. If purchases are introduced later, implement and test their full store lifecycle before exposing them.

Apple submission guidance: https://developer.apple.com/app-store/review/guidelines/
Privacy disclosure guidance: https://developer.apple.com/help/app-store-connect/manage-app-information/manage-app-privacy/

App Store approval, real-device performance, TestFlight coverage and a signed release build are not established by this visual integration.

## Latest visual revision

The original illustrated Main Menu has been restored from the saved backup, as requested. Gameplay now uses softer navy panels and subtle cyan borders rather than heavy gold frames. Dynamic UI labels render through serialized `Label_TMP` TextMeshPro objects with auto-size, padding and ellipsis bounds. Existing text drivers remain disabled as renderers and feed the saved TMP labels through `TMPTextBinding`; edit the TMP object for visual alignment and the source/controller for dynamic content. Main menu button lettering baked into the approved artwork is preserved.

Placement previews now use each prefab's actual battlefield sprite and world scale rather than its large card portrait. Frost Witch and Iron Paladin use the same 1.164-unit sprite bounds as Archer and Mage. The obsolete full-size portrait renderers on their prefabs are disabled. Drop squash/stretch briefly changes scale during landing, then settles to the authored miniature size.

Latest phone checks: no overflowing TMP text in normal gameplay, pause, settings or tutorial at 1080 × 1920; zero visible legacy Text renderers. Real five-point breach damage updates the crystal fill to 75 percent. Evidence: `ArtReview/PremiumVisuals/SoftUI-Phone-Check.json` and `Guardian-Drop-Sizes.json`.
Additional Game View checks passed at 1170 × 2532 and 2048 × 2732: zero TMP text overflow in the checked gameplay/tutorial/settings panels. Runtime console returned zero errors after the final integration. These are Editor previews; physical-device testing remains pending.
