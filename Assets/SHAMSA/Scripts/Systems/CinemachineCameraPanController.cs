using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.Systems
{
    /// <summary>
    /// Controls smooth movement, inertia, zoom, and confinement of the Cinemachine camera.
    /// Operates by moving a follow target that the CinemachineCamera tracks with smooth position damping.
    /// Confined within the domain of the background sprite/collider.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class CinemachineCameraPanController : MonoBehaviour
    {
        public static CinemachineCameraPanController Instance { get; private set; }

        [Header("Cinemachine References")]
        [SerializeField] private CinemachineCamera cinemachineCam;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Collider2D confinerBounds;

        [Header("Pan & Drag Settings")]
        [Tooltip("Multiplier for pointer drag movement.")]
        [SerializeField] private float dragSpeed = 1.0f;
        [Tooltip("Minimum pointer movement in pixels before a touch/click is treated as a drag instead of a tap.")]
        [SerializeField] private float dragThresholdPixels = 8f;
        [Tooltip("Enable inertial gliding after flicking/dragging.")]
        [SerializeField] private bool enableInertia = true;
        [Tooltip("How fast inertia decays after release.")]
        [SerializeField] private float inertiaDecay = 5.5f;
        [Header("Optional neighboring-home pages")]
        [SerializeField] private float neighborPageWidth;
        [SerializeField] private float neighborFocusY=2f;
        private int gestureStartPage;

        [Header("Keyboard Pan (Editor / Desktop)")]
        [SerializeField] private bool enableKeyboardPan = true;
        [SerializeField] private float keyboardSpeed = 6.5f;
        [SerializeField] private float keyboardSmoothTime = 12f;

        [Header("Smooth Zoom Settings")]
        [SerializeField] private bool enableZoom = true;
        [SerializeField] private float minOrthoSize = 4.2f;
        [SerializeField] private float maxOrthoSize = 7.5f;
        [SerializeField] private float zoomSpeed = 1.8f;
        [SerializeField] private float zoomSmoothDamp = 10f;

        [Header("Default Center (Frames Soul Crystal & Bottom Rows above Tray)")]
        [SerializeField] private Vector2 defaultCenter = new Vector2(0f, -2.35f);
        [SerializeField] private float defaultOrthoSize=6.05f;
        public bool IsBattleFraming {get;private set;}=true;

        // State
        private Camera mainCamera;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 targetPosition;
        private float targetOrthoSize = 6.05f;
        private bool isPointerDragging = false;
        private bool pointerEligibleForDrag = false;
        private Vector2 pointerDownScreenPos;
        private Vector2 lastScreenPos;
        private Vector3 lastWorldDragPos;
        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        public Transform FollowTarget => followTarget;
        public CinemachineCamera CinemachineCam => cinemachineCam;
        public bool IsDragging => isPointerDragging;
        public bool LastGestureWasDrag {get;private set;}
        public Vector3 TargetPosition => targetPosition;
        public bool IsViewingNeighbor=>neighborPageWidth>0&&Mathf.Abs(targetPosition.x)>neighborPageWidth*.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            mainCamera = Camera.main;
            InitializeComponents();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (followTarget != null)
            {
                targetPosition = followTarget.position;
            }
            else
            {
                targetPosition = new Vector3(defaultCenter.x, defaultCenter.y, 0f);
            }

            if (cinemachineCam != null)
            {
                targetOrthoSize = cinemachineCam.Lens.OrthographicSize;
            }
        }

        public void InitializeComponents()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            if (mainCamera != null && cinemachineBrain == null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
                if (cinemachineBrain == null)
                {
                    cinemachineBrain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                }
                cinemachineBrain.LensModeOverride.Enabled = true;
                cinemachineBrain.LensModeOverride.DefaultMode = LensSettings.OverrideModes.Orthographic;
                mainCamera.orthographic = true;
            }

            if (cinemachineCam == null)
            {
                cinemachineCam = FindFirstObjectByType<CinemachineCamera>();
            }

            if (followTarget == null)
            {
                var existingTarget = GameObject.Find("CameraFollowTarget");
                if (existingTarget != null) followTarget = existingTarget.transform;
                else
                {
                    var targetGO = new GameObject("CameraFollowTarget");
                    targetGO.transform.position = new Vector3(defaultCenter.x, defaultCenter.y, 0f);
                    followTarget = targetGO.transform;
                }
            }

            if (confinerBounds == null)
            {
                var confinerObj = GameObject.Find("BackdropConfinerBounds");
                if (confinerObj != null) confinerBounds = confinerObj.GetComponent<Collider2D>();
            }

            if (cinemachineCam != null)
            {
                cinemachineCam.Follow = followTarget;
                cinemachineCam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;

                // Ensure CinemachineFollow exists with smooth damping
                var follow = cinemachineCam.GetComponent<CinemachineFollow>();
                if (follow == null) follow = cinemachineCam.gameObject.AddComponent<CinemachineFollow>();
                follow.FollowOffset = new Vector3(0f, 0f, -10f);
                follow.TrackerSettings.PositionDamping = new Vector3(0.45f, 0.45f, 0f);

                // Ensure CinemachineConfiner2D is wired
                if (confinerBounds != null)
                {
                    var confiner = cinemachineCam.GetComponent<CinemachineConfiner2D>();
                    if (confiner == null) confiner = cinemachineCam.gameObject.AddComponent<CinemachineConfiner2D>();
                    confiner.BoundingShape2D = confinerBounds;
                    confiner.Damping = 0.5f;
                    confiner.OversizeWindow.Enabled = true;
                    confiner.InvalidateBoundingShapeCache();
                }
            }
        }

        private void Update()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || followTarget == null) return;
            if((GameManager.Instance!=null&&GameManager.Instance.IsPaused)||(FirstWaveTutorial.Instance!=null&&FirstWaveTutorial.Instance.Running))
            {currentVelocity=Vector3.zero;pointerEligibleForDrag=false;isPointerDragging=false;return;}

            HandlePointerDrag();
            HandleKeyboardPan();
            HandleZoom();
            ApplyTargetMovement();
        }

        private void HandlePointerDrag()
        {
            bool pointerDown = false;
            bool pointerHeld = false;
            bool pointerUp = false;
            Vector2 screenPos = Vector2.zero;
            bool isSpecialButton = false; // Right or Middle click

            if (Mouse.current != null)
            {
                bool rightDown = Mouse.current.rightButton.wasPressedThisFrame;
                bool rightHeld = Mouse.current.rightButton.isPressed;
                bool rightUp = Mouse.current.rightButton.wasReleasedThisFrame;

                bool midDown = Mouse.current.middleButton.wasPressedThisFrame;
                bool midHeld = Mouse.current.middleButton.isPressed;
                bool midUp = Mouse.current.middleButton.wasReleasedThisFrame;

                if (rightHeld || rightDown || rightUp || midHeld || midDown || midUp)
                {
                    isSpecialButton = true;
                    pointerDown = rightDown || midDown;
                    pointerHeld = rightHeld || midHeld;
                    pointerUp = rightUp || midUp;
                    screenPos = Mouse.current.position.ReadValue();
                }
            }

            if (!isSpecialButton && Pointer.current != null)
            {
                pointerDown = Pointer.current.press.wasPressedThisFrame;
                pointerHeld = Pointer.current.press.isPressed;
                pointerUp = Pointer.current.press.wasReleasedThisFrame;
                screenPos = Pointer.current.position.ReadValue();
            }

            // Pointer Press: check if drag is permitted
            if (pointerDown)
            {
                LastGestureWasDrag=false;
                gestureStartPage=neighborPageWidth>0?Mathf.Clamp(Mathf.RoundToInt(targetPosition.x/neighborPageWidth),-1,1):0;
                pointerEligibleForDrag = false;
                isPointerDragging = false;
                currentVelocity = Vector3.zero;

                // Special buttons (right/middle click) always drag
                if (isSpecialButton)
                {
                    pointerEligibleForDrag = true;
                    isPointerDragging = true;
                    LastGestureWasDrag=true;
                    pointerDownScreenPos = screenPos;
                    lastScreenPos = screenPos;
                    lastWorldDragPos = ScreenToWorld(screenPos);
                    return;
                }

                // Check UI raycast: do not drag if touching UI button/panel
                if (IsPointerOverUI(screenPos))
                {
                    return;
                }

                // Check card placement: do not drag camera if holding/placing a card
                if (PlacementController.Instance != null)
                {
                    if (PlacementController.Instance.IsDragging || PlacementController.Instance.SelectedCard != null)
                    {
                        return;
                    }
                }

                pointerEligibleForDrag = true;
                pointerDownScreenPos = screenPos;
                lastScreenPos = screenPos;
                lastWorldDragPos = ScreenToWorld(screenPos);
            }

            // Pointer Held
            if (pointerHeld && pointerEligibleForDrag)
            {
                if (!isPointerDragging)
                {
                    if (Vector2.Distance(screenPos, pointerDownScreenPos) >= dragThresholdPixels)
                    {
                        isPointerDragging = true;
                        LastGestureWasDrag=true;
                        lastScreenPos = screenPos;
                        lastWorldDragPos = ScreenToWorld(screenPos);
                    }
                }

                if (isPointerDragging)
                {
                    IsBattleFraming=false;
                    Vector3 currentWorldPos = ScreenToWorld(screenPos);
                    Vector3 worldDelta = currentWorldPos - lastWorldDragPos;

                    if (worldDelta.sqrMagnitude > 0.000001f)
                    {
                        targetPosition -= worldDelta * dragSpeed;
                        targetPosition.z = 0f;

                        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
                        currentVelocity = (-worldDelta * dragSpeed) / dt;

                        // Recompute world pos after target moved
                        lastWorldDragPos = ScreenToWorld(screenPos);
                    }
                    lastScreenPos = screenPos;
                }
            }

            // Pointer Release
            if (pointerUp)
            {
                var swipe=screenPos-pointerDownScreenPos;
                if(isPointerDragging&&neighborPageWidth>0&&Mathf.Abs(swipe.x)>Screen.width*.18f&&Mathf.Abs(swipe.x)>Mathf.Abs(swipe.y)*1.4f)
                    VisitNeighbor(gestureStartPage+(swipe.x<0?1:-1));
                if (isPointerDragging)
                {
                    if (!enableInertia)
                    {
                        currentVelocity = Vector3.zero;
                    }
                }
                isPointerDragging = false;
                pointerEligibleForDrag = false;
            }
        }

        private void HandleKeyboardPan()
        {
            if (!enableKeyboardPan || Keyboard.current == null || isPointerDragging) return;

            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;

            // Recenter key (Space or R)
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame)
            {
                RecenterCamera();
                return;
            }

            if (moveInput.sqrMagnitude > 0.01f)
            {
                IsBattleFraming=false;
                moveInput.Normalize();
                Vector3 targetVel = new Vector3(moveInput.x, moveInput.y, 0f) * keyboardSpeed;
                currentVelocity = Vector3.Lerp(currentVelocity, targetVel, Time.unscaledDeltaTime * keyboardSmoothTime);
            }
        }

        private void HandleZoom()
        {
            if (!enableZoom || cinemachineCam == null) return;

            float scroll = 0f;
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y;
            }

            if (Mathf.Abs(scroll) > 0.01f)
            {
                IsBattleFraming=false;
                float zoomDelta = -Mathf.Sign(scroll) * zoomSpeed * 0.35f;
                targetOrthoSize = Mathf.Clamp(targetOrthoSize + zoomDelta, minOrthoSize, Mathf.Max(maxOrthoSize,defaultOrthoSize));
            }

            // Smoothly apply zoom to Cinemachine camera lens
            float currentSize = cinemachineCam.Lens.OrthographicSize;
            if (!Mathf.Approximately(currentSize, targetOrthoSize))
            {
                float newSize = Mathf.Lerp(currentSize, targetOrthoSize, Time.unscaledDeltaTime * zoomSmoothDamp);
                cinemachineCam.Lens.OrthographicSize = newSize;
            }
        }

        private void ApplyTargetMovement()
        {
            // Apply inertial gliding when not directly dragging
            if (!isPointerDragging && currentVelocity.sqrMagnitude > 0.001f)
            {
                targetPosition += currentVelocity * Time.unscaledDeltaTime;
                targetPosition.z = 0f;

                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.unscaledDeltaTime * inertiaDecay);
                if (currentVelocity.sqrMagnitude < 0.001f)
                {
                    currentVelocity = Vector3.zero;
                }
            }

            // Clamp follow target within background domain bounds
            if (confinerBounds != null)
            {
                Bounds b = confinerBounds.bounds;
                targetPosition.x = Mathf.Clamp(targetPosition.x, b.min.x, b.max.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, b.min.y, b.max.y);
            }

            // Update follow target transform
            followTarget.position = targetPosition;
        }

        public void RecenterCamera(bool instant = false)
        {
            IsBattleFraming=true;
            targetPosition = new Vector3(defaultCenter.x, defaultCenter.y, 0f);
            currentVelocity = Vector3.zero;
            targetOrthoSize = defaultOrthoSize;

            if (instant && followTarget != null)
            {
                followTarget.position = targetPosition;
                if (cinemachineCam != null)
                {
                    cinemachineCam.transform.position = new Vector3(defaultCenter.x, defaultCenter.y, -10f);
                    cinemachineCam.Lens.OrthographicSize = defaultOrthoSize;
                }
            }
        }

        public void SetTargetPosition(Vector2 pos)
        {
            IsBattleFraming=false;
            targetPosition = new Vector3(pos.x, pos.y, 0f);
            currentVelocity = Vector3.zero;
        }
        public void VisitNeighbor(int page)
        {
            if(neighborPageWidth<=0)return;
            page=Mathf.Clamp(page,-1,1);
            if(page==0){RecenterCamera();return;}
            SetTargetPosition(new Vector2(page*neighborPageWidth,page==0?defaultCenter.y:neighborFocusY));
            targetOrthoSize=6.05f;
        }

        public void ConfigureBattleFrame(Vector2 center,float orthoSize,bool instant=false)
        {
            defaultCenter=center;defaultOrthoSize=orthoSize;
            if(IsBattleFraming)RecenterCamera(instant);
        }

        public void SetZoom(float orthoSize)
        {
            IsBattleFraming=false;
            targetOrthoSize = Mathf.Clamp(orthoSize, minOrthoSize, Mathf.Max(maxOrthoSize,defaultOrthoSize));
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return Vector3.zero;

            float zDist = -mainCamera.transform.position.z;
            Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
            world.z = 0f;
            return world;
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, uiRaycastResults);

            // Filter out raycasts that hit non-interactive UI if needed
            return uiRaycastResults.Count > 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (confinerBounds != null)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
                Gizmos.DrawWireCube(confinerBounds.bounds.center, confinerBounds.bounds.size);
            }

            if (followTarget != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(followTarget.position, 0.35f);
            }
        }
    }
}
