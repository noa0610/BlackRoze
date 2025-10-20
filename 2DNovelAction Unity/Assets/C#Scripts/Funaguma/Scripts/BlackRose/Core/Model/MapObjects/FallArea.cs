using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using UnityEngine;
namespace BlackRose.Core.Models.Objects
{
    public class FallArea : MonoBehaviour, IPlayerFollower
    {
        private UnitBase _target;

        public void SetTarget(UnitBase target)
        {
            _target = target;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.Equals(_target.gameObject))
            {
                // プレイヤーが落下エリアに入ったときの処理
                Debug.Log($"{collision.gameObject.name} has fallen into the area: {gameObject.name}");
                // ここでプレイヤーをリスポーンさせるなどの処理を追加できます
                PlayerSpawnner.Spawn(collision.GetComponent<UnitBase>());
            }
        }
    }
}