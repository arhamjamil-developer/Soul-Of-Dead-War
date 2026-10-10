using System.Collections;
using UnityEngine;

namespace SoulOfDeadWar.Animation
{
    /// <summary>
    /// Manages top-down 2D sprite outline highlighting and hit-flash visual feedback
    /// using MaterialPropertyBlocks for zero-allocation runtime performance.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteVFXController : MonoBehaviour
    {
        [Header("Shader Property Identifiers")]
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");

        [Header("Hit Flash Settings")]
        [Tooltip("Color flashed when receiving damage")]
        [SerializeField] private Color hitFlashColor = Color.white;

        [Tooltip("Duration of the hit flash decay in seconds")]
        [SerializeField] private float flashDuration = 0.15f;

        [Header("Outline Settings")]
        [Tooltip("Default outline color for targeting / selection")]
        [SerializeField] private Color defaultOutlineColor = new Color(1f, 0.2f, 0.2f, 1f);

        [Tooltip("Thickness in sprite texels")]
        [Range(1f, 5f)]
        [SerializeField] private float outlineThickness = 2.0f;

        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propBlock;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            // Initial sync of properties
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(OutlineThicknessId, outlineThickness);
            _propBlock.SetColor(OutlineColorId, defaultOutlineColor);
            _propBlock.SetFloat(FlashAmountId, 0f);
            _spriteRenderer.SetPropertyBlock(_propBlock);
        }

        /// <summary>
        /// Enables or disables the silhouette outline around the sprite.
        /// </summary>
        /// <param name="enabled">True to activate outline</param>
        /// <param name="color">Optional custom outline color</param>
        public void SetOutline(bool enabled, Color? color = null)
        {
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(OutlineEnabledId, enabled ? 1f : 0f);
            if (color.HasValue)
            {
                _propBlock.SetColor(OutlineColorId, color.Value);
            }
            _spriteRenderer.SetPropertyBlock(_propBlock);
        }

        /// <summary>
        /// Triggers a hit flash effect that smoothly decays to 0.
        /// </summary>
        public void TriggerHitFlash()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }
            _flashRoutine = StartCoroutine(HitFlashCoroutine());
        }

        private IEnumerator HitFlashCoroutine()
        {
            float elapsed = 0f;
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(FlashColorId, hitFlashColor);

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / flashDuration;
                // Decay flash amount from 1 down to 0
                float currentFlash = Mathf.Lerp(1f, 0f, progress);

                _propBlock.SetFloat(FlashAmountId, currentFlash);
                _spriteRenderer.SetPropertyBlock(_propBlock);
                yield return null;
            }

            _propBlock.SetFloat(FlashAmountId, 0f);
            _spriteRenderer.SetPropertyBlock(_propBlock);
            _flashRoutine = null;
        }

        private void OnDisable()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
        }
    }
}
