using BlackRose.Core.Models;
using BlackRose.Core.Models.Units;
using HighElixir;
using HighElixir.Unity.Pools;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace BlackRose.Core.UI
{
    [DefaultExecutionOrder(-1)]
    public class HPUI : SingletonBehavior<HPUI>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [Header("Pool")]
        [SerializeField] private Image _prefab;
        [SerializeField] private RectTransform _containar;
        [SerializeField] private int _size;
        [Header("Data"), SerializeField] private Vector2 _delta;
        private ObjectPool<Image> _pool;
        private Dictionary<object, (Image image, IDisposable disposable)> _owners = new();

        public void Get(UnitBase owner)
        {
            if (_owners.ContainsKey(owner)) return;
            var i = _pool.Pool.Get();
            var amount = owner.StatusManager.ReadValue(Status.HP);
            var dis =
                i.UpdateAsObservable()
                 .Where(_ => i.isActiveAndEnabled)
                 .Subscribe(_ =>
                 {
                     var screenPos = ScreenHelpers.WorldToUILocalPos(owner.transform.position, _camera, _canvas);
                     // ローカル座標にオフセットを足して配置
                     i.rectTransform.anchoredPosition = screenPos + _delta;

                     // HP比率更新
                     var current = owner.StatusManager.ReadValue(Status.HP);
                     var max = owner.StatusManager.ReadValue(Status.MaxHP);
                     i.fillAmount = current / max;
                 }).AddTo(this);
            _owners[owner] = (i, dis);
        }
        public void Release(UnitBase owner)
        {
            _pool.Pool.Release(_owners[owner].image);
            _owners[owner].disposable.Dispose();
            _owners.Remove(owner);
        }
        protected override void Awake()
        {
            base.Awake();
            _pool = new ObjectPool<Image>(_prefab, _size, _containar);
        }
    }
}