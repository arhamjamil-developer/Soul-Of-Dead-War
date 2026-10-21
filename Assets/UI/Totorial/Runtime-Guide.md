# Manual first-wave tutorial

Open `Assets/Scenes/SoulCrystalDefenseGameplay.unity`. The authored canvas objects are under `GameplayComposition/FirstWaveTutorialSystem`. They are saved in the scene; the controller does not generate UI at runtime.

Select FirstWaveTutorialSystem to edit the seven Steps in its inspector: speaker, instruction, portrait and optional voice AudioClip. Borders, portrait, progress, text, Skip and Continue are separate objects under TutorialDialogue. Text uses the existing PAYBACK TMP font with bounded autosizing.

## Player flow

1. Welcome: Let's Go starts the lesson; Skip exits it.
2. Select the highlighted Guardian card. Other card choices cannot advance the lesson.
3. Place the Guardian on center lane 2, row 3.
4. Select Archer and place it on left lane 1, row 2.
5. Select Guardian and place it on right lane 3, row 3. If needed, a clearly labelled training soul bonus makes the placement affordable while time is held.
6. Defend wave one in normal combat. No Continue shortcut clears this objective.
7. The actual wave-one completion event shows the completion dialogue. Keep Playing releases the next wave countdown and leaves the player in endless mode.

The wave clock is held through the guided setup. Selection and placement remain available; incorrect placement attempts cannot spend souls. Skip restores controls and clock ownership, cancels selection, hides tutorial UI, and records the tutorial as seen. In-game pause remains usable. Tutorial completion uses `SoulCrystal.FirstWaveTutorial.v1`; How To Play has an authored Replay Tutorial button which starts a fresh practice run without resetting the completion preference. Leaving or losing an unfinished lesson does not count as completion.

## Audio

Record the lines in `Audio/Voice-Lines.csv` and assign the clips to the matching Step.voice fields. AudioSource is already authored on FirstWaveTutorialSystem. Changing steps or skipping stops the previous recording. Audio remains optional; no external generation service is required.

## Authoring tools

`SoulCrystalDefense > Tutorial > Install Manual Tutorial` rebuilds only the tutorial objects and the How To Play replay button. It explicitly retains endless procedural progression. Avoid rerunning it after manually editing dialogue unless you intend to restore its default text/layout. The generic art-preview builder is for border/prefab art review and is not required for playing the tutorial.

Runtime verification is recorded in `ArtReview/Tutorial/Runtime-QA.md`; screenshots there are actual Game View captures. Physical iPhone/iPad touch testing remains separate from editor verification.
