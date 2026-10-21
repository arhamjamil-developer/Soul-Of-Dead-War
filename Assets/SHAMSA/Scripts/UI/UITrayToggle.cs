// <summary>
// UITrayToggle provides a simple button‑based toggle for the top HUD and bottom tray UI panels.
// Attach this script to a UI Button and assign the TopHUD and BottomTray objects in the inspector.
// When the button is pressed the panels will be shown/hidden by toggling their active state.
// This implementation uses CanvasGroup so that any layout calculations remain intact while the
// panels are hidden (the UI still participates in layout but is invisible and non‑interactable).
// </summary>
using UnityEngine;
using UnityEngine.UI;

namespace SoulCrystalDefense.UI
{
    public class UITrayToggle : MonoBehaviour
    {
        [Header("UI Panels to toggle")]
        [SerializeField] private CanvasGroup topHUDCanvasGroup;
        [SerializeField] private CanvasGroup bottomTrayCanvasGroup;
        // Optional: a reference to the button that invokes the toggle – can be set via the
        // UnityEvent in the inspector (or the button can call UITrayToggle.Toggle directly).
        [Header("Optional UI button (auto‑wire)" )]
        [SerializeField] private Button toggleButton;

        private bool _isVisible = true;

        private void Awake()
        {
            // Ensure CanvasGroup components exist on the target objects.
            if (topHUDCanvasGroup == null)
            {
                var topObj = GameObject.Find("TopHUD");
                if (topObj != null) topHUDCanvasGroup = topObj.GetComponent<CanvasGroup>() ?? topObj.AddComponent<CanvasGroup>();
            }
            if (bottomTrayCanvasGroup == null)
            {
                var bottomObj = GameObject.Find("BottomTray");
                if (bottomObj != null) bottomTrayCanvasGroup = bottomObj.GetComponent<CanvasGroup>() ?? bottomObj.AddComponent<CanvasGroup>();
            }
            // Auto‑wire the button if supplied.
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(Toggle);
            }
        }

        /// <summary>
        /// Toggle visibility of the top HUD and bottom tray.
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            SetCanvasGroupVisibility(topHUDCanvasGroup, _isVisible);
            SetCanvasGroupVisibility(bottomTrayCanvasGroup, _isVisible);
        }

        private void SetCanvasGroupVisibility(CanvasGroup cg, bool visible)
        {
            if (cg == null) return;
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }
}
