using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    /// <summary>
    /// Enforces buttery-smooth 60 FPS mobile performance, non-allocating physics time steps,
    /// screen sleep prevention, and smooth decaying camera impulse shake.
    /// </summary>
    public class MobileSmoothPerformanceSystem : MonoBehaviour
    {
        public static MobileSmoothPerformanceSystem Instance { get; private set; }

        [Header("Frame Rate & Thermal Optimization")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync = true;
        [SerializeField] private bool preventScreenSleep = true;

        [Header("Physics Smoothing (60Hz Fixed Delta)")]
        [SerializeField] private bool syncFixedDeltaTime = true;

        [Header("Camera Impulse Shake")]
        [SerializeField] private Camera targetCamera;
        private Vector3 initialCameraPosition;
        private float shakeIntensity = 0f;
        private float shakeDecay = 5.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            ApplyPerformanceSettings();

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                initialCameraPosition = targetCamera.transform.position;
            }
        }

        private void Start()
        {
            ApplyPerformanceSettings();
        }

        public void ApplyPerformanceSettings()
        {
            // Lock target framerate to 60 FPS for smooth mobile gameplay
            Application.targetFrameRate = targetFrameRate;

            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (preventScreenSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (syncFixedDeltaTime)
            {
                // Match fixed update to 60Hz (0.0166667s) to eliminate micro-stuttering
                Time.fixedDeltaTime = 1f / 60f;
                Time.maximumDeltaTime = 0.1f; // Prevent spiral of death on lag spikes
            }
        }

        public void TriggerShake(float intensity, float duration = 0.25f)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeDecay = duration > 0f ? (intensity / duration) : 5.0f;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            if (shakeIntensity > 0.001f)
            {
                float offsetX = (Random.value * 2f - 1f) * shakeIntensity;
                float offsetY = (Random.value * 2f - 1f) * shakeIntensity;
                targetCamera.transform.position += new Vector3(offsetX, offsetY, 0f);

                shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0f, shakeDecay * Time.unscaledDeltaTime);

                if (shakeIntensity <= 0.001f)
                {
                    shakeIntensity = 0f;
                }
            }
        }
    }
}
