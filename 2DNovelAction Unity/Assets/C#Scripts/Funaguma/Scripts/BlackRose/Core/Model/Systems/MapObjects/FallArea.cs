using BlackRose.Core.Models.Units;
using UnityEngine;
namespace BlackRose.Core.Models.Objects
{
    public class FallArea : MonoBehaviour
    {
        [SerializeField] private string _tag = "Player"; // プレイヤーのタグ
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(_tag))
            {
                // プレイヤーが落下エリアに入ったときの処理
                Debug.Log($"{collision.gameObject.name} has fallen into the area: {gameObject.name}");
                // ここでプレイヤーをリスポーンさせるなどの処理を追加できます
                PlayerSpawnner.Spawn(collision.GetComponent<UnitBase>());
            }
        }
    }
}