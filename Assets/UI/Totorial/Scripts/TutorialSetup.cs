using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoulCrystalDefense.UI
{
    /// <summary>
    /// Helper component that auto‑assigns the UI references required by <c>FirstWaveTutorial</c>.
    /// Attach this to the same GameObject that holds <c>FirstWaveTutorial</c>.
    /// It will locate child objects by naming conventions and populate the arrays.
    /// This reduces the manual drag‑and‑drop work and prevents the "UnassignedReferenceException"
    /// you saw in the console.
    /// </summary>
    public class TutorialSetup : MonoBehaviour
    {
        public FirstWaveTutorial tutorial;

        // Expected child names (can be overridden in the inspector if needed)
        [Header("Naming conventions for auto‑assignment")]
        public string guardedControlPrefix = "GuardedControl"; // e.g., GuardedControl0, GuardedControl1 ...
        public string guardedButtonPrefix = "GuardedButton";   // e.g., GuardedButton0, GuardedButton1 ...
        public string obstructingAnnouncementPrefix = "Announce"; // e.g., Announce0, Announce1 ...

        void Reset()
        {
            // Auto‑populate the reference on Reset (when the component is first added)
            tutorial = GetComponent<FirstWaveTutorial>();
        }

        void Awake()
        {
            if (tutorial == null)
                tutorial = GetComponent<FirstWaveTutorial>();

            if (tutorial == null)
            {
                Debug.LogError("TutorialSetup: No FirstWaveTutorial component found on the GameObject.");
                enabled = false;
                return;
            }

            // Auto‑assign guarded controls and buttons based on child count
            var controls = new System.Collections.Generic.List<CanvasGroup>();
            var buttons = new System.Collections.Generic.List<Button>();
            var announcements = new System.Collections.Generic.List<CanvasGroup>();

            foreach (Transform child in tutorial.transform)
            {
                if (child.name.StartsWith(guardedControlPrefix))
                {
                    var cg = child.GetComponent<CanvasGroup>();
                    if (cg != null) controls.Add(cg);
                }
                else if (child.name.StartsWith(guardedButtonPrefix))
                {
                    var btn = child.GetComponent<Button>();
                    if (btn != null) buttons.Add(btn);
                }
                else if (child.name.StartsWith(obstructingAnnouncementPrefix))
                {
                    var cg = child.GetComponent<CanvasGroup>();
                    if (cg != null) announcements.Add(cg);
                }
            }

            tutorial.guardedControls = controls.ToArray();
            tutorial.guardedButtons = buttons.ToArray();
            tutorial.obstructingAnnouncements = announcements.ToArray();
        }
    }
}
