using UnityEngine;

namespace SoulCrystalDefense.Systems
{
    [RequireComponent(typeof(Camera))]
    public class ResponsiveCamera : MonoBehaviour
    {
        [Header("Target Reference")]
        [SerializeField] private float referenceWidth = 1080f;
        [SerializeField] private float referenceHeight = 1920f;
        [SerializeField] private float baseOrthographicSize = 6.0f;

        private Camera cam;
        private float lastAspect = -1f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            ApplyResponsiveSize();
        }

        private void Update()
        {
            if (!Mathf.Approximately(cam.aspect, lastAspect))
            {
                ApplyResponsiveSize();
            }
        }

        public void ApplyResponsiveSize()
        {
            if (cam == null) cam = GetComponent<Camera>();

            float targetAspect = referenceWidth / referenceHeight; // 9:16 = 0.5625
            float currentAspect = cam.aspect;
            lastAspect = currentAspect;

            if (currentAspect < targetAspect)
            {
                // Narrow screen (e.g. tall phone like 19.5:9 or 20:9): adjust ortho size to ensure full width is visible
                cam.orthographicSize = baseOrthographicSize * (targetAspect / currentAspect);
            }
            else
            {
                // Wider screen (e.g. iPad 4:3 or standard tablet): vertical height is fully visible, extra width displays backdrop
                cam.orthographicSize = baseOrthographicSize;
            }
        }
    }
}
