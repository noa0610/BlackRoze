using BlackRose.Core.Models.Objects;
using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// Reflect State
    /// </summary>
    [Serializable]
    public class Reflect : StateComp, ICompleteEmitter, IGameObjectUser
    {
        private GameObject _go;
        [SerializeField]
        private float _lockFrame = 8f;
        public event System.Action OnCompleted;

        public Reflect()
            : base()
        {
            // 反射中は8フレームステート変更を禁止する
            Timer.ChangeDuration(nameof(_waitFrame), _lockFrame);
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            if (_go.TryGetComponent<ReflectMono>(out var _))
            {
                Debug.Log("ReflectMono component found.");
                OnCompleted?.Invoke();
            }
            // 反射エフェクトを再生する
            //var effect = _go.GetComponent<BlackRose.Core.Views.Effects.ReflectEffect>();
            //if (effect != null)
            //{
            //    effect.Play();
            //}
            _go.SetActive(true);
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            _go.transform.position = parent.transform.position;
        }
        public override void Exit(IState nextState, UnitBase parent)
        {
            base.Exit(nextState, parent);
            if (_go.activeInHierarchy)
            {
                _go.SetActive(false);
                OnCompleted?.Invoke();
            }
        }
        /// <summary>
        /// 反射の起点となるGameObjectを設定する
        /// </summary>
        /// <param name="go"></param>
        /// <param name="_"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetGameObject(UnityEngine.GameObject go, params UnityEngine.GameObject[] _)
        {
            _go = go;
        }
    }
}