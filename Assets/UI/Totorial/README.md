# UI Tutorial – "Soul of Dead War"

For the actual first-wave player onboarding system, editable scene hierarchy, Skip/Replay behavior and optional voice bindings, see [Runtime-Guide.md](Runtime-Guide.md). The older UI-building notes below do not describe the current onboarding controller.

This README provides the up‑to‑date guide for the UI tutorial in the **Soul of Dead War** project. It reflects the current implementation status, references the existing scripts, and leaves clearly marked placeholders where audio assets will be added later.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Setup](#project-setup)
4. [UI Canvas Setup](#ui-canvas-setup)
5. [Implemented UI Elements](#implemented-ui-elements)
6. [Scripting Overview](#scripting-overview)
7. [Audio Placeholders](#audio-placeholders)
8. [Testing the UI](#testing-the-ui)
9. [Future Work & FAQ](#future-work--faq)

---

## Overview

The UI tutorial walks you through creating a minimal but functional in‑game UI using Unity's UI Toolkit (or the classic UGUI) and the **AnkleBreaker‑MCP** utilities. The current implementation includes:

- **Score Counter** (`ScoreDisplay.cs`)
- **Health Bar** (`HealthBar.cs`)
- **Pause Menu** (`PauseMenu.cs`)
- **Game Manager** that ties the UI to gameplay (`GameManager.cs`)

The tutorial is deliberately small; you can extend it with additional panels, animations, and audio.

---

## Prerequisites

- Unity **6000.3.13f1** (the version used by this project).
- **AnkleBreaker Unity‑MCP** package installed (`com.anklebreaker.unity-mcp`).
- Basic familiarity with Unity's UI Toolkit or UGUI.
- Unity editor must be running on **port 7891** (the MCP broker port used by this project).

---

## Project Setup

```powershell
# From PowerShell
cd B:/Soul-Of-Dead-War
"C:/Program Files/Unity/Hub/Editor/6000.3.13f1/Editor/Unity.exe" -projectPath . -port 7891
```

Verify the MCP connection by checking the Unity Console for a log similar to:

```
[AB-UMCP] Server started on port 7891
```

If the log is missing, run `unity_select_instance` via the MCP to select the running instance.

---

## UI Canvas Setup

1. In the **Hierarchy**, right‑click → **UI → UI Document** (UI Toolkit) **or** **UI → Canvas** (UGUI). 
2. Rename the new object to `GameUI`.
3. Save the scene as `UI_Tutorial.unity` under `Assets/Scenes/`.
4. Ensure `GameUI` is active in the hierarchy.

---

## Implemented UI Elements

### Score Counter
- Script: `Assets/UI/Totorial/Scripts/ScoreDisplay.cs`
- Add a **Text** component named `ScoreText` and assign it to the `scoreText` field.

### Health Bar
- Script: `Assets/UI/Totorial/Scripts/HealthBar.cs`
- Add a **Slider** component, set Min = 0, Max = 1, and assign it to the `healthSlider` field.

### Pause Menu
- Script: `Assets/UI/Totorial/Scripts/PauseMenu.cs`
- Create a **Panel** named `PauseMenu`, set it inactive by default, and add **Resume** and **Quit** buttons.
- Assign the panel GameObject to the `pausePanel` field.

---

## Scripting Overview

All scripts reside in `Assets/UI/Totorial/Scripts/`.

```csharp
// GameManager.cs – central hub for UI interaction
public class GameManager : MonoBehaviour {
    public ScoreDisplay scoreDisplay;
    public HealthBar healthBar;

    void Start() {
        scoreDisplay.AddScore(0);
        healthBar.SetHealth(1f);
    }

    public void OnEnemyDefeated() {
        scoreDisplay.AddScore(10);
    }

    public void OnPlayerHit(float remainingHealthPercent) {
        healthBar.SetHealth(remainingHealthPercent);
    }
}
```

Assign the `ScoreDisplay` and `HealthBar` references in the inspector.

---

## Audio Placeholders

The tutorial does **not** include audio assets yet. Below are clearly marked placeholders where you can drop in your own audio clips later:

- **Score Increment Sound** – place an `AudioClip` named `ScoreIncrement.wav` in `Assets/Audio/` and attach it to `ScoreDisplay` (you will add the playback code yourself).
- **Pause Menu Open/Close Sound** – placeholder `PauseOpen.wav` and `PauseClose.wav` under `Assets/Audio/`.
- **Health Bar Damage Sound** – placeholder `HealthDamage.wav`.

> **Note:** The audio implementation is left for you. Ensure the audio clips are imported with **Load Type: Decompress on Load** and **Compression: Vorbis** for optimal performance.

---

## Testing the UI

1. Press **Play** in the Unity editor.
2. Use the inspector or temporary debug keys to call the public methods on `GameManager` (`OnEnemyDefeated`, `OnPlayerHit`).
3. Press **Esc** to toggle the pause menu.
4. Verify the health bar updates and the score counter increments.
5. If MCP is running, you can hot‑reload UI layout without restarting:

```powershell
# From a separate terminal while Unity is running
unity_execute_code -p 7891 -c "UnityEditor.EditorApplication.QueuePlayerLoopUpdate();"
```

---

## Future Work & FAQ

| Issue | Suggested Solution |
|-------|--------------------|
| UI elements not visible | Ensure `GameUI` object is active and its **Sorting Layer** is above other geometry. |
| No console logs from MCP | Verify Unity was started with `-port 7891` and that `com.anklebreaker.unity-mcp` is present in `Packages/manifest.json`. |
| Score does not update | Confirm `ScoreText` is assigned and `AddScore` is being called. |
| Pause menu flickers | Make sure `Time.timeScale` is set to `0` only when the menu is active. |
| Audio not playing | Add `AudioSource` components to the relevant scripts and assign the placeholder clips.

---

### 🎉 Congratulations!
You now have a functional UI skeleton for *Soul of Dead War*. Feel free to expand it with additional panels (inventory, settings), animations, and the audio assets you plan to add.

*Happy coding!*
