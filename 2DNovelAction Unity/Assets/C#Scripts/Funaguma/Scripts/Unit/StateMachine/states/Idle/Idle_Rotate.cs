using System;
using UnityEngine;

namespace BlackRose
{
    // =======================
    // Idle（待機）状態
    // =======================
    [Serializable]
    public class Idle_Rotate : Idle
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private float _rotateSpeed;
        public Idle_Rotate(Transform transform, float rotateSpeed)
        {
            _transform = transform;
            _rotateSpeed = rotateSpeed;
        }
        public Idle_Rotate() { }
        public override void Stay(IUnit parent)
        {
            _transform.Rotate(Vector3.forward, _rotateSpeed * Time.deltaTime);
        }
    }
}
