using BlackRose.Core.Models.EffectManager;
using BlackRose.Core.Models.Units;
using HighElixir.UI;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public partial class Stun : Idle_LazyEvent
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Vector2 _knockbackDirection = new Vector2(0.78f, 0.9f); // Default knockback direction
        [SerializeField] private TextThrower _thrower;
        public float StunTimer => _lazyChangeTime; // Expose the stun timer for external checks

        // ノックバックは親の向きを基準に力を加えます
        public Stun(Rigidbody2D rigidbody2D, float delay, bool isBlock)
            : base(delay, isBlock)
        {
            _rigidbody2D = rigidbody2D; // Store the Rigidbody2D reference for knockback
        }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            if (_thrower != null)
                _thrower.Create(parent.gameObject, "Stun!", Color.white);
            parent.Animator.SetFloat("StunTime", _lazyChangeTime);
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.AddForce(_knockbackDirection * -parent.Direction * 15f, ForceMode2D.Impulse); // Apply knockback force
            parent.GetComponent<SpriteEffectPlayer>().AddEffect(SpriteEffectHolders.SpriteEffects.Blinking, 1.5f);
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

        public Stun SetThrower(TextThrower thrower)
        {
            _thrower = thrower;
            return this;
        }
    }
}