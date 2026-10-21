# First-wave tutorial verification

Verified in Unity 6000.3.13f1, iOS project target, 20 October 2026. This is editor verification, not App Store/device certification.

## Passed

- Script compilation: zero errors after the controller and authoring fixes.
- Scene save/reload: all eleven guarded control groups, panel group, combat prompt, portraits, buttons, audio source and highlight references survive serialization.
- First-run welcome activates and holds the countdown at zero elapsed wave time. The countdown display is hidden during guided setup.
- Actual Unity UI pointer-click handlers advance Welcome and Guardian selection. Other card selections are rejected. Continue cannot bypass action objectives.
- Invalid placement attempts do not spend souls or advance the step.
- Correct Guardian center placement, Archer left placement and Guardian right placement each advance from a real successful deployment event.
- A clearly labelled training grant makes the final placement affordable while the clock is held.
- Setup holds game time at zero without setting GameManager's pause flag. In-game pause/resume remains usable.
- Combat restores time scale to one, restores normal controls, hides the large dialogue and shows a small objective strip with Skip.
- Wave one cleared through natural combat in two full runs. Completion was triggered by the real wave-complete event, with crystal health 12/20 and no active enemies. Wave two's countdown was held at the completion dialogue.
- Keep Playing records completion, restores controls and releases endless wave two.
- Skip from Welcome, placement and live combat hides both tutorial panels, clears selection, restores controls/time and records the preference.
- Reload after completion/skip bypasses onboarding. Replay restarts a fresh lesson despite the completion preference. The saved How To Play replay button has its persistent listener.
- Isolated wave-20 continuation test starts wave 21 while the game remains active. Saved high score restored after that boundary test. This is not a full twenty-wave campaign playthrough.
- Highlight corrected for ScreenSpaceCamera canvases. Final target alignment measured zero pixel error; tile outline size approximately 240 x 174 canvas units. It does not intercept pointer raycasts.
- Text uses PAYBACK TMP with bounded autosizing. No instruction overflow in inspected phone layouts. Duplicate hint text removed.

## Visual evidence

- `Gameplay-Tutorial-Welcome.png`: actual portrait Game View.
- `Gameplay-Tutorial-Highlight.png`: final camera-correct placement outline, actual simulated phone capture.
- `Gameplay-Tutorial-Defend.png`: compact combat prompt, 1080 x 1920 Game View.
- `Gameplay-Tutorial-Tall-Phone.png`: completion dialogue at 828 x 1792, actual simulated phone capture.

Multiple Simulator windows were open and could override the Game View's requested size. A requested iPad test resolved to a phone simulator; no successful tablet capture is claimed. Physical iPhone/iPad touch, notches and interruption behavior still need device testing. A synthetic mouse injection while the editor was unfocused did not produce a placement; selection pointer handlers, target raycast clearance and successful placement APIs were verified separately.

## Editing and audio

Edit `GameplayComposition/FirstWaveTutorialSystem` in `SoulCrystalDefenseGameplay`. Seven editable Step entries hold speaker, message, portrait and optional voice clip. The canvas/prefab is authored, not generated at runtime. Recording lines match `Assets/UI/Totorial/Audio/Voice-Lines.csv`. No voice recordings or external voice services were used.
