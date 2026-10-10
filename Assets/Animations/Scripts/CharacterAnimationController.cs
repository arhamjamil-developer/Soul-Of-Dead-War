using System.Collections;
using UnityEngine;

namespace SoulOfDeadWar.Animation
{
    /// <summary>
    /// Handles 2D top-down character animation state changes, 6-frame pacing triggers,
    /// horizontal directional flipping, and combat damage feedback.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        public enum CharacterType
        {
            BarbarianMan,
            WarElephant,
            BattleChef,
            IronTrain
        }

        [Header("Character Configuration")]
        [SerializeField] private CharacterType characterType = CharacterType.BarbarianMan;

        [Header("Locomotion Tuning")]
        [Tooltip("Minimum horizontal velocity to trigger sprite flip")]
        [SerializeField] private float flipThreshold = 0.05f;

        [Header("Components")]
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private SpriteVFXController _vfxController;

        // Cached Animator parameter hashes
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        public CharacterType Type => characterType;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _vfxController = GetComponent<SpriteVFXController>();
        }

        /// <summary>
        /// Updates locomotion blend tree parameters and handles horizontal facing.
        /// </summary>
        /// <param name="velocity">World velocity vector (X, Y)</param>
        public void UpdateMovement(Vector2 velocity)
        {
            if (IsDead) return;

            float speed = velocity.magnitude;
            _animator.SetFloat(SpeedHash, speed);
            _animator.SetFloat(MoveXHash, velocity.x);
            _animator.SetFloat(MoveYHash, velocity.y);

            // Top-down horizontal sprite flip
            if (velocity.x > flipThreshold)
            {
                _spriteRenderer.flipX = false;
            }
            else if (velocity.x < -flipThreshold)
            {
                _spriteRenderer.flipX = true;
            }
        }

        /// <summary>
        /// Triggers the 6-frame Attack animation.
        /// </summary>
        public void TriggerAttack()
        {
            if (IsDead) return;
            _animator.SetTrigger(AttackTriggerHash);
        }

        /// <summary>
        /// Triggers the 6-frame Hurt animation and the Hit-Flash / Outline VFX.
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (IsDead) return;

            _animator.SetTrigger(HurtTriggerHash);

            if (_vfxController != null)
            {
                _vfxController.TriggerHitFlash();
            }
        }

        /// <summary>
        /// Transitions character into the 6-frame Dead collapse animation.
        /// </summary>
        public void Die()
        {
            if (IsDead) return;

            IsDead = true;
            _animator.SetBool(IsDeadHash, true);

            if (_vfxController != null)
            {
                _vfxController.SetOutline(false);
            }
        }
    }
}
