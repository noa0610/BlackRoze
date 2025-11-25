using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class ShootStateBase : StateWithAnime, IGameObjectUser
    {
        // 弾丸にセットするレイヤー
        [SerializeField] protected LayerMask _targetLayer;
        [SerializeField] protected BulletData _data;         // 発射する弾のデータ
        [SerializeField] protected float _createPos = 0.35f;
        [SerializeField] protected Vector2 _direction = Vector2.right;
        [SerializeField] protected GameObject _muzzle;
        [SerializeField] protected UnityEvent _onShootComplete = new();

        public UnityEvent onShootComplete
        {
            get => _onShootComplete;
            set => _onShootComplete = value;
        }
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
            return true;
        }
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }
        public void SetLayer(LayerMask layer)
        {
            _targetLayer = layer;
        }
        public void SetGameObject(GameObject go, params GameObject[] options)
        {
            _muzzle = go;
        }
        public ShootStateBase SetDirection(Vector2 newDirection)
        {
            _direction = newDirection.normalized;
            return this;
        }
        public override void Enter(IState preview, UnitBase parent)
        {
            base.Enter(preview, parent);
            _ = Shoot(parent);
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

        protected virtual void InitBullet(Bullet bullet, Vector3 dict)
        {
            // ステータスをセット（速度、方向、ダメージなど）
            bullet.SetBulletStatus(_data, _targetLayer);
            bullet.SetDirection(dict);
            bullet.Invoke();
        }

    }
}