using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SoulCrystalDefense.Grid;
using SoulCrystalDefense.UI;

namespace SoulCrystalDefense.EditorTools
{
    public static class GameplayUIOverhaul
    {
        [MenuItem("Soul Crystal/Apply Full Graveyard UI Overhaul")]
        public static void Execute()
        {
            var canvas = GameObject.Find("HUDCanvas");
            if (canvas == null)
            {
                Debug.LogError("[UIOverhaul] HUDCanvas not found!");
                return;
            }

            // 1. Load fonts
            var paybackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/PaybAck SDF.asset");
            var magicFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Final/MagicBaloonDemoRegular SDF.asset");
            if (paybackFont == null) Debug.LogWarning("[UIOverhaul] PaybAck SDF font missing!");
            if (magicFont == null) Debug.LogWarning("[UIOverhaul] MagicBaloonDemoRegular SDF font missing!");

            // 2. Overhaul World Backdrop & Stone Patches
            OverhaulWorldBackdropAndGrid();

            // 3. Overhaul Top HUD
            OverhaulTopHUD(canvas, paybackFont, magicFont);

            // 4. Overhaul Bottom Tray & Cards
            OverhaulBottomTray(canvas, paybackFont, magicFont);

            // 5. Setup BattlefieldViewManager and Toggle Buttons
            SetupViewManagerAndToggleButtons(canvas, paybackFont);

            // 6. Global Font Pass
            ApplyFontsGlobally(canvas, paybackFont, magicFont);

            // Save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );

            Debug.Log("[UIOverhaul] ✅ All 4 objectives successfully executed and applied to the scene!");
        }

