using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class EnemySpaner : MonoBehaviour
    {
        [SerializeField] private GameObject EnemyPrfab;
        [SerializeField] private LayerMask TargetLayer;


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((1 << collision.gameObject.layer & TargetLayer) != 0)
            {
                Debug.Log(collision.gameObject.name + "が対象レイヤーと一致しました (トリガー)");
                Spawn();
            }
        }

        private void Spawn()
        {
            GameObject enemy = Instantiate(EnemyPrfab, transform.localPosition, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}