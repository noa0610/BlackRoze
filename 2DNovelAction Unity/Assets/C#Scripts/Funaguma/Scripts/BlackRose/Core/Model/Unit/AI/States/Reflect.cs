using HighElixir.StateMachine;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    /// <summary>
    /// Reflect State
    /// </summary>
    [Serializable]
    public class Reflect : State<AIController>
    {
        [SerializeField]
        private float _lockFrame = 8f;
        public event System.Action OnCompleted;

        public override void Enter()
        {
            Cont.ReflectMono.gameObject.SetActive(true);
        }

        public override void Update(float deltaTime)
        {
            Cont.ReflectMono.transform.position = Cont.transform.position;
        }
        public override void Exit()
        {
            if (Cont.ReflectMono.gameObject.activeInHierarchy)
            {
                Cont.ReflectMono.gameObject.SetActive(false);
            }
        }
    }
}