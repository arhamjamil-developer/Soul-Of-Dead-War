using System;
using UnityEngine;

namespace SoulCrystalDefense.Units
{
    public class UnitAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        
        [Header("Animation Sets")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private float idleFps = 8f;

        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private float walkFps = 11f;

        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private float attackFps = 14f;

        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float deathFps = 10f;

        public enum AnimState
        {
            Idle,
            Walk,
            Attack,
            Death
        }

        private AnimState currentState = AnimState.Idle;
        private Sprite[] currentClip;
        private float currentFps = 10f;
        private bool currentLoop = true;
        private bool holdFinalFrame = false;

        private float frameTimer = 0f;
        private int currentFrameIndex = 0;
        private Action onAnimComplete;

        public AnimState CurrentState => currentState;
        public SpriteRenderer TargetRenderer => targetRenderer;
        public bool HasIdleFrames => idleFrames != null && idleFrames.Length > 0;
        public bool HasWalkFrames => walkFrames != null && walkFrames.Length > 0;
        public bool HasAttackFrames => attackFrames != null && attackFrames.Length > 0;
        public bool HasDeathFrames => deathFrames != null && deathFrames.Length > 0;

        private void Awake()
        {
            EnsureRenderer();
        }

        private void OnEnable()
        {
            EnsureRenderer();
        }

        public void EnsureRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
                if (targetRenderer == null)
                {
                    targetRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        public void SetFrames(Sprite[] walk, Sprite[] attack, Sprite[] death)
        {
            walkFrames = walk;
            attackFrames = attack;
            deathFrames = death;
        }

        public void SetFrames(Sprite[] idle, Sprite[] walk, Sprite[] attack, Sprite[] death)
        {
            idleFrames = idle;
            walkFrames = walk;
            attackFrames = attack;
            deathFrames = death;
        }

        public void SetFps(float walk, float attack, float death, float idle = 8f)
        {
            walkFps = walk;
            attackFps = attack;
            deathFps = death;
            idleFps = idle;
        }

        public void PlayIdle()
        {
            if (currentState == AnimState.Idle && currentClip == idleFrames && currentClip != null) return;
            currentState = AnimState.Idle;

            if (idleFrames != null && idleFrames.Length > 0)
            {
                PlayClip(idleFrames, idleFps, true, false, null);
            }
            else if (walkFrames != null && walkFrames.Length > 0)
            {
                // Fallback for units without dedicated idle strip (e.g. enemies standing still in combat)
                EnsureRenderer();
                currentClip = null;
                currentLoop = false;
                holdFinalFrame = false;
                onAnimComplete = null;
                if (targetRenderer != null)
                {
                    targetRenderer.sprite = walkFrames[0];
                }
            }
        }

        public void PlayWalk()
        {
            if (currentState == AnimState.Walk && currentClip == walkFrames && currentClip != null) return;
            if (walkFrames != null && walkFrames.Length > 0)
            {
                currentState = AnimState.Walk;
                PlayClip(walkFrames, walkFps, true, false, null);
            }
            else
            {
                PlayIdle();
            }
        }

        public void PlayAttack(Action onComplete = null)
        {
            if (attackFrames == null || attackFrames.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            currentState = AnimState.Attack;
            PlayClip(attackFrames, attackFps, false, false, () =>
            {
                onComplete?.Invoke();
                if (currentState == AnimState.Attack)
                {
                    PlayIdle();
                }
            });
        }

        public void PlayDeath(Action onComplete = null)
        {
            currentState = AnimState.Death;
            if (deathFrames != null && deathFrames.Length > 0)
            {
                PlayClip(deathFrames, deathFps, false, true, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void PlayClip(Sprite[] clip, float fps, bool loop, bool holdLast, Action onComplete)
        {
            EnsureRenderer();
            currentClip = clip;
            currentFps = Mathf.Max(1f, fps);
            currentLoop = loop;
            holdFinalFrame = holdLast;
            onAnimComplete = onComplete;
            currentFrameIndex = 0;
            frameTimer = 0f;

            if (currentClip != null && currentClip.Length > 0 && targetRenderer != null)
            {
                targetRenderer.sprite = currentClip[0];
            }
        }

        private void Update()
        {
            if (currentClip == null || currentClip.Length == 0) return;
            EnsureRenderer();
            if (targetRenderer == null) return;

            // When holding the final frame after non-looping playback, remain halted on the last frame
            if (!currentLoop && holdFinalFrame && currentFrameIndex >= currentClip.Length - 1)
            {
                var finished = onAnimComplete;
                onAnimComplete = null;
                finished?.Invoke();
                return;
            }

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / currentFps;

            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrameIndex++;

                if (currentFrameIndex >= currentClip.Length)
                {
                    if (currentLoop)
                    {
                        currentFrameIndex = 0;
                    }
                    else
                    {
                        if (holdFinalFrame)
                        {
                            currentFrameIndex = currentClip.Length - 1;
                            targetRenderer.sprite = currentClip[currentFrameIndex];
                            var cb = onAnimComplete;
                            onAnimComplete = null;
                            cb?.Invoke();
                            return;
                        }
                        else
                        {
                            currentFrameIndex = 0;
                            var cb = onAnimComplete;
                            onAnimComplete = null;
                            cb?.Invoke();
                            return;
                        }
                    }
                }

                targetRenderer.sprite = currentClip[currentFrameIndex];
            }
        }
    }
}
