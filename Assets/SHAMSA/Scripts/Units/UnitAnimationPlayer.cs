using System;
using UnityEngine;

namespace SoulCrystalDefense.Units
{
    public class UnitAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        
        [Header("Animation Sets")]
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private float walkFps = 10f;

        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private float attackFps = 12f;

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

        private void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetFrames(Sprite[] walk, Sprite[] attack, Sprite[] death)
        {
            walkFrames = walk;
            attackFrames = attack;
            deathFrames = death;
        }

        public void PlayWalk()
        {
            if (currentState == AnimState.Walk) return;
            currentState = AnimState.Walk;
            PlayClip(walkFrames, walkFps, true, false, null);
        }

        public void PlayAttack(Action onComplete = null)
        {
            currentState = AnimState.Attack;
            PlayClip(attackFrames, attackFps, false, false, () =>
            {
                onComplete?.Invoke();
                if (currentState == AnimState.Attack)
                    PlayWalk();
            });
        }

        public void PlayDeath(Action onComplete = null)
        {
            currentState = AnimState.Death;
            PlayClip(deathFrames, deathFps, false, true, onComplete);
        }

        private void PlayClip(Sprite[] clip, float fps, bool loop, bool holdLast, Action onComplete)
        {
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
            if (currentClip == null || currentClip.Length == 0 || targetRenderer == null) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / currentFps;

            if (frameTimer >= frameDuration)
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
                        }
                        else
                        {
                            currentFrameIndex = 0;
                        }

                        var cb = onAnimComplete;
                        onAnimComplete = null;
                        cb?.Invoke();
                        return;
                    }
                }

                targetRenderer.sprite = currentClip[currentFrameIndex];
            }
        }
    }
}
