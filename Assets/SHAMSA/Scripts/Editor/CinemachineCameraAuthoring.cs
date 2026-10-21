using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;
using SoulCrystalDefense.Systems;

namespace SoulCrystalDefense.EditorTools
{
    public static class CinemachineCameraAuthoring
    {
        [MenuItem("SoulCrystalDefense/Camera/Setup Cinemachine Movable Camera")]
        public static void SetupCinemachineCamera()
        {
            var activeScene = EditorSceneManager.GetActiveScene();

            // 1. Find Main Camera
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var camGO = GameObject.Find("Main Camera");
                if (camGO != null) mainCamera = camGO.GetComponent<Camera>();
            }

            if (mainCamera == null)
            {
                Debug.LogError("[CinemachineAuthoring] Main Camera not found in active scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(mainCamera.gameObject, "Setup Cinemachine Camera");

            // Ensure Camera is orthographic
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 6.05f;

            // Ensure CinemachineBrain is on Main Camera
            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(mainCamera.gameObject);
            }
            brain.LensModeOverride.Enabled = true;
            brain.LensModeOverride.DefaultMode = LensSettings.OverrideModes.Orthographic;
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            brain.DefaultBlend.Time = 0.4f;
            EditorUtility.SetDirty(brain);
            EditorUtility.SetDirty(mainCamera);

            // 2. Adjust GraveyardBackdrop so it spans from beneath the Soul Crystal to above the Portals
            var backdrop = GameObject.Find("Environment/GraveyardBackdrop");
            if (backdrop == null) backdrop = GameObject.Find("GraveyardBackdrop");

            if (backdrop != null)
            {
                Undo.RegisterCompleteObjectUndo(backdrop.transform, "Adjust Backdrop Transform");
                backdrop.transform.position = new Vector3(0f, 0f, 0f);
                backdrop.transform.localScale = new Vector3(1.015f, 1.05f, 1f);
                EditorUtility.SetDirty(backdrop);
            }

            Bounds bgBounds;
            if (backdrop != null && backdrop.TryGetComponent<SpriteRenderer>(out var sr))
            {
                bgBounds = sr.bounds;
            }
            else
            {
                bgBounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(9.5f, 17.65f, 0.2f));
            }

            Vector2 center = bgBounds.center;
            Vector2 extents = bgBounds.extents;

            // 3. Create or update BackdropConfinerBounds GameObject
            var env = GameObject.Find("Environment");
            Transform envTransform = env != null ? env.transform : null;

            var confinerGO = GameObject.Find("BackdropConfinerBounds");
            if (confinerGO == null)
            {
                confinerGO = new GameObject("BackdropConfinerBounds");
                if (envTransform != null) confinerGO.transform.SetParent(envTransform, false);
                Undo.RegisterCreatedObjectUndo(confinerGO, "Create BackdropConfinerBounds");
            }
            confinerGO.transform.position = Vector3.zero;

            var poly = confinerGO.GetComponent<PolygonCollider2D>();
            if (poly == null) poly = confinerGO.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;

            // 4 points of the background domain rectangle
            poly.points = new Vector2[]
            {
                new Vector2(center.x - extents.x, center.y - extents.y),
                new Vector2(center.x + extents.x, center.y - extents.y),
                new Vector2(center.x + extents.x, center.y + extents.y),
                new Vector2(center.x - extents.x, center.y + extents.y)
            };
            EditorUtility.SetDirty(poly);
            EditorUtility.SetDirty(confinerGO);

            // 4. Default tactical center: (0, -2.35) frames the Soul Crystal and bottom rows right above the card tray
            Vector3 tacticalCenter = new Vector3(0f, -2.35f, 0f);

            // Create or update CameraFollowTarget
            var targetGO = GameObject.Find("CameraFollowTarget");
            if (targetGO == null)
            {
                targetGO = new GameObject("CameraFollowTarget");
                Undo.RegisterCreatedObjectUndo(targetGO, "Create CameraFollowTarget");
            }
            targetGO.transform.position = tacticalCenter;
            EditorUtility.SetDirty(targetGO);

            // 5. Create or update CinemachineCamera
            var vcamGO = GameObject.Find("CinemachineCamera");
            if (vcamGO == null)
            {
                vcamGO = new GameObject("CinemachineCamera");
                Undo.RegisterCreatedObjectUndo(vcamGO, "Create CinemachineCamera");
            }
            vcamGO.transform.position = new Vector3(tacticalCenter.x, tacticalCenter.y, -10f);

            var vcam = vcamGO.GetComponent<CinemachineCamera>();
            if (vcam == null) vcam = vcamGO.AddComponent<CinemachineCamera>();

            vcam.Follow = targetGO.transform;
            vcam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            vcam.Lens.OrthographicSize = 6.05f;
            vcam.Lens.NearClipPlane = 0.3f;
            vcam.Lens.FarClipPlane = 1000f;

            // Add & configure CinemachineFollow
            var follow = vcamGO.GetComponent<CinemachineFollow>();
            if (follow == null) follow = vcamGO.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 0f, -10f);
            follow.TrackerSettings.PositionDamping = new Vector3(0.45f, 0.45f, 0f);

            // Add & configure CinemachineConfiner2D
            var confiner = vcamGO.GetComponent<CinemachineConfiner2D>();
            if (confiner == null) confiner = vcamGO.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = poly;
            confiner.Damping = 0.5f;
            confiner.OversizeWindow.Enabled = true;
            confiner.InvalidateBoundingShapeCache();
            confiner.BakeBoundingShape(vcam, 1f);

            // 6. Add & configure CinemachineCameraPanController
            var panController = vcamGO.GetComponent<CinemachineCameraPanController>();
            if (panController == null) panController = vcamGO.AddComponent<CinemachineCameraPanController>();
            panController.InitializeComponents();

            // Set serialized fields through SerializedObject for clean persistence
            var so = new SerializedObject(panController);
            so.FindProperty("cinemachineCam").objectReferenceValue = vcam;
            so.FindProperty("cinemachineBrain").objectReferenceValue = brain;
            so.FindProperty("followTarget").objectReferenceValue = targetGO.transform;
            so.FindProperty("confinerBounds").objectReferenceValue = poly;
            so.FindProperty("defaultCenter").vector2Value = new Vector2(tacticalCenter.x, tacticalCenter.y);
            so.FindProperty("minOrthoSize").floatValue = 4.2f;
            so.FindProperty("maxOrthoSize").floatValue = 7.5f;
            so.FindProperty("dragSpeed").floatValue = 1.0f;
            so.FindProperty("enableInertia").boolValue = true;
            so.FindProperty("inertiaDecay").floatValue = 5.5f;
            so.FindProperty("enableKeyboardPan").boolValue = true;
            so.FindProperty("keyboardSpeed").floatValue = 7.0f;
            so.FindProperty("enableZoom").boolValue = true;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(panController);
            EditorUtility.SetDirty(vcamGO);

            // 7. Save Scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CinemachineAuthoring] ✅ Smooth Cinemachine Camera successfully configured!");
            Debug.Log($"[CinemachineAuthoring] Default tactical center set to {tacticalCenter} (Start of land fully visible)");
            Debug.Log($"[CinemachineAuthoring] Background domain: min={poly.points[0]}, max={poly.points[2]}");
        }
    }
}