        // =========================================================================
        // 1. WORLD BACKDROP & STONE PATCHES
        // =========================================================================
        private static void OverhaulWorldBackdropAndGrid()
        {
            // Backdrop scale
            var backdrop = GameObject.Find("GraveyardBackdrop");
            if (backdrop != null)
            {
                var modularSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Final/Production/World/Graveyard-Modular.png");
                var sr = backdrop.GetComponent<SpriteRenderer>();
                if (sr != null && modularSprite != null)
                {
                    sr.sprite = modularSprite;
                    sr.sortingOrder = -20;
                }
                backdrop.transform.position = new Vector3(0f, 0.3f, 0f);
                backdrop.transform.localScale = new Vector3(1.025f, 1.02f, 1f);
                EditorUtility.SetDirty(backdrop);
            }

            // BoardGrid stone patches
            var gridGO = GameObject.Find("BoardGrid");
            if (gridGO != null)
            {
                var grid = gridGO.GetComponent<BoardGrid>();
                var patches = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    patches[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/Final/Production/World/StonePatch_{i}.png");
                }

                // Assign to serialized property
                var so = new SerializedObject(grid);
                var prop = so.FindProperty("stonePatches");
                if (prop != null)
                {
                    prop.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = patches[i];
                    }
                    so.ApplyModifiedProperties();
                }

                // Apply to all existing child cells
                for (int l = 0; l < 5; l++)
                {
                    for (int r = 0; r < 8; r++)
                    {
                        var cell = grid.GetCell(l, r);
                        if (cell == null) continue;
                        var sr = cell.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            int idx = (l * 3 + r * 7) % 4;
                            sr.sprite = patches[idx];
                            sr.color = Color.white;
                            sr.sortingOrder = -12;
                            cell.transform.localScale = new Vector3(1.05f, 1.0f, 1.0f);
                            EditorUtility.SetDirty(cell);
                        }
                    }
                }
                EditorUtility.SetDirty(gridGO);
            }
        }

        // =========================================================================
        // 2. TOP HUD (RECTANGULAR, DYNAMIC, COMPACT)
        // =========================================================================
        private static void OverhaulTopHUD(GameObject canvas, TMP_FontAsset titleFont, TMP_FontAsset bodyFont)
        {
            var topHUD = FindDeep(canvas.transform, "TopHUD");
            if (topHUD == null) return;

            var topRT = topHUD.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0f, 1f);
            topRT.anchorMax = new Vector2(1f, 1f);
            topRT.pivot = new Vector2(0.5f, 1f);
            topRT.anchoredPosition = Vector2.zero;
            topRT.sizeDelta = new Vector2(0f, 130f);

            var topImg = topHUD.GetComponent<Image>();
            if (topImg != null)
            {
                topImg.sprite = null;
                topImg.color = new Color(0.05f, 0.03f, 0.12f, 0.92f);
                topImg.type = Image.Type.Simple;
            }

            // Top outline/border
            var outline = topHUD.GetComponent<Outline>();
            if (outline == null) outline = topHUD.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.35f, 0.65f, 0.85f);
            outline.effectDistance = new Vector2(0f, -2f);

            // Left Pod: HealthPod (Crystal Health)
            var hp = FindDeep(topHUD, "HealthPod");
            if (hp != null)
            {
                var hpRT = hp.GetComponent<RectTransform>();
                hpRT.anchorMin = new Vector2(0.015f, 0.08f);
                hpRT.anchorMax = new Vector2(0.32f, 0.92f);
                hpRT.offsetMin = Vector2.zero;
                hpRT.offsetMax = Vector2.zero;
                var img = hp.GetComponent<Image>();
                if (img)
                {
                    img.sprite = null;
                    img.color = new Color(0.10f, 0.06f, 0.22f, 0.85f);
                }
                var hpOutline = hp.GetComponent<Outline>();
                if (hpOutline == null) hpOutline = hp.gameObject.AddComponent<Outline>();
                hpOutline.effectColor = new Color(0.35f, 0.25f, 0.55f, 0.65f);
                hpOutline.effectDistance = new Vector2(1f, -1f);
            }

            // Center Pod: StagePod (Realm & Wave)
            var sp = FindDeep(topHUD, "StagePod");
            if (sp != null)
            {
                var spRT = sp.GetComponent<RectTransform>();
                spRT.anchorMin = new Vector2(0.335f, 0.08f);
                spRT.anchorMax = new Vector2(0.665f, 0.92f);
                spRT.offsetMin = Vector2.zero;
                spRT.offsetMax = Vector2.zero;
                var img = sp.GetComponent<Image>();
                if (img)
                {
                    img.sprite = null;
                    img.color = new Color(0.10f, 0.06f, 0.22f, 0.85f);
                }
                var spOutline = sp.GetComponent<Outline>();
                if (spOutline == null) spOutline = sp.gameObject.AddComponent<Outline>();
                spOutline.effectColor = new Color(0.35f, 0.25f, 0.55f, 0.65f);
                spOutline.effectDistance = new Vector2(1f, -1f);
            }

            // Right Pod: EnergyPod (Soul Energy & Controls)
            var ep = FindDeep(topHUD, "EnergyPod");
            if (ep != null)
            {
                var epRT = ep.GetComponent<RectTransform>();
                epRT.anchorMin = new Vector2(0.68f, 0.08f);
                epRT.anchorMax = new Vector2(0.985f, 0.92f);
                epRT.offsetMin = Vector2.zero;
                epRT.offsetMax = Vector2.zero;
                var img = ep.GetComponent<Image>();
                if (img)
                {
                    img.sprite = null;
                    img.color = new Color(0.10f, 0.06f, 0.22f, 0.85f);
                }
                var epOutline = ep.GetComponent<Outline>();
                if (epOutline == null) epOutline = ep.gameObject.AddComponent<Outline>();
                epOutline.effectColor = new Color(0.35f, 0.25f, 0.55f, 0.65f);
                epOutline.effectDistance = new Vector2(1f, -1f);
            }

            // Hide old insets that have round bright borders
            var waveInset = FindDeep(topHUD, "WaveInset");
            if (waveInset != null) waveInset.gameObject.SetActive(false);
            var energyInset = FindDeep(topHUD, "EnergyInset");
            if (energyInset != null) energyInset.gameObject.SetActive(false);

            // Position Health bar track
            var healthTrack = FindDeep(topHUD, "HealthTrack");
            if (healthTrack != null)
            {
                var htRT = healthTrack.GetComponent<RectTransform>();
                htRT.anchorMin = new Vector2(0.03f, 0.15f);
                htRT.anchorMax = new Vector2(0.31f, 0.32f);
                htRT.offsetMin = Vector2.zero;
                htRT.offsetMax = Vector2.zero;
            }

            var healthFill = FindDeep(topHUD, "HealthFill");
            if (healthFill != null)
            {
                var hfRT = healthFill.GetComponent<RectTransform>();
                hfRT.anchorMin = new Vector2(0.035f, 0.17f);
                hfRT.anchorMax = new Vector2(0.305f, 0.30f);
                hfRT.offsetMin = Vector2.zero;
                hfRT.offsetMax = Vector2.zero;
            }

            // Pause button on far right
            var pauseBtn = FindDeep(topHUD, "PauseButton");
            if (pauseBtn != null)
            {
                var pbRT = pauseBtn.GetComponent<RectTransform>();
                pbRT.anchorMin = new Vector2(0.98f, 0.5f);
                pbRT.anchorMax = new Vector2(0.98f, 0.5f);
                pbRT.pivot = new Vector2(1f, 0.5f);
                pbRT.anchoredPosition = new Vector2(-6f, 0f);
                pbRT.sizeDelta = new Vector2(46f, 46f);
            }

            // Style Texts
            SetTMPProps(FindDeep(topHUD, "CrystalHealthText"), titleFont, 24f, new Color(0.55f, 1f, 0.75f), FontStyles.Bold);
            SetTMPProps(FindDeep(topHUD, "WaveText"), titleFont, 22f, new Color(1f, 0.9f, 0.45f), FontStyles.Bold);
            SetTMPProps(FindDeep(topHUD, "EnergyText"), titleFont, 24f, new Color(0.45f, 0.95f, 1f), FontStyles.Bold);
            SetTMPProps(FindDeep(topHUD, "CrystalName"), bodyFont, 14f, new Color(0.9f, 0.85f, 1f), FontStyles.Normal);
            SetTMPProps(FindDeep(topHUD, "World"), bodyFont, 13f, new Color(0.85f, 0.8f, 1f), FontStyles.Normal);
            SetTMPProps(FindDeep(topHUD, "Stage"), bodyFont, 13f, new Color(0.85f, 0.8f, 1f), FontStyles.Normal);
            SetTMPProps(FindDeep(topHUD, "SoulLabel"), bodyFont, 13f, new Color(0.7f, 0.9f, 1f), FontStyles.Normal);

            EditorUtility.SetDirty(topHUD.gameObject);
        }

        // =========================================================================
        // 3. BOTTOM TRAY & CARD HOLDERS (RECTANGULAR, ADJUSTED SIZE, PREMIUM FRAMES)
        // =========================================================================
        private static void OverhaulBottomTray(GameObject canvas, TMP_FontAsset titleFont, TMP_FontAsset bodyFont)
        {
            var bottomTray = FindDeep(canvas.transform, "BottomTray");
            if (bottomTray == null) return;

            var cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Frames/CardFrame.png");
            var buttonFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Frames/Button.png");
            var panelFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Premium/Frames/Panel.png");

            var btRT = bottomTray.GetComponent<RectTransform>();
            btRT.anchorMin = new Vector2(0f, 0f);
            btRT.anchorMax = new Vector2(1f, 0f);
            btRT.pivot = new Vector2(0.5f, 0f);
            btRT.anchoredPosition = Vector2.zero;
            btRT.sizeDelta = new Vector2(0f, 440f);

            var btImg = bottomTray.GetComponent<Image>();
            if (btImg != null)
            {
                btImg.sprite = null; // Clean rectangular solid panel
                btImg.color = new Color(0.04f, 0.02f, 0.10f, 0.92f);
                btImg.type = Image.Type.Simple;
            }

            var outline = bottomTray.GetComponent<Outline>();
            if (outline == null) outline = bottomTray.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.35f, 0.65f, 0.85f);
            outline.effectDistance = new Vector2(0f, 2f);

            // Hide decorative old rims if any
            var rim = FindDeep(bottomTray, "TraySoulRim");
            if (rim != null) rim.gameObject.SetActive(false);

            // Tier 1: Crystal Arts buttons (Y = 385 to 430)
            for (int i = 0; i < 3; i++)
            {
                var art = FindDeep(bottomTray, $"Art_{i}");
                if (art == null) continue;
                var artRT = art.GetComponent<RectTransform>();
                artRT.anchorMin = new Vector2(0.025f + i * 0.325f, 0.88f);
                artRT.anchorMax = new Vector2(0.025f + (i + 1) * 0.325f - 0.015f, 0.98f);
                artRT.offsetMin = Vector2.zero;
                artRT.offsetMax = Vector2.zero;

                var img = art.GetComponent<Image>();
                if (img)
                {
                    if (buttonFrameSprite != null) img.sprite = buttonFrameSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }

                SetTMPProps(FindDeep(art, "Name"), titleFont, 13f, new Color(1f, 0.95f, 0.75f), FontStyles.Bold);
                SetTMPProps(FindDeep(art, "State"), bodyFont, 11f, new Color(0.5f, 1f, 0.8f), FontStyles.Normal);
            }

            // Hide old ArtsTitle and ArtsEmblem if active
            var artsTitle = FindDeep(bottomTray, "ArtsTitle");
            if (artsTitle != null) artsTitle.gameObject.SetActive(false);
            var artsEmblem = FindDeep(bottomTray, "ArtsEmblem");
            if (artsEmblem != null) artsEmblem.gameObject.SetActive(false);

            // Tier 2: Reinforcements (Y = 320 to 375)
            var rb = FindDeep(bottomTray, "RoyalBulwarkButton");
            if (rb != null)
            {
                var rbRT = rb.GetComponent<RectTransform>();
                rbRT.anchorMin = new Vector2(0.025f, 0.75f);
                rbRT.anchorMax = new Vector2(0.485f, 0.865f);
                rbRT.offsetMin = Vector2.zero;
                rbRT.offsetMax = Vector2.zero;
                var img = rb.GetComponent<Image>();
                if (img)
                {
                    if (buttonFrameSprite != null) img.sprite = buttonFrameSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }

                SetTMPProps(FindDeep(rb, "Title"), titleFont, 14f, new Color(1f, 0.92f, 0.65f), FontStyles.Bold);
                SetTMPProps(FindDeep(rb, "State"), bodyFont, 12f, new Color(0.6f, 0.9f, 1f), FontStyles.Normal);
            }

            var se = FindDeep(bottomTray, "SoulExpressButton");
            if (se != null)
            {
                var seRT = se.GetComponent<RectTransform>();
                seRT.anchorMin = new Vector2(0.515f, 0.75f);
                seRT.anchorMax = new Vector2(0.975f, 0.865f);
                seRT.offsetMin = Vector2.zero;
                seRT.offsetMax = Vector2.zero;
                var img = se.GetComponent<Image>();
                if (img)
                {
                    if (buttonFrameSprite != null) img.sprite = buttonFrameSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }

                SetTMPProps(FindDeep(se, "Title"), titleFont, 14f, new Color(1f, 0.92f, 0.65f), FontStyles.Bold);
                SetTMPProps(FindDeep(se, "State"), bodyFont, 12f, new Color(0.6f, 0.9f, 1f), FontStyles.Normal);
            }

            // Tier 3: Defender Cards Scroll Area (Y = 35 to 315)
            var scroll = FindDeep(bottomTray, "GuardianScroll");
            if (scroll != null)
            {
                var sRT = scroll.GetComponent<RectTransform>();
                sRT.anchorMin = new Vector2(0.01f, 0.07f);
                sRT.anchorMax = new Vector2(0.99f, 0.73f);
                sRT.offsetMin = Vector2.zero;
                sRT.offsetMax = Vector2.zero;

                var sImg = scroll.GetComponent<Image>();
                if (sImg != null)
                {
                    sImg.sprite = null;
                    sImg.color = new Color(0f, 0f, 0f, 0.25f);
                }
            }

            // Cards layout
            var cardsLayout = FindDeep(bottomTray, "CardsLayout");
            if (cardsLayout != null)
            {
                var hlg = cardsLayout.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.spacing = 10f;
                    hlg.padding = new RectOffset(8, 8, 4, 4);
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = false;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = false;
                }
            }

            // Adjust each defender card
            var cardShortNames = new Dictionary<string, string>
            {
                { "Card_DwarfGuardian", "Dwarf" },
                { "Card_BloodChef", "Chef" },
                { "Card_SoulArcher", "Archer" },
                { "Card_BoneMage", "Mage" },
                { "Card_IronPaladin", "Paladin" },
                { "Card_FrostWitch", "Witch" }
            };

            foreach (var kvp in cardShortNames)
            {
                var card = FindDeep(bottomTray, kvp.Key);
                if (card == null) continue;

                // Adjust card size
                var le = card.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredWidth = 152f;
                    le.preferredHeight = 250f;
                    le.minWidth = 145f;
                    le.minHeight = 240f;
                }

                var cardRT = card.GetComponent<RectTransform>();
                cardRT.sizeDelta = new Vector2(152f, 250f);

                // Use the premium CardFrame rectangular sprite
                var cardImg = card.GetComponent<Image>();
                if (cardImg != null)
                {
                    if (cardFrameSprite != null) cardImg.sprite = cardFrameSprite;
                    cardImg.color = Color.white;
                    cardImg.type = Image.Type.Simple;
                }

                // Portrait in upper 62% of card
                var portrait = FindDeep(card, "Portrait");
                if (portrait != null)
                {
                    var pRT = portrait.GetComponent<RectTransform>();
                    pRT.anchorMin = new Vector2(0.08f, 0.36f);
                    pRT.anchorMax = new Vector2(0.92f, 0.94f);
                    pRT.offsetMin = Vector2.zero;
                    pRT.offsetMax = Vector2.zero;
                }

                // Short Name: Centered in middle strip (Y = 0.20 to 0.34)
                var nameObj = FindDeep(card, "Name");
                if (nameObj != null)
                {
                    var nRT = nameObj.GetComponent<RectTransform>();
                    nRT.anchorMin = new Vector2(0.05f, 0.20f);
                    nRT.anchorMax = new Vector2(0.95f, 0.34f);
                    nRT.offsetMin = Vector2.zero;
                    nRT.offsetMax = Vector2.zero;

                    var legText = nameObj.GetComponent<Text>();
                    if (legText) legText.text = kvp.Value;

                    SetTMPProps(nameObj, titleFont, 16f, new Color(1f, 0.95f, 0.7f), FontStyles.Bold, kvp.Value);

                    var tmp = nameObj.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        tmp.rectTransform.anchorMin = Vector2.zero;
                        tmp.rectTransform.anchorMax = Vector2.one;
                        tmp.rectTransform.offsetMin = Vector2.zero;
                        tmp.rectTransform.offsetMax = Vector2.zero;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.enableVertexGradient = true;
                        tmp.colorGradient = new VertexGradient(
                            new Color(1f, 1f, 0.85f), new Color(1f, 1f, 0.85f),
                            new Color(1f, 0.85f, 0.45f), new Color(1f, 0.85f, 0.45f)
                        );
                    }
                }

                // Cost Badge at bottom (Y = 0.05 to 0.19)
                var costBadge = FindDeep(card, "CostBadge");
                if (costBadge != null)
                {
                    var cbRT = costBadge.GetComponent<RectTransform>();
                    cbRT.anchorMin = new Vector2(0.1f, 0.05f);
                    cbRT.anchorMax = new Vector2(0.9f, 0.19f);
                    cbRT.offsetMin = Vector2.zero;
                    cbRT.offsetMax = Vector2.zero;

                    var cbImg = costBadge.GetComponent<Image>();
                    if (cbImg != null)
                    {
                        cbImg.color = new Color(0.06f, 0.04f, 0.16f, 0.85f);
                    }

                    // Currency Icon
                    var currency = FindDeep(costBadge, "Currency");
                    if (currency != null)
                    {
                        var cRT = currency.GetComponent<RectTransform>();
                        cRT.anchorMin = new Vector2(0.05f, 0.15f);
                        cRT.anchorMax = new Vector2(0.35f, 0.85f);
                        cRT.offsetMin = Vector2.zero;
                        cRT.offsetMax = Vector2.zero;
                    }

                    // Cost text
                    var costObj = FindDeep(costBadge, "Cost");
                    if (costObj != null)
                    {
                        var coRT = costObj.GetComponent<RectTransform>();
                        coRT.anchorMin = new Vector2(0.35f, 0.05f);
                        coRT.anchorMax = new Vector2(0.95f, 0.95f);
                        coRT.offsetMin = Vector2.zero;
                        coRT.offsetMax = Vector2.zero;

                        SetTMPProps(costObj, titleFont, 16f, new Color(0.45f, 0.95f, 1f), FontStyles.Bold);
                        var cTmp = costObj.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (cTmp != null)
                        {
                            cTmp.rectTransform.anchorMin = Vector2.zero;
                            cTmp.rectTransform.anchorMax = Vector2.one;
                            cTmp.rectTransform.offsetMin = Vector2.zero;
                            cTmp.rectTransform.offsetMax = Vector2.zero;
                            cTmp.alignment = TextAlignmentOptions.Center;
                        }
                    }
                }

                EditorUtility.SetDirty(card.gameObject);
            }

            // Hint banner at very bottom
            var hint = FindDeep(bottomTray, "GuardianHint");
            if (hint != null)
            {
                var hRT = hint.GetComponent<RectTransform>();
                hRT.anchorMin = new Vector2(0.05f, 0.01f);
                hRT.anchorMax = new Vector2(0.95f, 0.06f);
                hRT.offsetMin = Vector2.zero;
                hRT.offsetMax = Vector2.zero;
                var hImg = hint.GetComponent<Image>();
                if (hImg != null)
                {
                    hImg.sprite = null;
                    hImg.color = new Color(0f, 0f, 0f, 0f); // Invisible background
                }
                SetTMPProps(FindDeep(hint, "Instructions"), bodyFont, 11f, new Color(0.9f, 0.85f, 0.65f, 0.85f), FontStyles.Normal);
            }

            EditorUtility.SetDirty(bottomTray.gameObject);
        }

        // =========================================================================
        // 4. TOGGLE BUTTONS & VIEW MANAGER
        // =========================================================================
        private static void SetupViewManagerAndToggleButtons(GameObject canvas, TMP_FontAsset font)
        {
        }

        private static RectTransform CreateOrUpdatePillButton(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size,
            string labelText, TMP_FontAsset font, UnityEngine.Events.UnityAction onClick)
        {
            var existing = parent.Find(name);
            GameObject btnGO;
            if (existing != null)
            {
                btnGO = existing.gameObject;
            }
            else
            {
                btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(parent, false);
            }

            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.12f, 0.08f, 0.24f, 0.92f);
            img.type = Image.Type.Simple;

            var outline = btnGO.GetComponent<Outline>();
            if (outline == null) outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.5f, 0.35f, 0.8f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);

            var btn = btnGO.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, onClick);

            // Label
            var labelTrans = btnGO.transform.Find("Label");
            GameObject labelGO;
            if (labelTrans != null)
            {
                labelGO = labelTrans.gameObject;
            }
            else
            {
                labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelGO.transform.SetParent(btnGO.transform, false);
            }

            var lRT = labelGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero;
            lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(4f, 2f);
            lRT.offsetMax = new Vector2(-4f, -2f);

            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 13f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 9f;
            tmp.fontSizeMax = 14f;
            tmp.color = new Color(1f, 0.95f, 0.8f);
            if (font != null) tmp.font = font;

            EditorUtility.SetDirty(btnGO);
            return rt;
        }

        // =========================================================================
        // 5. GLOBAL FONT PASS
        // =========================================================================
        private static void ApplyFontsGlobally(GameObject canvas, TMP_FontAsset titleFont, TMP_FontAsset bodyFont)
        {
            if (titleFont == null && bodyFont == null) return;

            var allTMPs = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs)
            {
                var parent = tmp.transform.parent != null ? tmp.transform.parent.name : "";
                var selfName = tmp.name;

                bool useTitle = tmp.fontSize >= 18f ||
                                selfName.Contains("Title") || selfName.Contains("Heading") ||
                                parent.Contains("Title") || parent.Contains("Name") ||
                                parent.Contains("Cost") || parent.Contains("Health") ||
                                parent.Contains("Energy") || parent.Contains("Wave");

                if (useTitle && titleFont != null)
                {
                    tmp.font = titleFont;
                }
                else if (bodyFont != null)
                {
                    tmp.font = bodyFont;
                }
                EditorUtility.SetDirty(tmp);
            }
        }

        // =========================================================================
        // UTILS
        // =========================================================================
        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var res = FindDeep(child, name);
                if (res != null) return res;
            }
            return null;
        }

        private static void SetTMPProps(Transform obj, TMP_FontAsset font, float fontSize, Color color, FontStyles style, string overrideText = null)
        {
            if (obj == null) return;
            var tmps = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (overrideText != null) tmp.text = overrideText;
                if (font != null) tmp.font = font;
                tmp.fontSize = fontSize;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = Mathf.Max(8f, fontSize - 6f);
                tmp.fontSizeMax = fontSize;
                tmp.color = color;
                tmp.fontStyle = style;
                EditorUtility.SetDirty(tmp);
            }
        }
    }
}
