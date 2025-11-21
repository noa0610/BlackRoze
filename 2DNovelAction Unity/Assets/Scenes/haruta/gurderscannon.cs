using UnityEngine;
using BlackRose.Core.Models.Units;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class gurderscannon : Bullet
    {
        [Header("Initial impulse (applied once when Invoke is called)")]
        [SerializeField] private float initialForce = 10f;
        private Rigidbody2D _rb;
        private bool _impulseApplied = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        private void Start()
        {
            // インスタンス化された瞬間に発射処理を行う
            ApplyInitialForce();
        }

        public override void Invoke()
        {
            base.Invoke();
            ApplyInitialForce();
        }

        private void ApplyInitialForce()
        {
            if (_impulseApplied) return;

            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                var dir = Vector2.zero;
                var gurter = GetComponentInParent<BlackRose.Core.Models.Units.Enemy_Gurter>();
                if (gurter != null)
                {
                    // ガーダーの角度と向きを取得
                    float angleRad = gurter.AngleDeg * Mathf.Deg2Rad;
                    int facing = gurter.FacingSign;
                    
                    // ガーダーの向きに基づいて弾の方向を設定
                    dir = new Vector2(facing * Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
                    Debug.Log($"[gurderscannon] ガーダーの向き: {facing}, 角度: {gurter.AngleDeg}度");
                }
                else
                {
                    // ガーダーが見つからない場合はデフォルトの方向を使用
                    dir = _direction != Vector2.zero ? _direction : Vector2.right;
                    Debug.Log("[gurderscannon] ガーダーが見つからないためデフォルトの方向を使用");
                }

               Debug.Log($"[gurderscannon] Invoke: dir={dir}, initialForce={initialForce}");
                _rb.AddForce(dir.normalized * initialForce, ForceMode2D.Impulse);
                // フェールセーフ（必要なら有効化）: インパルスで動かない場合は直接速度をセット
                // _rb.velocity = dir.normalized * initialForce;
                _impulseApplied = true;
            }
            else
            {
                Debug.LogWarning($"[{nameof(gurderscannon)}] Rigidbody2D not found. Unable to apply initial impulse.");
            }
        }
// ...existing code...
    }
// ...existing code...
}
