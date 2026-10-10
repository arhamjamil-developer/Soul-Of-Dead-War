using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoulOfDeadWar.Animation
{
    /// <summary>
    /// Master Playable Demo Manager for Soul of Dead War.
    /// Manages high-definition top-down character animation, smooth locomotion,
    /// dynamic VFX overlay arcs (slash, smash, fire, smoke, sparks), and camera tracking.
    /// </summary>
    public class PlayableDemoManager : MonoBehaviour
    {
        [Header("Characters in Scene")]
        [SerializeField] private List<CharacterAnimationController> characters = new List<CharacterAnimationController>();
        [SerializeField] private int activeIndex = 0;

        [Header("Locomotion Tuning")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float runSpeed = 6.4f;
        [SerializeField] private float smoothTime = 0.06f;

        [Header("Camera Tracking")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraSmoothSpeed = 8.0f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -10f);

        [Header("VFX Outline Colors")]
        [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.85f, 0.2f, 1f); // Golden
        [SerializeField] private Color passiveOutlineColor = new Color(0.2f, 0.6f, 1f, 0.4f);  // Soft Cyan

        [Header("VFX Sprites")]
        [SerializeField] private Sprite[] attackVfxSprites; // Train, Barbarian, Elephant, Chef
        [SerializeField] private Sprite[] hurtVfxSprites;   // Train, Barbarian, Elephant, Chef

        private Vector2 _currentVelocity;
        private Vector2 _velocitySmoothing;
        private bool _outlineEnabled = true;

        public CharacterAnimationController ActiveCharacter =>
            (characters != null && characters.Count > activeIndex) ? characters[activeIndex] : null;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (characters == null || characters.Count == 0)
            {
                characters = new List<CharacterAnimationController>(FindObjectsByType<CharacterAnimationController>(FindObjectsSortMode.None));
                characters.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            }

            LoadVfxSprites();
            SelectCharacter(0);

            if (mainCamera != null && ActiveCharacter != null)
            {
                mainCamera.transform.position = ActiveCharacter.transform.position + cameraOffset;
            }
        }

        private void LoadVfxSprites()
        {
#if UNITY_EDITOR
            attackVfxSprites = new Sprite[4]
            {
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Train_Fire.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Barbarian_Slash.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Elephant_Smash.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Chef_Fire_Swing.png")
            };

            hurtVfxSprites = new Sprite[4]
            {
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Train_Smoke.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Barbarian_Impact.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Elephant_Dust.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Animations/MasterClips/VFX_Elements/Chef_Sparks.png")
            };
#endif
        }

        private void Update()
        {
            HandleCharacterSwitching();
            HandleMovementInput();
            HandleCombatTriggers();
            UpdateCameraFollow();
        }

        private void HandleCharacterSwitching()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && characters.Count > 0) SelectCharacter(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && characters.Count > 1) SelectCharacter(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && characters.Count > 2) SelectCharacter(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) && characters.Count > 3) SelectCharacter(3);

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                int nextIndex = (activeIndex + 1) % characters.Count;
                SelectCharacter(nextIndex);
            }
        }

        public void SelectCharacter(int index)
        {
            if (characters == null || characters.Count == 0) return;

            activeIndex = Mathf.Clamp(index, 0, characters.Count - 1);

            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] == null) continue;

                var vfx = characters[i].GetComponent<SpriteVFXController>();
                if (i == activeIndex)
                {
                    if (vfx != null && _outlineEnabled)
                    {
                        vfx.SetOutline(true, selectedOutlineColor);
                    }
                }
                else
                {
                    characters[i].UpdateMovement(Vector2.zero);
                    if (vfx != null)
                    {
                        vfx.SetOutline(false);
                    }
                }
            }
        }

        private void HandleMovementInput()
        {
            var character = ActiveCharacter;
            if (character == null || character.IsDead) return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector2 inputDir = new Vector2(horizontal, vertical).normalized;

            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float targetSpeed = (inputDir.magnitude > 0.05f) ? (isRunning ? runSpeed : walkSpeed) : 0f;

            Vector2 targetVelocity = inputDir * targetSpeed;
            _currentVelocity = Vector2.SmoothDamp(_currentVelocity, targetVelocity, ref _velocitySmoothing, smoothTime);

            character.transform.Translate((Vector3)_currentVelocity * Time.deltaTime, Space.World);

            float animSpeedParam = (targetSpeed > walkSpeed + 0.1f) ? 4.5f : ((targetSpeed > 0.1f) ? 1.5f : 0f);
            character.UpdateMovement(inputDir * animSpeedParam);
        }

        private void HandleCombatTriggers()
        {
            var character = ActiveCharacter;
            if (character == null) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                TriggerAttack();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                TriggerHurt();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleDead();
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                ToggleOutline();
            }
        }

        public void TriggerAttack()
        {
            var character = ActiveCharacter;
            if (character != null && !character.IsDead)
            {
                character.TriggerAttack();

                // Spawn Attack VFX
                int vfxIdx = GetVfxIndexForCharacter(character);
                if (attackVfxSprites != null && vfxIdx >= 0 && vfxIdx < attackVfxSprites.Length)
                {
                    Vector3 offset = new Vector3(0.6f, 0.2f, 0f);
                    var sr = character.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.flipX) offset.x = -offset.x;
                    StartCoroutine(SpawnVfxRoutine(attackVfxSprites[vfxIdx], character.transform.position + offset, 0.35f, 1.2f));
                }
            }
        }

        public void TriggerHurt()
        {
            var character = ActiveCharacter;
            if (character != null && !character.IsDead)
            {
                character.TakeDamage(10);

                // Spawn Hurt VFX
                int vfxIdx = GetVfxIndexForCharacter(character);
                if (hurtVfxSprites != null && vfxIdx >= 0 && vfxIdx < hurtVfxSprites.Length)
                {
                    StartCoroutine(SpawnVfxRoutine(hurtVfxSprites[vfxIdx], character.transform.position + new Vector3(0, 0.4f, 0), 0.3f, 1.3f));
                }
            }
        }

        private int GetVfxIndexForCharacter(CharacterAnimationController charCtrl)
        {
            string name = charCtrl.gameObject.name;
            if (name.Contains("Train")) return 0;
            if (name.Contains("Man") || name.Contains("Barbarian")) return 1;
            if (name.Contains("Elephant")) return 2;
            if (name.Contains("Chef")) return 3;
            return 0;
        }

        private IEnumerator SpawnVfxRoutine(Sprite sprite, Vector3 position, float duration, float scale)
        {
            if (sprite == null) yield break;

            var vfxObj = new GameObject("VFX_Instance");
            vfxObj.transform.position = position;
            vfxObj.transform.localScale = Vector3.one * scale;

            var sr = vfxObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 50;

            float elapsed = 0f;
            Color startColor = Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                vfxObj.transform.localScale = Vector3.one * Mathf.Lerp(scale, scale * 1.3f, t);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            Destroy(vfxObj);
        }

        public void ToggleDead()
        {
            var character = ActiveCharacter;
            if (character == null) return;

            if (!character.IsDead)
            {
                character.Die();
            }
            else
            {
                var animator = character.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool("IsDead", false);
                    animator.Play("Idle", 0, 0f);
                }

                var prop = typeof(CharacterAnimationController).GetProperty("IsDead");
                if (prop != null)
                {
                    prop.SetValue(character, false);
                }

                var vfx = character.GetComponent<SpriteVFXController>();
                if (vfx != null && _outlineEnabled)
                {
                    vfx.SetOutline(true, selectedOutlineColor);
                }
            }
        }

        public void ToggleOutline()
        {
            _outlineEnabled = !_outlineEnabled;
            var character = ActiveCharacter;
            if (character != null)
            {
                var vfx = character.GetComponent<SpriteVFXController>();
                if (vfx != null)
                {
                    vfx.SetOutline(_outlineEnabled, selectedOutlineColor);
                }
            }
        }

        private void UpdateCameraFollow()
        {
            if (mainCamera == null || ActiveCharacter == null) return;

            Vector3 targetPos = ActiveCharacter.transform.position + cameraOffset;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraSmoothSpeed);
        }

        private void OnGUI()
        {
            GUI.skin.box.fontSize = 12;
            GUI.skin.button.fontSize = 12;

            GUILayout.BeginArea(new Rect(15, 15, 370, 430), GUI.skin.box);
            GUILayout.Label("<size=14><b>SOUL OF DEAD WAR — MASTER DEMO</b></size>");
            GUILayout.Space(4);

            var charName = (ActiveCharacter != null) ? ActiveCharacter.gameObject.name : "None";
            var status = (ActiveCharacter != null && ActiveCharacter.IsDead) ? "<color=red>DEAD</color>" : "<color=green>ALIVE</color>";
            GUILayout.Label($"<b>Controlled:</b> {charName} ({status})");
            GUILayout.Label($"<b>Velocity:</b> {_currentVelocity.magnitude:F1} m/s | <b>Outline:</b> {(_outlineEnabled ? "ON" : "OFF")}");
            GUILayout.Space(6);

            GUILayout.Label("<b>1. Switch Character:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("[1] Train", GUILayout.Height(28))) SelectCharacter(0);
            if (GUILayout.Button("[2] Barbarian", GUILayout.Height(28))) SelectCharacter(1);
            if (GUILayout.Button("[3] Elephant", GUILayout.Height(28))) SelectCharacter(2);
            if (GUILayout.Button("[4] Chef", GUILayout.Height(28))) SelectCharacter(3);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>2. Combat & VFX Actions:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Attack + VFX [Space]", GUILayout.Height(30))) TriggerAttack();
            if (GUILayout.Button("Hurt + VFX [H]", GUILayout.Height(30))) TriggerHurt();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button((ActiveCharacter != null && ActiveCharacter.IsDead) ? "Revive [K]" : "Die [K]", GUILayout.Height(30))) ToggleDead();
            if (GUILayout.Button("Outline [O]", GUILayout.Height(30))) ToggleOutline();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("<b>Controls Guide:</b>");
            GUILayout.Label("• <b>[W,A,S,D / Arrows]</b>: 8-Way Top-Down Movement");
            GUILayout.Label("• <b>[Left Shift]</b>: Hold to Sprint (Run)");
            GUILayout.Label("• <b>[Space / Click]</b>: Attack + Slash/Smash/Fire VFX");
            GUILayout.Label("• <b>[H]</b>: Hurt VFX (Hit Flash + Impact Sparks)");
            GUILayout.Label("• <b>[K]</b>: Fatal Collapse / Revive");
            GUILayout.Label("• <b>[O]</b>: Toggle Dynamic Silhouette Outline");
            GUILayout.Label("• <b>[Tab / 1-4]</b>: Switch Controlled Unit");

            GUILayout.EndArea();
        }
    }
}
