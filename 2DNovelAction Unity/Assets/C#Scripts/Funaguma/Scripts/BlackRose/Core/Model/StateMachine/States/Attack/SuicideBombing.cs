using BlackRose.Core.Models.Units;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;
using BlackRose.Core.Models.Units;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class SuicideBombing : StateWithAnime
    {
        [Header("爆発")]
        [Tooltip("爆風のノックバック")]
        [SerializeField] private float _knockBack = 0f;
        [Tooltip("爆風の持続時間")]
        [SerializeField] private float _time = 1.5f;
        [Tooltip("爆風範囲")]
        [SerializeField, Min(0f)] private float _radius = 5f;
        [Tooltip("未設定（-5）の場合、_statusのdamageを使用")]
        [SerializeField] private float _damage = -5f;
        [Tooltip("爆発遅延")]
        [SerializeField, Min(0)] private float _delay = 0.4f;
        [Tooltip("爆発の中心")]
        [SerializeField] private CircleCollider2D _collider;
        private bool _isExploding = false;

        public UnityEvent OnExplode { get; set; } = new UnityEvent();

        public override void Enter(IState previousIState, UnitBase parent)
        {
            Debug .Log("Enter of SuicideBombing");
            _ = Explode(parent);
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            // 爆発タスクが完了していない場合は遷移を許可しない
            if (_isExploding)
            {
                return false;
            }
            return base.AllowChange(nextState, parent);
        }
        private async UniTask Explode(UnitBase parent)
        {
            _isExploding = true;
            _collider.radius = _radius;
            _collider.enabled = true; // コライダーを有効化
            await UniTask.WaitForSeconds(_delay);

            var hitColliders = Physics2D.OverlapCircleAll(_collider.transform.position, _radius);
            Debug.Log($"hitted : {hitColliders.Length}");
            foreach (var hitCollider in hitColliders)
            {
                Debug.Log($"Hit: {hitCollider.name}");
                if (hitCollider.TryGetComponent<Rigidbody>(out var rigidbody))
                {
                    // 爆発の中心から対象オブジェクトへの方向ベクトルを正規化
                    var direction = (hitCollider.transform.position - _collider.transform.position).normalized;
                    // ノックバックの力を計算
                    var force = direction * _knockBack;
                    // Rigidbody に力を加える
                    rigidbody.AddForce(force, ForceMode.Impulse);
                }
                if (hitCollider.TryGetComponent<UnitBase>(out var unit))
                {
                    Debug.Log($"Damage dealt to {unit.name}: { _damage}");
                    UnitManager.instance.AddDamage(unit, parent, _damage == -5 ? 1 : _damage);
                }
            }

            await UniTask.WaitForSeconds(_time);
            _collider.enabled = false; // コライダーを無効化
            _isExploding = false;

            OnExplode?.Invoke(); // 爆発イベントを呼び出す
        }
    }
}