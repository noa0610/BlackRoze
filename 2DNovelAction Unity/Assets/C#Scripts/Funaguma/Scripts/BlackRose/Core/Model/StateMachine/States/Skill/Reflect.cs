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