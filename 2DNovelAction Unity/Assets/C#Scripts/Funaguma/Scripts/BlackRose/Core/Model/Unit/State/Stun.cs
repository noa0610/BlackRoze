using BlackRose.Core.Models.EffectManager;
using HighElixir.StateMachine;
using System;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public partial class Stun<T> : State<T>, INotifyStateCompletion
        where T : UnitBase
    {
        [SerializeField] private Vector2 _knockbackDirection = new Vector2(0.78f, 0.9f);
        [SerializeField] private float _stunTime = 0.6f;
        private ReactiveCommand<byte> _command = new();
        public float KnockbackForce { get; set; } = 15f; // Fixed knockback force

        public IObservable<byte> Completion => _command;

        public override void Enter()
        {
            Cont.Rigidbody2D.velocity = Vector2.zero;
            var dir = Cont.Direction.x < 0 ? -1 : 1;
            Cont.Rigidbody2D.AddForce(_knockbackDirection * dir * KnockbackForce, ForceMode2D.Impulse); // Apply knockback force
            Cont.SpriteEffectPlayer.AddEffect(SpriteEffectHolders.SpriteEffects.Blinking, _stunTime);
            Cont.StatusEffectManager.AddEffect(StatusEffectHolder.instance.Stun.EffectFactory());
            _command.Execute(0);
        }
        public void SetKnockback(Vector2 knockBack)
        {
            if (knockBack != Vector2.zero)
            {
                _knockbackDirection = knockBack.normalized; // Ensure the knockback direction is normalized
            }
        }
    }
}