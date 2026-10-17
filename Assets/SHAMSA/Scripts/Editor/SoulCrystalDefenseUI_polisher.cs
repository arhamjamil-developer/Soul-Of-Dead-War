using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    /// <summary>
    /// Applies the production HUD treatment to the authored gameplay scene without
    /// rebuilding gameplay objects or changing any combat/placement configuration.
    /// The operation is intentionally idempotent so the scene can be polished again
    /// after a SceneBuilder refresh.
    /// </summary>
    public static class SoulCrystalDefenseUIPolisher
    {
        private const string ScenePath = "Assets/Scenes/SoulCrystalDefenseGameplay.unity";

        private static readonly Color Ink = new Color(0.018f, 0.028f, 0.055f, 0.96f);
        private static readonly Color Panel = new Color(0.035f, 0.060f, 0.100f, 0.97f);
        private static readonly Color Steel = new Color(0.22f, 0.32f, 0.44f, 0.95f);
        private static readonly Color Soul = new Color(0.12f, 0.82f, 1.00f, 1.00f);
        private static readonly Color SoulSoft = new Color(0.20f, 0.65f, 0.86f, 0.65f);
        private static readonly Color Bone = new Color(0.90f, 0.91f, 0.88f, 1.00f);
        private static readonly Color Ember = new Color(1.00f, 0.63f, 0.18f, 1.00f);

        [MenuItem("SoulCrystalDefense/Polish Gameplay UI")]
        public static void ApplyAndSave()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Apply(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Soul Crystal UI] Gameplay HUD polish applied and scene saved.");
        }

        private static void Apply(Scene scene)
        {
            Transform canvas = Find(scene, "HUDCanvas");
            Transform safeArea = Find(scene, "SafeAreaRoot");
            Transform topHud = Find(scene, "TopHUD");
            Transform tray = Find(scene, "BottomTray");
            Transform resultModal = Find(scene, "VictoryDefeatModal");

            if (canvas == null || safeArea == null || topHud == null || tray == null)
            {
                throw new System.InvalidOperationException(
                    "SoulCrystalDefense gameplay UI hierarchy is incomplete. Rebuild the vertical slice scene first.");
            }

            PolishTopHud(topHud);
            PolishCountdown(Find(scene, "CountdownContainer"));
            PolishCardTray(tray);
            PolishResults(resultModal);
            canvas.GetComponent<Canvas>().sortingOrder = 50;
        }

        private static void PolishTopHud(Transform topHud)
        {
            RectTransform topRect = topHud.GetComponent<RectTransform>();
            topRect.sizeDelta = new Vector2(0f, 134f);

            Image topBackground = topHud.GetComponent<Image>();
            topBackground.color = new Color(0.012f, 0.020f, 0.042f, 0.90f);
            topBackground.raycastTarget = false;

            Image bottomGlow = CreatePanel(topHud, "BottomSoulGlow", Vector2.zero, Vector2.right,
                new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(0f, 2f), SoulSoft, false);
            bottomGlow.transform.SetAsLastSibling();

            Transform healthPod = CreateFixedPanel(topHud, "CrystalHealthPod", new Vector2(184f, 0f),
                new Vector2(328f, 94f), new Vector2(0f, 0.5f), Panel, Steel).transform;
            Transform wavePod = CreateFixedPanel(topHud, "WavePod", Vector2.zero,
                new Vector2(270f, 88f), new Vector2(0.5f, 0.5f), Panel, Steel).transform;
            Transform energyPod = CreateFixedPanel(topHud, "EnergyPod", new Vector2(-178f, 0f),
                new Vector2(184f, 88f), new Vector2(1f, 0.5f), Panel, Steel).transform;

            // Keep decorative pods behind the authored live text/button elements.
            healthPod.SetAsFirstSibling();
            wavePod.SetAsFirstSibling();
            energyPod.SetAsFirstSibling();

            Font font = topHud.GetComponentInChildren<Text>(true)?.font;
            Sprite crystalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/SoulCrystal_Sprite.png");

            Image healthIcon = CreateFixedPanel(healthPod, "CrystalIcon", new Vector2(-131f, 0f),
                new Vector2(58f, 72f), new Vector2(0.5f, 0.5f), Color.white, Color.clear);
            healthIcon.sprite = crystalSprite;
            healthIcon.preserveAspect = true;

            Text healthLabel = CreateText(healthPod, "CrystalLabel", "SOUL CRYSTAL", font, 17, TextAnchor.MiddleLeft,
                new Vector2(-15f, 22f), new Vector2(210f, 24f), Soul);
            healthLabel.fontStyle = FontStyle.Bold;

            Image healthTrack = CreateFixedPanel(healthPod, "HealthTrack", new Vector2(24f, -24f),
                new Vector2(218f, 15f), new Vector2(0.5f, 0.5f), new Color(0.10f, 0.02f, 0.035f, 1f), Steel);
            healthTrack.transform.SetAsFirstSibling();
            Image healthFill = CreatePanel(healthTrack.transform, "HealthFill", Vector2.zero, Vector2.one,
                new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.08f, 0.14f, 1f), false);
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = 0;
            healthFill.fillAmount = 1f;

            Text healthText = Find(topHud.gameObject.scene, "CrystalHealthText")?.GetComponent<Text>();
            if (healthText != null)
            {
                healthText.transform.SetParent(healthPod, false);
                SetRect(healthText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(76f, 22f), new Vector2(85f, 30f));
                healthText.alignment = TextAnchor.MiddleRight;
                healthText.fontSize = 27;
                healthText.fontStyle = FontStyle.Bold;
                healthText.color = Bone;
                AddTextEffects(healthText, new Color(0f, 0f, 0f, 0.9f));
            }

            GameplayHUD gameplayHud = topHud.GetComponent<GameplayHUD>();
            if (gameplayHud != null)
            {
                SerializedObject hudObject = new SerializedObject(gameplayHud);
                SerializedProperty barProperty = hudObject.FindProperty("crystalHealthBar");
                if (barProperty != null) barProperty.objectReferenceValue = healthFill;
                hudObject.ApplyModifiedPropertiesWithoutUndo();
            }

            Text waveLabel = CreateText(wavePod, "ArenaLabel", "FORGOTTEN GRAVEYARD", font, 14,
                TextAnchor.MiddleCenter, new Vector2(0f, -24f), new Vector2(240f, 22f), new Color(0.58f, 0.70f, 0.82f, 1f));
            waveLabel.fontStyle = FontStyle.Bold;

            Text waveText = Find(topHud.gameObject.scene, "WaveText")?.GetComponent<Text>();
            if (waveText != null)
            {
                waveText.transform.SetParent(wavePod, false);
                SetRect(waveText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(250f, 42f));
                waveText.fontSize = 34;
                waveText.fontStyle = FontStyle.Bold;
                waveText.color = Bone;
                AddTextEffects(waveText, Color.black);
            }

            Image energyIcon = CreateFixedPanel(energyPod, "EnergyIcon", new Vector2(-61f, 1f),
                new Vector2(38f, 54f), new Vector2(0.5f, 0.5f), Color.white, Color.clear);
            energyIcon.sprite = crystalSprite;
            energyIcon.preserveAspect = true;

            Text energyLabel = CreateText(energyPod, "EnergyLabel", "SOUL ENERGY", font, 13,
                TextAnchor.MiddleCenter, new Vector2(24f, -23f), new Vector2(125f, 20f), new Color(0.58f, 0.70f, 0.82f, 1f));
            energyLabel.fontStyle = FontStyle.Bold;

            Text energyText = Find(topHud.gameObject.scene, "EnergyText")?.GetComponent<Text>();
            if (energyText != null)
            {
                energyText.transform.SetParent(energyPod, false);
                SetRect(energyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(25f, 10f), new Vector2(120f, 44f));
                energyText.alignment = TextAnchor.MiddleCenter;
                energyText.fontSize = 32;
                energyText.fontStyle = FontStyle.Bold;
                energyText.color = Soul;
                AddTextEffects(energyText, Color.black);
            }

            Transform pauseTransform = Find(topHud.gameObject.scene, "PauseButton");
            if (pauseTransform != null)
            {
                RectTransform pauseRect = pauseTransform.GetComponent<RectTransform>();
                pauseRect.anchorMin = pauseRect.anchorMax = pauseRect.pivot = new Vector2(1f, 0.5f);
                pauseRect.anchoredPosition = new Vector2(-26f, 0f);
                pauseRect.sizeDelta = new Vector2(78f, 78f);

                Image pauseImage = pauseTransform.GetComponent<Image>();
                pauseImage.color = new Color(0.055f, 0.085f, 0.13f, 1f);
                ConfigureOutline(pauseTransform.gameObject, SoulSoft, new Vector2(2f, -2f));
                ConfigureButton(pauseTransform.GetComponent<Button>(), SoulSoft, new Color(0.04f, 0.18f, 0.28f, 1f));

                Text label = pauseTransform.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = "II";
                    label.fontSize = 30;
                    label.fontStyle = FontStyle.Bold;
                    label.color = Bone;
                    AddTextEffects(label, Color.black);
                }
            }
        }

        private static void PolishCountdown(Transform countdown)
        {
            if (countdown == null) return;

            RectTransform rect = countdown.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 20f);
            rect.sizeDelta = new Vector2(520f, 150f);

            Image background = GetOrAdd<Image>(countdown.gameObject);
            background.color = new Color(0.015f, 0.035f, 0.07f, 0.94f);
            background.raycastTarget = false;
            ConfigureOutline(countdown.gameObject, Soul, new Vector2(3f, -3f));

            Text countdownText = countdown.GetComponentInChildren<Text>(true);
            if (countdownText != null)
            {
                countdownText.fontSize = 72;
                countdownText.fontStyle = FontStyle.Bold;
                countdownText.color = Bone;
                AddTextEffects(countdownText, new Color(0f, 0.15f, 0.25f, 1f));
            }
        }

        private static void PolishCardTray(Transform tray)
        {
            RectTransform trayRect = tray.GetComponent<RectTransform>();
            trayRect.sizeDelta = new Vector2(0f, 250f);

            Image trayBackground = tray.GetComponent<Image>();
            trayBackground.color = new Color(0.010f, 0.020f, 0.040f, 0.96f);
            trayBackground.raycastTarget = false;

            Image rim = CreatePanel(tray, "TraySoulRim", new Vector2(0f, 1f), Vector2.one,
                new Vector2(0.5f, 1f), new Vector2(0f, 5f), new Vector2(0f, -2f), SoulSoft, false);
            rim.transform.SetAsLastSibling();

            Font font = tray.GetComponentInChildren<Text>(true)?.font;
            Text trayTitle = CreateText(tray, "TrayTitle", "SUMMON DEFENDERS", font, 15, TextAnchor.MiddleCenter,
                new Vector2(0f, -15f), new Vector2(320f, 24f), new Color(0.56f, 0.72f, 0.84f, 1f));
            trayTitle.rectTransform.anchorMin = trayTitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            trayTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            trayTitle.fontStyle = FontStyle.Bold;

            Transform layoutTransform = tray.Find("CardsLayout");
            if (layoutTransform != null)
            {
                RectTransform layoutRect = layoutTransform.GetComponent<RectTransform>();
                layoutRect.offsetMin = new Vector2(22f, 10f);
                layoutRect.offsetMax = new Vector2(-22f, -30f);

                HorizontalLayoutGroup layout = layoutTransform.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 14f;
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.childAlignment = TextAnchor.MiddleCenter;
            }

            Sprite soulFragment = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SHAMSA/Visuals/SoulFragment_Sprite.png");
            foreach (DefenderCardUI card in tray.GetComponentsInChildren<DefenderCardUI>(true))
            {
                PolishCard(card.transform, soulFragment);
            }
        }

        private static void PolishCard(Transform card, Sprite soulFragment)
        {
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(235f, 202f);

            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.040f, 0.065f, 0.105f, 0.99f);
            ConfigureOutline(card.gameObject, Steel, new Vector2(2f, -2f));
            ConfigureButton(card.GetComponent<Button>(), SoulSoft, new Color(0.055f, 0.20f, 0.29f, 1f));

            Image inner = CreatePanel(card, "InnerShade", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(-10f, -10f), Vector2.zero, new Color(0.015f, 0.026f, 0.050f, 0.58f), false);
            inner.transform.SetAsFirstSibling();

            Transform iconTransform = card.Find("Icon");
            if (iconTransform != null)
            {
                RectTransform iconRect = iconTransform.GetComponent<RectTransform>();
                SetRect(iconRect, new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(142f, 142f));
                Image icon = iconTransform.GetComponent<Image>();
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }

            Text nameText = card.Find("NameText")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.rectTransform.anchoredPosition = new Vector2(0f, -7f);
                nameText.rectTransform.sizeDelta = new Vector2(-16f, 30f);
                nameText.fontSize = 20;
                nameText.fontStyle = FontStyle.Bold;
                nameText.color = Bone;
                nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
                AddTextEffects(nameText, Color.black);

                if (card.name == "Card_Elephant") nameText.text = "WAR ELEPHANT";
                if (card.name == "Card_Train") nameText.text = "WAR TRAIN";
            }

            Transform badge = card.Find("CostBadge");
            if (badge != null)
            {
                RectTransform badgeRect = badge.GetComponent<RectTransform>();
                badgeRect.anchoredPosition = new Vector2(-8f, 8f);
                badgeRect.sizeDelta = new Vector2(86f, 42f);
                Image badgeImage = badge.GetComponent<Image>();
                badgeImage.color = new Color(0.025f, 0.20f, 0.32f, 0.98f);
                ConfigureOutline(badge.gameObject, SoulSoft, new Vector2(1f, -1f));

                Image icon = CreateFixedPanel(badge, "SoulCostIcon", new Vector2(-25f, 0f), new Vector2(28f, 30f),
                    new Vector2(0.5f, 0.5f), Color.white, Color.clear);
                icon.sprite = soulFragment;
                icon.preserveAspect = true;

                Text cost = badge.Find("CostText")?.GetComponent<Text>();
                if (cost != null)
                {
                    cost.rectTransform.anchorMin = new Vector2(0.38f, 0f);
                    cost.rectTransform.anchorMax = Vector2.one;
                    cost.rectTransform.offsetMin = Vector2.zero;
                    cost.rectTransform.offsetMax = Vector2.zero;
                    cost.fontSize = 20;
                    cost.fontStyle = FontStyle.Bold;
                    cost.color = Bone;
                    AddTextEffects(cost, Color.black);
                }
            }

            Transform selection = card.Find("SelectionBorder");
            if (selection != null)
            {
                Image selectionImage = selection.GetComponent<Image>();
                selectionImage.color = new Color(Soul.r, Soul.g, Soul.b, 0.13f);
                selectionImage.raycastTarget = false;
                ConfigureOutline(selection.gameObject, Soul, new Vector2(4f, -4f));
                selection.SetAsFirstSibling();
            }

            Transform unaffordable = card.Find("UnaffordableOverlay");
            if (unaffordable != null)
            {
                Image overlay = unaffordable.GetComponent<Image>();
                overlay.color = new Color(0.12f, 0.01f, 0.025f, 0.68f);
                overlay.raycastTarget = false;
            }

            Transform locked = card.Find("LockOverlay");
            if (locked != null)
            {
                Image overlay = locked.GetComponent<Image>();
                overlay.color = new Color(0.010f, 0.015f, 0.028f, 0.88f);
                overlay.raycastTarget = false;

                Text lockText = locked.GetComponentInChildren<Text>(true);
                if (lockText != null)
                {
                    lockText.fontSize = 20;
                    lockText.fontStyle = FontStyle.Bold;
                    lockText.color = new Color(0.70f, 0.75f, 0.79f, 1f);
                    AddTextEffects(lockText, Color.black);
                }

                Text lockGlyph = CreateText(locked, "LockGlyph", "◆", lockText != null ? lockText.font : null, 25,
                    TextAnchor.MiddleCenter, new Vector2(0f, 42f), new Vector2(50f, 40f), new Color(0.47f, 0.57f, 0.64f, 1f));
                lockGlyph.fontStyle = FontStyle.Bold;
            }
        }

        private static void PolishResults(Transform modal)
        {
            if (modal == null) return;

            Image overlay = modal.GetComponent<Image>();
            overlay.color = new Color(0.005f, 0.008f, 0.018f, 0.88f);

            Transform victory = modal.Find("VictoryContent");
            Transform defeat = modal.Find("DefeatContent");
            if (victory != null)
            {
                Image card = CreateFixedPanel(victory, "VictoryCard", Vector2.zero, new Vector2(820f, 720f),
                    new Vector2(0.5f, 0.5f), Ink, new Color(0.30f, 0.75f, 0.92f, 0.9f));
                card.transform.SetAsFirstSibling();
                PolishResultText(victory, "VictoryTitle", Soul);
                PolishResultText(victory, "RewardFragment", Soul);
                PolishResultText(victory, "TotalFragments", Bone);
                PolishResultButton(victory, "PlayAgainBtn", new Color(0.10f, 0.28f, 0.40f, 1f));
                PolishResultButton(victory, "NextBtn", new Color(0.08f, 0.42f, 0.56f, 1f));
            }

            if (defeat != null)
            {
                Image card = CreateFixedPanel(defeat, "DefeatCard", Vector2.zero, new Vector2(820f, 640f),
                    new Vector2(0.5f, 0.5f), Ink, new Color(0.68f, 0.16f, 0.18f, 0.95f));
                card.transform.SetAsFirstSibling();
                PolishResultText(defeat, "DefeatTitle", new Color(1f, 0.28f, 0.22f, 1f));
                PolishResultText(defeat, "DefeatHint", Bone);
                PolishResultButton(defeat, "RetryBtn", new Color(0.48f, 0.10f, 0.12f, 1f));
            }
        }

        private static void PolishResultText(Transform parent, string name, Color color)
        {
            Text text = parent.Find(name)?.GetComponent<Text>();
            if (text == null) return;
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            AddTextEffects(text, Color.black);
        }

        private static void PolishResultButton(Transform parent, string name, Color normal)
        {
            Transform child = parent.Find(name);
            if (child == null) return;
            Image image = child.GetComponent<Image>();
            image.color = normal;
            ConfigureOutline(child.gameObject, SoulSoft, new Vector2(2f, -2f));
            ConfigureButton(child.GetComponent<Button>(), SoulSoft, normal * 0.75f);
            Text label = child.GetComponentInChildren<Text>(true);
            if (label != null) AddTextEffects(label, Color.black);
        }

        private static Image CreateFixedPanel(Transform parent, string name, Vector2 position, Vector2 size,
            Vector2 anchor, Color color, Color outline)
        {
            Image image = CreatePanel(parent, name, anchor, anchor, new Vector2(0.5f, 0.5f), size, position, color, false);
            if (outline.a > 0f) ConfigureOutline(image.gameObject, outline, new Vector2(2f, -2f));
            return image;
        }

        private static Image CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 size, Vector2 position, Color color, bool raycastTarget)
        {
            GameObject obj = GetOrCreateChild(parent, name);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            if (anchorMin != anchorMax)
            {
                rect.offsetMin = new Vector2(position.x - size.x * 0.5f, position.y - size.y * 0.5f);
                rect.offsetMax = new Vector2(position.x + size.x * 0.5f, position.y + size.y * 0.5f);
            }

            Image image = GetOrAdd<Image>(obj);
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, Font font, int size,
            TextAnchor alignment, Vector2 position, Vector2 dimensions, Color color)
        {
            GameObject obj = GetOrCreateChild(parent, name);
            RectTransform rect = obj.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0.5f), position, dimensions);

            Text text = GetOrAdd<Text>(obj);
            text.text = value;
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void AddTextEffects(Text text, Color outlineColor)
        {
            Shadow shadow = GetOrAdd<Shadow>(text.gameObject);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
            shadow.effectDistance = new Vector2(2f, -2f);
            shadow.useGraphicAlpha = true;

            Outline outline = GetOrAdd<Outline>(text.gameObject);
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private static void ConfigureOutline(GameObject obj, Color color, Vector2 distance)
        {
            Outline outline = GetOrAdd<Outline>(obj);
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void ConfigureButton(Button button, Color highlight, Color pressed)
        {
            if (button == null) return;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = highlight;
            colors.pressedColor = pressed;
            colors.selectedColor = highlight;
            colors.disabledColor = new Color(0.28f, 0.30f, 0.34f, 0.62f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            return component != null ? component : obj.AddComponent<T>();
        }

        private static Transform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform current in transforms)
                {
                    if (current.name == name) return current;
                }
            }
            return null;
        }
    }
}
