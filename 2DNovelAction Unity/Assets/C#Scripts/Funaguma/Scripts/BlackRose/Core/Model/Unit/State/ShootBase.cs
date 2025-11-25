using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using HighElixir.Implements.Observables;
using HighElixir.StateMachine;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class ShootBase<T> : State<T>, INotifyStateCompletion
        where T : UnitBase
    {
        [SerializeField] protected BulletData _data;         // 発射する弾のデータ
        [SerializeField] protected float _createPos = 0.35f;

        private ActionAsObservable _action = new();
        public IObservable<byte> Completion => _action;

        // === Constractor ===
        public ShootBase(BulletData data)
        {
            _data = data;
        }
        public ShootBase() { }
        // === Public ===
        public bool IsCancel()
        {
            return true;
        }
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }

        public override void Enter()
        {
            _ = Shoot();
        }
        public override bool AllowExit()
        {
            return IsCancel();
        }
        protected async virtual UniTask Shoot()
        {
            await UniTask.WaitUntil(() => IsCancel());
            _action?.Invoke();
        }

        protected virtual void InitBullet(Bullet bullet, Vector3 dict)
        {
            // ステータスをセット（速度、方向、ダメージなど）
            bullet.SetBulletStatus(_data, Cont.AttackLayer);
            bullet.SetDirection(dict);
            bullet.Invoke();
        }

    }
}