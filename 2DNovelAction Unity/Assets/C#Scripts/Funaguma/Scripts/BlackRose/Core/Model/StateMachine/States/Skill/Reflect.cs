using UnityEngine;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// Reflect State
    /// </summary>
    public class Reflect : StateComp, ICompleteEmitter, IGameObjectUser
    {
        private GameObject _go;
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
            // 反射エフェクトを再生する
            var effect = _go.GetComponent<BlackRose.Core.Views.Effects.ReflectEffect>();
            if (effect != null)
            {
                effect.Play();
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