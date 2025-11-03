using UnityEngine;
using UniRx;
namespace BlackRose.Core.Models.Units
{
    public class AutoFlipHelper : MonoBehaviour
    {
        private UnitBase _target;
        [SerializeField] private bool _enable = true;
        public bool Enable { get => _enable; set => _enable = value; }
        private void Awake()
        {
            if (!TryGetComponent<UnitBase>(out _target))
            {
                Destroy(gameObject);
                return;
            }
            _target.ReactiveDirection.Subscribe(d =>
            {
                if (!Enable) return;
                if (d.x > 0) transform.localScale = Vector3.one;
                else if (d.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }).AddTo(this);
        }
    }
}