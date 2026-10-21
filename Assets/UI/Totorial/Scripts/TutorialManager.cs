using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SoulOfDeadWar.UI.Tutorial
{
    /// <summary>
    /// Simple tutorial manager that shows a sequence of instructional messages on screen.
    /// Attach this component to a GameObject under the UI Canvas (e.g., a Panel).
    /// It expects a Text component (or TMP_Text) to display the current step and a Button to advance.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI References")] 
        [Tooltip("Text component that shows the tutorial step.")]
        public Text tutorialText;

        [Tooltip("Button that the player presses to continue to the next step.")]
        public Button nextButton;

        // Define the tutorial steps. You can edit these in the inspector or via code.
        private readonly List<string> steps = new List<string>
        {
            "Welcome to the Soul of Dead War tutorial!",
            "Use WASD or the left stick to move your character.",
            "Press Space or the A button to attack.",
            "Collect crystals to power up your abilities.",
            "Avoid enemy projectiles – they can stun you.",
            "Good luck, hero!"
        };

        private int currentStep = 0;

        private void Awake()
        {
            if (tutorialText == null || nextButton == null)
            {
                Debug.LogError("TutorialManager: UI references not assigned.");
                enabled = false;
                return;
            }

            nextButton.onClick.AddListener(OnNextClicked);
            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            tutorialText.text = steps[currentStep];
            // Change button label on the last step
            var btnText = nextButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = (currentStep < steps.Count - 1) ? "Next" : "Finish";
            }
        }

        private void OnNextClicked()
        {
            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                ShowCurrentStep();
            }
            else
            {
                // Tutorial finished – hide UI
                gameObject.SetActive(false);
            }
        }
    }
}
