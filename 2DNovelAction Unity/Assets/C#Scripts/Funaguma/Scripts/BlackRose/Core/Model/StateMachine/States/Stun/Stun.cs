using BlackRose.Core.Models.EffectManager;
using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public partial class Stun : Idle_LazyEvent, IRigidbodyUser
    {
        [SerializeField] private Vector2 _knockbackDirection = new Vector2(0.78f, 0.9f); // Default knockback direction
        public float StunTimer => _eventTime; // Expose the stun timer for external checks

        public float KnockbackForce { get; set; } = 15f; // Fixed knockback force
        public Rigidbody2D Rigidbody2D {  get; private set; }

        // ノックバックは親の向きを基準に力を加えます
        public Stun(Rigidbody2D rigidbody2D, float delay, bool isBlock)
            : base(delay, isBlock)
        {
            Rigidbody2D = rigidbody2D; // Store the Rigidbody2D reference for knockback
        }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            parent.Animator.SetFloat("StunTime", _eventTime);
            Rigidbody2D.velocity = Vector2.zero;
            Rigidbody2D.AddForce(_knockbackDirection * parent.Direction * KnockbackForce, ForceMode2D.Impulse); // Apply knockback force
            parent.SpriteEffectPlayer.AddEffect(SpriteEffectHolders.SpriteEffects.Blinking, 1.5f);
            parent.StatusEffectManager.AddEffect(StatusEffectHolder.instance.Stun.EffectFactory());
        }
        public Stun SetKnockback(Vector2 knockBack)
        {
            if (knockBack != Vector2.zero)
            {
                _knockbackDirection = knockBack.normalized; // Ensure the knockback direction is normalized
            }
            return this;
        }

        public void SetRB2(Rigidbody2D rb)
        {
            Rigidbody2D = rb;
        }
    }
}