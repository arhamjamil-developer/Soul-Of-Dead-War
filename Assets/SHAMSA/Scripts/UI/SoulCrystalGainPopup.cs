// <summary>
// Displays a stylish floating popup whenever the player gains Soul Energy (e.g., by defeating enemies).
// Attach this script to a Canvas object (e.g., the HUDCanvas) and assign a simple prefab for the popup.
// If no prefab is set, the script will create a TextMeshProUGUI element on the fly.
// </summary>
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.UI
{
    public class SoulCrystalGainPopup : MonoBehaviour
    {
        [Header("Popup Settings")]
        // Optional prefab with a CanvasGroup and TextMeshProUGUI component.
        [SerializeField] private GameObject popupPrefab;
        // How long the popup lives before fading out (seconds).
        [SerializeField] private float lifetime = 1.2f;
        // Vertical movement distance (world units) during the animation.
        [SerializeField] private float riseDistance = 50f;

        private void OnEnable()
        {
            // Subscribe to the energy‑gained event when this component becomes active.
            if (SoulEnergyWallet.Instance != null)
                SoulEnergyWallet.Instance.OnEnergyGained += OnEnergyGained;
        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks or missed events.
            if (SoulEnergyWallet.Instance != null)
                SoulEnergyWallet.Instance.OnEnergyGained -= OnEnergyGained;
        }

        private void OnDestroy()
        {
            if (SoulEnergyWallet.Instance != null)
                SoulEnergyWallet.Instance.OnEnergyGained -= OnEnergyGained;
        }

        private void OnEnergyGained(int amount)
        {
            // Create a popup displaying the amount gained.
            var popup = CreatePopup($"+{amount}");
            if (popup != null)
                StartCoroutine(AnimatePopup(popup));
        }

        private GameObject CreatePopup(string text)
        {
            GameObject go;
            if (popupPrefab != null)
            {
                go = Instantiate(popupPrefab, transform);
            }
            else
            {
                // Build a minimal popup on the fly.
                go = new GameObject("SoulEnergyPopup");
                go.transform.SetParent(transform, false);

                var canvasGroup = go.AddComponent<CanvasGroup>();
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;

                var txt = go.AddComponent<TextMeshProUGUI>();
                txt.text = text;
                txt.fontSize = 36;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = new Color(0.2f, 0.9f, 1f, 1f);
                txt.enableAutoSizing = true;
            }
            // Position at the bottom‑center of the screen (just above the BottomTray).
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 50f); // slightly above bottom UI
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            return go;
        }

        private IEnumerator AnimatePopup(GameObject popup)
        {
            var rect = popup.GetComponent<RectTransform>();
            var cg = popup.GetComponent<CanvasGroup>();
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, riseDistance);

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / lifetime);
                // Move upward.
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                // Fade out towards the end.
                if (cg != null)
                    cg.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            Destroy(popup);
        }
    }
}
