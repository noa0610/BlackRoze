using HighElixir.Implements.Observables;
using HighElixir.StateMachine;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class Warp : State<AIController>, INotifyStateCompletion
    {
        [SerializeField]
        private Vector2 _pos;

        private ActionAsObservable _action;
        public IObservable<byte> Completion => _action;

        public override void Enter()
        {
            var start = (Vector2)Cont.transform.position;
            var vec = _pos - start;

            // ===== キャラの判定サイズを取得 =====
            var col = Cont.GetComponent<CapsuleCollider2D>();
            if (col == null)
            {
                Debug.LogWarning("Warp: CapsuleCollider2D が見つからないので直接ワープします");
                Cont.transform.position = _pos;
                _action?.Invoke();
                return;
            }

            Vector2 size = col.size;
            float radius = size.x * 0.5f;       // 横幅の半分を半径に
            float height = size.y;              // 高さ
            float angle = 0f;                   // CapsuleCastの回転(縦なら0)

            int mask = LayerMask.GetMask("Wall");

            // ===== カプセルキャストで経路をチェック =====
            RaycastHit2D hit = Physics2D.CapsuleCast(
                start,
                size,
                CapsuleDirection2D.Vertical,
                angle,
                vec.normalized,
                vec.magnitude,
                mask
            );

            Vector2 targetPos;

            if (hit.collider != null)
            {
                // 壁に当たった → ちょっと手前で止める
                targetPos = hit.point - vec.normalized * 0.05f;
            }
            else
            {
                // 当たらなかった → 目的地候補へ
                targetPos = _pos;
            }

            // ===== 最終チェック (OverlapCircle) =====
            // 目的地が空いてるか調べる
            bool blocked = Physics2D.OverlapCircle(targetPos, radius, mask);

            if (blocked)
            {
                Debug.Log("Warp: 目的地が塞がれているのでキャンセル/調整");
                // ここでキャンセルするか、安全地点にずらす処理を入れる
                // 例: 壁から少し戻す
                if (hit.collider != null)
                {
                    targetPos = hit.point - vec.normalized * 0.2f;
                }
                else
                {
                    // 仕方なく出発点に戻す
                    targetPos = start;
                }
            }

            // ===== ワープ実行 =====
            Cont.transform.position = targetPos;

            _action?.Invoke();
        }



        public void SetPos(Vector2 pos)
        {
            _pos = pos;
        }


    }
}