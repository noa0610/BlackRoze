using BlackRose.Core.Models.Objects;
using UnityEngine;

namespace BlackRose
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class DamageFloorTest : MonoBehaviour
    {
        [SerializeField] private DamageFloorMaker _maker;
        [SerializeField] private float _length;
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