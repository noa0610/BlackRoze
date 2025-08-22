using UnityEngine;
using System;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class ShootStateBase : StateWithAnime
    {
        // 弾丸にセットするレイヤー
        [SerializeField] protected LayerMask _targetLayer;
        [SerializeField] protected BulletData _data;         // 発射する弾のデータ
        [SerializeField] public UnityEvent onShootComplete = new();
        [SerializeField] protected float _allowShootCancel = 0.4f;

        // === Constractor ===
        public ShootStateBase(BulletData data, LayerMask targetLayer)
        {
            _data = data;
            _targetLayer = targetLayer; // レイヤーをセット
        }
        public ShootStateBase() { }
        // === Public ===
        public bool IsCancel(UnitBase parent)
        {
            return !HasTrigger(parent) || GetNormalized(parent) >= _allowShootCancel;
        }
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }
        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (base.AllowChange(nextState, parent)) return true;
            if (nextState is Stun) return true;
            if (IsCancel(parent) && nextState is MoveOnGround) return true;
            return false;
        }
        protected async virtual UniTask Shoot(UnitBase parent)
        {
            await UniTask.WaitUntil(() => IsCancel(parent));
            onShootComplete?.Invoke();
        }
    }
}