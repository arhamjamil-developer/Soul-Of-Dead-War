using System.Collections;
using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    public class PortalVisual : MonoBehaviour
    {
        [SerializeField] private int laneIndex;
        [SerializeField] private SpriteRenderer portalRenderer;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color spawnColor = new Color(1f, 0.9f, 0.4f, 1f);

        private Coroutine activePulseRoutine;

        public int LaneIndex => laneIndex;

        public void Initialize(int lane)
        {
            laneIndex = lane;
            if (portalRenderer == null)
                portalRenderer = GetComponent<SpriteRenderer>();

            if (portalRenderer != null)
                portalRenderer.color = idleColor;
        }

        public void TriggerTelegraph(float duration)
        {
            if (activePulseRoutine != null)
                StopCoroutine(activePulseRoutine);

            activePulseRoutine = StartCoroutine(TelegraphRoutine(duration));
        }

        public void TriggerSpawnFlash()
        {
            if (activePulseRoutine != null)
                StopCoroutine(activePulseRoutine);

            activePulseRoutine = StartCoroutine(SpawnFlashRoutine());
        }

        private IEnumerator TelegraphRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 4f, 1f);
                if (portalRenderer != null)
                {
                    portalRenderer.color = Color.Lerp(idleColor, telegraphColor, t);
                    transform.localScale = Vector3.one * (1f + t * 0.15f);
                }
                yield return null;
            }

            if (portalRenderer != null)
            {
                portalRenderer.color = idleColor;
                transform.localScale = Vector3.one;
            }
        }

        private IEnumerator SpawnFlashRoutine()
        {
            if (portalRenderer != null)
            {
                portalRenderer.color = spawnColor;
                transform.localScale = Vector3.one * 1.3f;
            }

            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (portalRenderer != null)
                {
                    portalRenderer.color = Color.Lerp(spawnColor, idleColor, t);
                    transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t);
                }
                yield return null;
            }

            if (portalRenderer != null)
            {
                portalRenderer.color = idleColor;
                transform.localScale = Vector3.one;
            }
        }
    }
}
