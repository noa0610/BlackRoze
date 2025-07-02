using HighElixir.Pool;
using System;
using System.Linq.Expressions;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class Stun : Idle
    {
        [SerializeField] private float _stunTime;
        [SerializeField] private float _stunTimer = 0f;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Vector2 _knockbackDirection = new Vector2(0.78f, 0.9f); // Default knockback direction
        [SerializeField] private PopText _popText;
        public float StunTimer => _stunTimer; // Expose the stun timer for external checks

        // ノックバックは親の向きを基準に力を加えます
        public Stun(Rigidbody2D rigidbody2D, PopText popText, float stunTime, Vector2 knockbackDirection = new(), string animationKey = "Stun")
        {
            _stunTime = stunTime;
            _rigidbody2D = rigidbody2D; // Store the Rigidbody2D reference for knockback
            _popText = popText;
            if (knockbackDirection != Vector2.zero)
            {
                _knockbackDirection = knockbackDirection.normalized; // Ensure the knockback direction is normalized
            }
        }
        public Stun() { }
        public override void Enter(IState previousIState, IUnit parent)
        {
            _stunTimer = _stunTime; // Initialize the stun timer
            _popText.CreateText((parent as UnitBase).transform, "Stun!");
            parent.Animator.SetFloat("StunTime", _stunTime);
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.AddForce(_knockbackDirection * parent.Direction * 15f, ForceMode2D.Impulse); // Apply knockback force
            parent.Player.AddEffect(SpriteEffectHolders.SpriteEffects.Blinking, 1.5f);
            parent.StatusEffectManager.AddEffect(StatusEffectHolder.instance.Stun.EffectFactory());
        }
        public override void Stay(IUnit parent)
        {
            if (_stunTimer > 0f)
                _stunTimer = Mathf.Max(0f, _stunTimer - Time.deltaTime); // Decrease the stun timer
        }

        public override bool AllowChange(IState nextState, IUnit parent)
        {
            return _stunTimer <= 0f; // Check if the stun duration has ended
        }
    }
}