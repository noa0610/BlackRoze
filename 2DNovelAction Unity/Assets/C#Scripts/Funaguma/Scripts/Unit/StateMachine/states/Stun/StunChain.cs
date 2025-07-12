using HighElixir.UI;
using UnityEngine;

namespace BlackRose
{
    public partial class Stun
    {
        public Stun SetKnockback(Vector2 knockBack)
        {
            if (knockBack != Vector2.zero)
            {
                _knockbackDirection = knockBack.normalized; // Ensure the knockback direction is normalized
            }
            return this;    
        }
        public Stun SetDuration(float duration)
        {
            _stunTime = duration;
            return this;
        }

        public Stun SetThrower(TextThrower thrower)
        {
            _thrower = thrower;
            return this;
        }
    }
}