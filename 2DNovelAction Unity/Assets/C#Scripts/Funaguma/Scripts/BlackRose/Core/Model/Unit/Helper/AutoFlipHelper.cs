using UnityEngine;
using UniRx;
using System;
namespace BlackRose.Core.Models.Units
{
    public class AutoFlipHelper : MonoBehaviour
    {
        [SerializeField] private bool _enable = true;
        private UnitBase _target;
        private readonly Subject<Vector2> _onFlipped = new Subject<Vector2>();
        public bool Enable { get => _enable; set => _enable = value; }
        public IObservable<Vector2> OnFlipped => _onFlipped;
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
                _onFlipped.OnNext(d);
            }).AddTo(this);
        }

        private void OnDestroy()
        {
            _onFlipped.OnCompleted();
        }
    }
}