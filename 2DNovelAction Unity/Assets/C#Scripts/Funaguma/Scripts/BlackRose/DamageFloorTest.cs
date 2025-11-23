using BlackRose.Core.Models.Objects;
using UnityEngine;

namespace BlackRose
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class DamageFloorTest : MonoBehaviour
    {
        [SerializeField] private DamageFloorMaker _maker;
        [SerializeField] private float _length;

        private void Awake()
        {
            // もしInspectorで未設定なら自動取得
            if (_maker == null)
            {
                _maker = FindObjectOfType<DamageFloorMaker>();
                if (_maker == null)
                {
                    Debug.LogError("DamageFloorMaker がシーン上に見つかりません。");
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Hitted.");
            if (collision != null && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _maker.Create(collision.contacts[0].point, 10f, _length);
            }
        }
    }
}
