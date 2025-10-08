using UnityEngine;
using UniRx;
namespace BlackRose.Core.Models.Units
{
    public class AutoFlippingUnit : MonoBehaviour
    {
        private UnitBase _target;
        private void Awake()
        {
            if (!TryGetComponent<UnitBase>(out _target))
            {
                Destroy(gameObject);
                return;
            }
            _target.ReactiveDirection.Subscribe(d =>
            {
                if (d.x > 0) transform.localScale = Vector3.one;
                else if (d.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }).AddTo(this);
        }
    }
}