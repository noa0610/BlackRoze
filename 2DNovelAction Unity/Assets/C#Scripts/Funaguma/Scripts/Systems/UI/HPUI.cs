using BlackRose.Core.Models;
using HighElixir.Pool;
using HighElixir.Utilities;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace BlackRose.UI
{
    [DefaultExecutionOrder(-1)]
    public class HPUI : SingletonBehavior<HPUI>
    {
        [SerializeField] private Camera _camera;
        [Header("Pool")]
        [SerializeField] private Image _prefab;
        [SerializeField] private RectTransform _containar;
        [SerializeField] private int _size;
        [Header("Data"), SerializeField] private Vector2 _delta;
        private Pool<Image> _pool;
        private Dictionary<object, (Image image, IDisposable disposable)> _owners = new();

        public void Get(UnitBase owner)
        {
            if (_owners.ContainsKey(owner)) return;
            var i = _pool.Get();
            var amount = owner.StatusManager.ReadValue(Status.HP);
            var dis =
                i.UpdateAsObservable()
                 .Where(_ => i.isActiveAndEnabled)
                 .Subscribe(_ =>
                 {
                     // ワールド → スクリーン座標
                     Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_camera, owner.transform.position);

                     // スクリーン → コンテナのローカル座標
                     RectTransformUtility.ScreenPointToLocalPointInRectangle(
                         _containar,          // 親RectTransform
                         screenPos,           // スクリーン座標
                         _camera,             // カメラ（Screen Space - Camera の場合）
                         out Vector2 localPos // 出力されるローカル座標
                     );

                     // ローカル座標にオフセットを足して配置
                     i.rectTransform.anchoredPosition = localPos + _delta;

                     // HP比率更新
                     var current = owner.StatusManager.ReadValue(Status.HP);
                     var max = owner.StatusManager.ReadValue(Status.MaxHP);
                     i.fillAmount = current / max;
                 }).AddTo(this);
            _owners[owner] = (i, dis);
        }
        public void Release(UnitBase owner)
        {
            _pool.Release(_owners[owner].image);
            _owners[owner].disposable.Dispose();
            _owners.Remove(owner);
        }
        protected override void Awake()
        {
            base.Awake();
            _pool = new Pool<Image>(_prefab, _size, _containar);
        }
    }
}