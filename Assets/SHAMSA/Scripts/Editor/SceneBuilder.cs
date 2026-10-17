using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using SoulCrystalDefense.Data;
using SoulCrystalDefense.Combat;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.Units;
using SoulCrystalDefense.Systems;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class SceneBuilder
    {
        [MenuItem("SoulCrystalDefense/Build Vertical Slice Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
            camObj.AddComponent<AudioListener>();
            var respCam = camObj.AddComponent<ResponsiveCamera>();

            // 2. Event System with InputSystemUIInputModule
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                eventObj.AddComponent(inputModuleType);
            }
            else
            {
                eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 3. Environment Background
            var envObj = new GameObject("Environment");
            var bgObj = new GameObject("GraveyardBackdrop");
            bgObj.transform.SetParent(envObj.transform);
            bgObj.transform.position = new Vector3(0f, 0f, 0f);
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Concept/Portrait/01-ForgottenGraveyard-CleanBoard.png");
            if (bgSprite != null)
            {
                bgSr.sprite = bgSprite;
                // Scale so 1672px height fills ~12 units (scale approx 0.72)
                bgObj.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            }
            bgSr.sortingOrder = -20;

            // 4. Board Grid
            var tileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/TileCell_Sprite.png");
            var gridObj = new GameObject("BoardGrid");
            var boardGrid = gridObj.AddComponent<BoardGrid>();
            boardGrid.EnsureGridGenerated();

            // Apply tile cell sprite to all cells
            var allCells = gridObj.GetComponentsInChildren<PlacementCell>();
            foreach (var c in allCells)
            {
                var sr = c.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (tileSprite != null) sr.sprite = tileSprite;
                    sr.sortingOrder = -5;
                }
            }

            // 5. Portals (5 lanes at top)
            var portalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/Portal_Sprite.png");
            var portalsObj = new GameObject("Portals");
            var portalVisuals = new List<PortalVisual>();
            for (int l = 0; l < 5; l++)
            {
                var pObj = new GameObject($"Portal_Lane_{l}");
                pObj.transform.SetParent(portalsObj.transform);
                pObj.transform.position = boardGrid.GetPortalPosition(l);

                var pSr = pObj.AddComponent<SpriteRenderer>();
                if (portalSprite != null)
                {
                    pSr.sprite = portalSprite;
                    pObj.transform.localScale = new Vector3(0.20f, 0.22f, 1f);
                }
                else
                {
                    pSr.sprite = HealthBar.GetWhitePixelSprite();
                    pObj.transform.localScale = new Vector3(0.8f, 0.35f, 1f);
                }
                pSr.sortingOrder = -2;

                var pv = pObj.AddComponent<PortalVisual>();
                pv.Initialize(l);
                portalVisuals.Add(pv);
            }

            // 6. Soul Crystal Platform (at bottom)
            var crystalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/SoulCrystal_Sprite.png");
            var crystalObj = new GameObject("SoulCrystalPlatform");
            crystalObj.transform.position = new Vector3(0f, boardGrid.CrystalY, 0f);

            var crystalBase = new GameObject("PlatformBase");
            crystalBase.transform.SetParent(crystalObj.transform, false);
            var baseSr = crystalBase.AddComponent<SpriteRenderer>();
            baseSr.sprite = HealthBar.GetWhitePixelSprite();
            baseSr.color = new Color(0.08f, 0.14f, 0.22f, 0.9f);
            baseSr.sortingOrder = 1;
            crystalBase.transform.localScale = new Vector3(2.5f, 0.4f, 1f);

            var crystalVisual = new GameObject("CrystalCore");
            crystalVisual.transform.SetParent(crystalObj.transform, false);
            crystalVisual.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            var coreSr = crystalVisual.AddComponent<SpriteRenderer>();
            if (crystalSprite != null)
            {
                coreSr.sprite = crystalSprite;
                crystalVisual.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            }
            else
            {
                coreSr.sprite = HealthBar.GetWhitePixelSprite();
                coreSr.color = new Color(0.2f, 0.9f, 1f, 1f);
                crystalVisual.transform.localScale = new Vector3(0.7f, 1.0f, 1f);
            }
            coreSr.sortingOrder = 3;

            var soulCrystal = crystalObj.AddComponent<SoulCrystal>();
            SetPrivateField(soulCrystal, "crystalRenderer", coreSr);
            SetPrivateField(soulCrystal, "crystalVisualTransform", crystalVisual.transform);

            // 7. Managers
            var managersObj = new GameObject("GameManagers");
            var gameManager = managersObj.AddComponent<GameManager>();
            var energyWallet = managersObj.AddComponent<SoulEnergyWallet>();
            var waveDirector = managersObj.AddComponent<WaveDirector>();
            var soundManager = managersObj.AddComponent<SoundManager>();
            var qaAutomator = managersObj.AddComponent<GameplayQAAutomator>();
            qaAutomator.SetAutoRun(false);

            // Load Game Data
            var levelData = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/SHAMSA/GameplayData/Level01_ForgottenGraveyard.asset");
            var dwarfData = AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/DwarfGuardian_Data.asset");
            var chefData = AssetDatabase.LoadAssetAtPath<DefenderData>("Assets/SHAMSA/GameplayData/BloodChef_Data.asset");
            var boneData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/BoneRaider_Data.asset");
            var soulData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/SHAMSA/GameplayData/SoulReaper_Data.asset");
            var bonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/BoneRaider.prefab");
            var soulPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SHAMSA/Prefabs/SoulReaper.prefab");

            // Wire WaveDirector
            SetPrivateField(waveDirector, "boneRaiderPrefab", bonePrefab);
            SetPrivateField(waveDirector, "soulReaperPrefab", soulPrefab);
            SetPrivateField(waveDirector, "boneRaiderData", boneData);
            SetPrivateField(waveDirector, "soulReaperData", soulData);
            waveDirector.RegisterPortals(portalVisuals.ToArray());

            // 8. UI Canvas
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvasObj = new GameObject("HUDCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 10f;
            canvas.sortingOrder = 50;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Safe Area Root
            var safeAreaObj = new GameObject("SafeAreaRoot");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
            safeAreaObj.AddComponent<SafeAreaFitter>();

            // Top HUD
            var topHudObj = new GameObject("TopHUD");
            topHudObj.transform.SetParent(safeAreaObj.transform, false);
            var topRect = topHudObj.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 160f);

            var topBg = topHudObj.AddComponent<Image>();
            topBg.color = new Color(0.06f, 0.08f, 0.12f, 0.85f);

            // Top HUD Elements: Crystal Health (Left)
            var hpTextObj = CreateText(topHudObj, "CrystalHealthText", "CRYSTAL: 20/20", defaultFont, 32, TextAnchor.MiddleLeft);
            var hpRect = hpTextObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0f, 0.5f);
            hpRect.anchorMax = new Vector2(0f, 0.5f);
            hpRect.pivot = new Vector2(0f, 0.5f);
            hpRect.anchoredPosition = new Vector2(40f, 0f);
            hpRect.sizeDelta = new Vector2(300f, 80f);
            var hpText = hpTextObj.GetComponent<Text>();
            hpText.color = new Color(0.3f, 0.95f, 1f, 1f);

            // Wave Text (Center)
            var waveTextObj = CreateText(topHudObj, "WaveText", "WAVE 1/1", defaultFont, 36, TextAnchor.MiddleCenter);
            var waveRect = waveTextObj.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 0.5f);
            waveRect.anchorMax = new Vector2(0.5f, 0.5f);
            waveRect.pivot = new Vector2(0.5f, 0.5f);
            waveRect.anchoredPosition = Vector2.zero;
            waveRect.sizeDelta = new Vector2(300f, 80f);
            var waveText = waveTextObj.GetComponent<Text>();
            waveText.fontStyle = FontStyle.Bold;
            waveText.color = Color.white;

            // Soul Energy (Right)
            var energyTextObj = CreateText(topHudObj, "EnergyText", "SOUL: 250", defaultFont, 32, TextAnchor.MiddleRight);
            var energyRect = energyTextObj.GetComponent<RectTransform>();
            energyRect.anchorMin = new Vector2(1f, 0.5f);
            energyRect.anchorMax = new Vector2(1f, 0.5f);
            energyRect.pivot = new Vector2(1f, 0.5f);
            energyRect.anchoredPosition = new Vector2(-190f, 0f);
            energyRect.sizeDelta = new Vector2(250f, 80f);
            var energyText = energyTextObj.GetComponent<Text>();
            energyText.color = new Color(1f, 0.85f, 0.2f, 1f);

            // Pause Button (Far Right)
            var pauseBtnObj = new GameObject("PauseButton");
            pauseBtnObj.transform.SetParent(topHudObj.transform, false);
            var pauseRect = pauseBtnObj.AddComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1f, 0.5f);
            pauseRect.anchorMax = new Vector2(1f, 0.5f);
            pauseRect.pivot = new Vector2(1f, 0.5f);
            pauseRect.anchoredPosition = new Vector2(-30f, 0f);
            pauseRect.sizeDelta = new Vector2(130f, 70f);
            var pauseImg = pauseBtnObj.AddComponent<Image>();
            pauseImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);
            var pauseBtn = pauseBtnObj.AddComponent<Button>();

            var pauseLabelObj = CreateText(pauseBtnObj, "Label", "PAUSE", defaultFont, 24, TextAnchor.MiddleCenter);
            var pauseLabelText = pauseLabelObj.GetComponent<Text>();
            pauseLabelText.color = Color.white;
            StretchRect(pauseLabelObj.GetComponent<RectTransform>());

            var gameplayHud = topHudObj.AddComponent<GameplayHUD>();
            SetPrivateField(gameplayHud, "crystalHealthText", hpText);
            SetPrivateField(gameplayHud, "waveText", waveText);
            SetPrivateField(gameplayHud, "energyText", energyText);
            SetPrivateField(gameplayHud, "pauseButton", pauseBtn);
            SetPrivateField(gameplayHud, "pauseButtonText", pauseLabelText);

            // Countdown Container
            var cdObj = new GameObject("CountdownContainer");
            cdObj.transform.SetParent(safeAreaObj.transform, false);
            var cdRect = cdObj.AddComponent<RectTransform>();
            cdRect.anchorMin = new Vector2(0.5f, 0.6f);
            cdRect.anchorMax = new Vector2(0.5f, 0.6f);
            cdRect.sizeDelta = new Vector2(500f, 140f);

            var cdTextObj = CreateText(cdObj, "CountdownText", "READY!", defaultFont, 72, TextAnchor.MiddleCenter);
            var cdText = cdTextObj.GetComponent<Text>();
            cdText.color = new Color(1f, 0.9f, 0.2f, 1f);
            cdText.fontStyle = FontStyle.Bold;
            StretchRect(cdTextObj.GetComponent<RectTransform>());

            SetPrivateField(gameplayHud, "countdownContainer", cdObj);
            SetPrivateField(gameplayHud, "countdownText", cdText);
            cdObj.SetActive(false);

            // Bottom Card Tray
            var trayObj = new GameObject("BottomTray");
            trayObj.transform.SetParent(safeAreaObj.transform, false);
            var trayRect = trayObj.AddComponent<RectTransform>();
            trayRect.anchorMin = new Vector2(0f, 0f);
            trayRect.anchorMax = new Vector2(1f, 0f);
            trayRect.pivot = new Vector2(0.5f, 0f);
            trayRect.sizeDelta = new Vector2(0f, 220f);

            var trayBg = trayObj.AddComponent<Image>();
            trayBg.color = new Color(0.05f, 0.07f, 0.1f, 0.92f);

            var cardsContainer = new GameObject("CardsLayout");
            cardsContainer.transform.SetParent(trayObj.transform, false);
            var cardsLayoutRect = cardsContainer.AddComponent<RectTransform>();
            StretchRect(cardsLayoutRect);
            var hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 30f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var cardUIs = new List<DefenderCardUI>();

            // Card 1: Dwarf
            var dwarfCard = CreateCard(cardsContainer, "Card_Dwarf", dwarfData, false, "", defaultFont);
            cardUIs.Add(dwarfCard);

            // Card 2: Chef
            var chefCard = CreateCard(cardsContainer, "Card_Chef", chefData, false, "", defaultFont);
            cardUIs.Add(chefCard);

            // Card 3: Elephant (Locked preview)
            var elephantCard = CreateCard(cardsContainer, "Card_Elephant", null, true, "BOSS\nSOON", defaultFont);
            cardUIs.Add(elephantCard);

            // Card 4: Train (Locked ultimate)
            var trainCard = CreateCard(cardsContainer, "Card_Train", null, true, "WAR TRAIN\nLOCKED", defaultFont);
            cardUIs.Add(trainCard);

            // Placement Controller
            var placementCtrl = trayObj.AddComponent<PlacementController>();
            SetPrivateField(placementCtrl, "cards", cardUIs);

            // 9. Victory / Defeat Panel
            var resultPanelObj = new GameObject("VictoryDefeatModal");
            resultPanelObj.transform.SetParent(canvasObj.transform, false);
            var rpRect = resultPanelObj.AddComponent<RectTransform>();
            StretchRect(rpRect);
            var rpBg = resultPanelObj.AddComponent<Image>();
            rpBg.color = new Color(0f, 0f, 0f, 0.85f);

            var vContainer = new GameObject("VictoryContent");
            vContainer.transform.SetParent(resultPanelObj.transform, false);
            var vcRect = vContainer.AddComponent<RectTransform>();
            StretchRect(vcRect);

            var vTitleObj = CreateText(vContainer, "VictoryTitle", "VICTORY!", defaultFont, 76, TextAnchor.MiddleCenter);
            var vTitleRect = vTitleObj.GetComponent<RectTransform>();
            vTitleRect.anchoredPosition = new Vector2(0f, 350f);
            vTitleRect.sizeDelta = new Vector2(900f, 130f);
            var vTitleText = vTitleObj.GetComponent<Text>();
            vTitleText.color = new Color(1f, 0.85f, 0.2f, 1f);
            vTitleText.fontStyle = FontStyle.Bold;
            vTitleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            // Stars Row
            var starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/Star_Sprite.png");
            var starsContainer = new GameObject("StarsRow");
            starsContainer.transform.SetParent(vContainer.transform, false);
            var starsRect = starsContainer.AddComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(0.5f, 0.5f);
            starsRect.anchorMax = new Vector2(0.5f, 0.5f);
            starsRect.pivot = new Vector2(0.5f, 0.5f);
            starsRect.anchoredPosition = new Vector2(0f, 220f);
            starsRect.sizeDelta = new Vector2(320f, 80f);

            var starsHlg = starsContainer.AddComponent<HorizontalLayoutGroup>();
            starsHlg.spacing = 24f;
            starsHlg.childAlignment = TextAnchor.MiddleCenter;
            starsHlg.childControlWidth = false;
            starsHlg.childControlHeight = false;

            var starImagesList = new List<Image>();
            for (int s = 0; s < 3; s++)
            {
                var starObj = new GameObject($"Star_{s + 1}");
                starObj.transform.SetParent(starsContainer.transform, false);
                var sRect = starObj.AddComponent<RectTransform>();
                sRect.sizeDelta = new Vector2(76f, 76f);
                var sImg = starObj.AddComponent<Image>();
                if (starSprite != null) sImg.sprite = starSprite;
                sImg.color = new Color(1f, 0.85f, 0.1f, 1f);
                starImagesList.Add(sImg);
            }

            var vFragObj = CreateText(vContainer, "RewardFragment", "+1 Crystal Fragment", defaultFont, 38, TextAnchor.MiddleCenter);
            var vFragRect = vFragObj.GetComponent<RectTransform>();
            vFragRect.anchoredPosition = new Vector2(0f, 120f);
            vFragRect.sizeDelta = new Vector2(900f, 60f);
            var vFragText = vFragObj.GetComponent<Text>();
            vFragText.color = new Color(0.3f, 0.95f, 1f, 1f);
            vFragText.fontStyle = FontStyle.Bold;
            vFragText.horizontalOverflow = HorizontalWrapMode.Overflow;

            var vTotalObj = CreateText(vContainer, "TotalFragments", "Total Fragments: 1 / 3", defaultFont, 30, TextAnchor.MiddleCenter);
            var vTotalRect = vTotalObj.GetComponent<RectTransform>();
            vTotalRect.anchoredPosition = new Vector2(0f, 50f);
            vTotalRect.sizeDelta = new Vector2(900f, 50f);
            var vTotalText = vTotalObj.GetComponent<Text>();
            vTotalText.color = Color.white;
            vTotalText.horizontalOverflow = HorizontalWrapMode.Overflow;

            var vPlayAgainObj = CreateButton(vContainer, "PlayAgainBtn", "PLAY AGAIN", defaultFont, new Vector2(0f, -60f), new Vector2(380f, 90f));
            var vNextObj = CreateButton(vContainer, "NextBtn", "CONTINUE", defaultFont, new Vector2(0f, -180f), new Vector2(380f, 90f));

            // Defeat Content
            var dContainer = new GameObject("DefeatContent");
            dContainer.transform.SetParent(resultPanelObj.transform, false);
            var dcRect = dContainer.AddComponent<RectTransform>();
            StretchRect(dcRect);

            var dTitleObj = CreateText(dContainer, "DefeatTitle", "CRYSTAL BREACHED", defaultFont, 64, TextAnchor.MiddleCenter);
            var dTitleRect = dTitleObj.GetComponent<RectTransform>();
            dTitleRect.anchoredPosition = new Vector2(0f, 260f);
            dTitleRect.sizeDelta = new Vector2(900f, 120f);
            var dTitleText = dTitleObj.GetComponent<Text>();
            dTitleText.color = new Color(1f, 0.3f, 0.3f, 1f);
            dTitleText.fontStyle = FontStyle.Bold;
            dTitleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            var dHintObj = CreateText(dContainer, "DefeatHint", "Place Blood Chef behind Dwarf Guardian\nto splash grouped enemies!", defaultFont, 32, TextAnchor.MiddleCenter);
            var dHintRect = dHintObj.GetComponent<RectTransform>();
            dHintRect.anchoredPosition = new Vector2(0f, 100f);
            dHintRect.sizeDelta = new Vector2(800f, 120f);
            var dHintText = dHintObj.GetComponent<Text>();
            dHintText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            var dRetryObj = CreateButton(dContainer, "RetryBtn", "RETRY WAVE", defaultFont, new Vector2(0f, -120f), new Vector2(360f, 90f));

            var resultPanel = resultPanelObj.AddComponent<VictoryDefeatPanel>();
            SetPrivateField(resultPanel, "panelRoot", resultPanelObj);
            SetPrivateField(resultPanel, "victoryContainer", vContainer);
            SetPrivateField(resultPanel, "defeatContainer", dContainer);
            SetPrivateField(resultPanel, "victoryTitleText", vTitleText);
            SetPrivateField(resultPanel, "starImages", starImagesList.ToArray());
            SetPrivateField(resultPanel, "rewardFragmentText", vFragText);
            SetPrivateField(resultPanel, "totalFragmentsText", vTotalText);
            SetPrivateField(resultPanel, "victoryReplayButton", vPlayAgainObj.GetComponent<Button>());
            SetPrivateField(resultPanel, "victoryNextButton", vNextObj.GetComponent<Button>());
            SetPrivateField(resultPanel, "defeatTitleText", dTitleText);
            SetPrivateField(resultPanel, "defeatHintText", dHintText);
            SetPrivateField(resultPanel, "defeatRetryButton", dRetryObj.GetComponent<Button>());

            resultPanelObj.SetActive(false);

            // Wire GameManager
            SetPrivateField(gameManager, "currentLevelData", levelData);
            SetPrivateField(gameManager, "energyWallet", energyWallet);
            SetPrivateField(gameManager, "soulCrystal", soulCrystal);
            SetPrivateField(gameManager, "waveDirector", waveDirector);
            SetPrivateField(gameManager, "gameplayHUD", gameplayHud);
            SetPrivateField(gameManager, "victoryDefeatPanel", resultPanel);

            // Save Scene
            string scenePath = "Assets/Scenes/SoulCrystalDefenseGameplay.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();

            Debug.Log("SoulCrystalDefenseGameplay scene successfully built and saved!");
        }

        private static DefenderCardUI CreateCard(GameObject parent, string name, DefenderData data, bool isLocked, string lockLabel, Font font)
        {
            var cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent.transform, false);
            var cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(190f, 180f);

            var bgImg = cardObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);
            var btn = cardObj.AddComponent<Button>();

            // Icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(120f, 120f);
            var iconImg = iconObj.AddComponent<Image>();
            if (data != null && data.cardIcon != null)
                iconImg.sprite = data.cardIcon;

            // Name
            var nameObj = CreateText(cardObj, "NameText", data != null ? data.displayName : "Unit", font, 20, TextAnchor.UpperCenter);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, -6f);
            nameRect.sizeDelta = new Vector2(0f, 30f);

            // Cost Badge
            var costBadgeObj = new GameObject("CostBadge");
            costBadgeObj.transform.SetParent(cardObj.transform, false);
            var costBadgeRect = costBadgeObj.AddComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(1f, 0f);
            costBadgeRect.anchorMax = new Vector2(1f, 0f);
            costBadgeRect.pivot = new Vector2(1f, 0f);
            costBadgeRect.anchoredPosition = new Vector2(-6f, 6f);
            costBadgeRect.sizeDelta = new Vector2(65f, 36f);
            var badgeImg = costBadgeObj.AddComponent<Image>();
            badgeImg.color = new Color(0.1f, 0.3f, 0.5f, 0.95f);

            var costTextObj = CreateText(costBadgeObj, "CostText", data != null ? data.soulCost.ToString() : "--", font, 20, TextAnchor.MiddleCenter);
            StretchRect(costTextObj.GetComponent<RectTransform>());
            var costText = costTextObj.GetComponent<Text>();
            costText.color = new Color(1f, 0.9f, 0.2f, 1f);
            costText.fontStyle = FontStyle.Bold;

            // Selection Border
            var borderObj = new GameObject("SelectionBorder");
            borderObj.transform.SetParent(cardObj.transform, false);
            var borderRect = borderObj.AddComponent<RectTransform>();
            StretchRect(borderRect);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.2f, 0.9f, 1f, 0.8f);
            borderObj.SetActive(false);

            // Unaffordable Overlay
            var unaffordObj = new GameObject("UnaffordableOverlay");
            unaffordObj.transform.SetParent(cardObj.transform, false);
            var unaffordRect = unaffordObj.AddComponent<RectTransform>();
            StretchRect(unaffordRect);
            var unaffordImg = unaffordObj.AddComponent<Image>();
            unaffordImg.color = new Color(0f, 0f, 0f, 0.6f);
            unaffordObj.SetActive(false);

            // Lock Overlay
            var lockObj = new GameObject("LockOverlay");
            lockObj.transform.SetParent(cardObj.transform, false);
            var lockRect = lockObj.AddComponent<RectTransform>();
            StretchRect(lockRect);
            var lockImg = lockObj.AddComponent<Image>();
            lockImg.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

            var lockLabelObj = CreateText(lockObj, "LockText", lockLabel, font, 22, TextAnchor.MiddleCenter);
            StretchRect(lockLabelObj.GetComponent<RectTransform>());
            var lockLabelText = lockLabelObj.GetComponent<Text>();
            lockLabelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            lockObj.SetActive(isLocked);

            var cardUI = cardObj.AddComponent<DefenderCardUI>();
            SetPrivateField(cardUI, "defenderData", data);
            SetPrivateField(cardUI, "isLocked", isLocked);
            SetPrivateField(cardUI, "lockedLabel", lockLabel);
            SetPrivateField(cardUI, "iconImage", iconImg);
            SetPrivateField(cardUI, "nameText", nameObj.GetComponent<Text>());
            SetPrivateField(cardUI, "costText", costText);
            SetPrivateField(cardUI, "selectionBorder", borderImg);
            SetPrivateField(cardUI, "lockOverlay", lockObj);
            SetPrivateField(cardUI, "lockText", lockLabelText);
            SetPrivateField(cardUI, "unaffordableOverlay", unaffordObj);
            SetPrivateField(cardUI, "cardButton", btn);

            return cardUI;
        }

        private static GameObject CreateText(GameObject parent, string name, string content, Font font, int size, TextAnchor align)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = size;
            txt.alignment = align;
            txt.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label, Font font, Vector2 pos, Vector2 size)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.8f, 1f);
            var btn = btnObj.AddComponent<Button>();

            var lblObj = CreateText(btnObj, "Label", label, font, 28, TextAnchor.MiddleCenter);
            StretchRect(lblObj.GetComponent<RectTransform>());
            var lbl = lblObj.GetComponent<Text>();
            lbl.color = Color.white;
            lbl.fontStyle = FontStyle.Bold;

            return btnObj;
        }

        private static void StretchRect(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
