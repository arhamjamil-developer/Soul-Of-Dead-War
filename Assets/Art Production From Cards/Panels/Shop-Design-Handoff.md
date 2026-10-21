# Soul Shop visual reference

`Shop-Design-1080x1920.png` matches the Settings and How to Play crimson-stone/gold theme. The original generated image is retained in `Sources/Shop-Design-Original.png`. Generated with the built-in image generator; exact prompt and references are in `Shop-Generation-Prompt.json`.

The live MainMenu shop was read without editing the scene. Its root is 1080 x 1920 and it currently presents four textual entries (Dwarf, Chef, Royal Elephant and Soul Train), explicitly stating that purchases are unavailable.

This concept shows the six existing allied guardians in a consistent two-column by three-row collection: Dwarf, Chef, Archer, Mage, Paladin and Frost Witch. Each has an IN SQUAD status. Royal Elephant and Soul Train appear in a separate reinforcement strip. No prices or purchasing flows were invented. The concept has not been applied to the shop scene.

For editable Unity implementation, use one reusable squared card frame, six independent existing portraits, separate TMP name/status labels, a text-free Mage/Witch header, the common stone modal frame, and the blank crimson Back button. Keep all labels separate from sprites and use saved anchors for phone/iPad fitting, without a new runtime layout script.
