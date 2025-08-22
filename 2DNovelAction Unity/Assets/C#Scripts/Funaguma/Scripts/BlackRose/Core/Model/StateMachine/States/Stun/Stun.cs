using BlackRose.Core.Models.EffectManager;
using BlackRose.Core.Models.Units;
using HighElixir.UI;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public partial class Stun : Idle
    {
        [SerializeField] private float _stunTime;
        [SerializeField] private float _stunTimer = 0f;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Vector2 _knockbackDirection = new Vector2(0.78f, 0.9f); // Default knockback direction
        [SerializeField] private TextThrower _thrower;
        public float StunTimer => _stunTimer; // Expose the stun timer for external checks

        // ノックバックは親の向きを基準に力を加えます
        public Stun(Rigidbody2D rigidbody2D)
        {
            _rigidbody2D = rigidbody2D; // Store the Rigidbody2D reference for knockback
        }
        public Stun() { }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            _stunTimer = _stunTime; // Initialize the stun timer
            if (_thrower != null)
                _thrower.Create(parent.gameObject, "Stun!", Color.white);
            parent.Animator.SetFloat("StunTime", _stunTime);
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.AddForce(_knockbackDirection * parent.Direction * 15f, ForceMode2D.Impulse); // Apply knockback force
            parent.GetComponent<SpriteEffectPlayer>().AddEffect(SpriteEffectHolders.SpriteEffects.Blinking, 1.5f);
            parent.StatusEffectManager.AddEffect(StatusEffectHolder.instance.Stun.EffectFactory());
        }
        public override void Stay(UnitBase parent)
        {
            if (_stunTimer > 0f)
                _stunTimer = Mathf.Max(0f, _stunTimer - Time.deltaTime); // Decrease the stun timer
            else
                Debug.Log("AAAa");
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            return _stunTimer <= 0f; // Check if the stun duration has ended
        }
    }
}