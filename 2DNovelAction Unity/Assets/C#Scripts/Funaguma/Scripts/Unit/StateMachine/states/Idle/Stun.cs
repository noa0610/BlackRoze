using HighElixir.Pool;
using UnityEngine;

namespace BlackRose
{
    public class Stun : Idle
    {
        private float _stunTime;
        private float _stunTimer = 0f;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _knockbackDirection = new Vector2(0.6f, 0.4f); // Default knockback direction
        private PopText _popText;
        public float StunTimer => _stunTimer; // Expose the stun timer for external checks

        // ノックバックは親の向きを基準に力を加えます
        public Stun(Rigidbody2D rigidbody2D, PopText popText, float stunTime, Vector2 knockbackDirection = new(), string animationKey = "Stun") : base(animationKey)
        {
            _stunTime = stunTime;
            _rigidbody2D = rigidbody2D; // Store the Rigidbody2D reference for knockback
            _popText = popText;
            if (knockbackDirection != Vector2.zero)
            {
                _knockbackDirection = knockbackDirection.normalized; // Ensure the knockback direction is normalized
            }
        }

        public override bool Enter(IState previousState, IUnit parent)
        {
            _stunTimer = _stunTime; // Initialize the stun timer
            _popText.CreateText((parent as UnitBase).transform, "Stun!");
            parent.Animator.SetFloat("StunTime", _stunTime);
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.AddForce(_knockbackDirection * parent.Direction * 10f, ForceMode2D.Impulse); // Apply knockback force
            return true;
        }

        public override bool Stay(IUnit parent)
        {

            if (_stunTimer <= 0f) return true;
            _stunTimer = Mathf.Max(0f, _stunTimer - Time.deltaTime); // Decrease the stun timer
            return true; // Continue staying in the Stun state
        }
    }
}