using UnityEngine;

namespace SoulCrystalDefense.Combat
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Vector2 barSize = new Vector2(0.8f, 0.09f);
        [SerializeField] private Color fillColor = new Color(0.2f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color bgColor = new Color(0.05f, 0.05f, 0.08f, 0.8f);
        [SerializeField] private bool hideWhenFull = true;

        private float maxHealth = 100f;
        private float currentHealth = 100f;

        private SpriteRenderer bgRenderer;
        private SpriteRenderer fillRenderer;
        private Transform bgTrans;
        private Transform fillTrans;

        private void Awake()
        {
            SetupRenderers();
        }

        public void SetColor(Color c)
        {
            fillColor = c;
            if (fillRenderer != null) fillRenderer.color = fillColor;
        }

        public void Initialize(float maxHp, Color color)
        {
            maxHealth = Mathf.Max(1f, maxHp);
            currentHealth = maxHealth;
            SetColor(color);
            SetupRenderers();
            UpdateBar();
        }

        public void UpdateHealth(float hp)
        {
            currentHealth = Mathf.Clamp(hp, 0f, maxHealth);
            UpdateBar();
        }

        private void SetupRenderers()
        {
            // Clean up any old broken children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name == "BG" || child.name == "Fill" || child.name == "Visual")
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            Sprite pixel = GetWhitePixelSprite();

            // Background
            var bgObj = new GameObject("BG");
            bgObj.transform.SetParent(transform, false);
            bgTrans = bgObj.transform;
            bgTrans.localPosition = Vector3.zero;
            bgTrans.localScale = new Vector3(barSize.x, barSize.y, 1f);
            bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = pixel;
            bgRenderer.color = bgColor;
            bgRenderer.sortingOrder = 10;

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(transform, false);
            fillTrans = fillObj.transform;
            fillRenderer = fillObj.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = pixel;
            fillRenderer.color = fillColor;
            fillRenderer.sortingOrder = 11;

            // Normalize parent scale if scaled
            if (transform.parent != null)
            {
                Vector3 pScale = transform.parent.lossyScale;
                if (pScale.x > 0.001f && pScale.y > 0.001f)
                {
                    transform.localScale = new Vector3(1f / pScale.x, 1f / pScale.y, 1f);
                }
            }
        }

        private void UpdateBar()
        {
            if (bgRenderer == null || fillRenderer == null)
            {
                SetupRenderers();
            }

            float ratio = Mathf.Clamp01(currentHealth / maxHealth);

            if (hideWhenFull && Mathf.Approximately(ratio, 1f))
            {
                bgRenderer.enabled = false;
                fillRenderer.enabled = false;
                return;
            }

            bgRenderer.enabled = true;
            fillRenderer.enabled = true;

            float currentWidth = Mathf.Max(0.01f, barSize.x * ratio);
            fillTrans.localScale = new Vector3(currentWidth, barSize.y, 1f);
            // Left-aligned fill
            fillTrans.localPosition = new Vector3((-barSize.x + currentWidth) * 0.5f, 0f, 0f);
        }

        private static Sprite cachedWhitePixel;
        public static Sprite GetWhitePixelSprite()
        {
            if (cachedWhitePixel == null)
            {
                var tex = new Texture2D(2, 2);
                Color[] colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
                tex.SetPixels(colors);
                tex.Apply();
                cachedWhitePixel = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2);
            }
            return cachedWhitePixel;
        }
    }
}
