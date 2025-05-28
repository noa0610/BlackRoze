using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Minigames.UI
{
    public class PopText : MonoBehaviour
    {
        private class PopEntry
        {
            public TextMeshProUGUI text;
            public RectTransform rect;
            public Color startColor;
            public float elapsed;
        }

        [Header("Reference")]
        [SerializeField] TextMeshProUGUI _uGUI;
        [SerializeField] PopTextData _popTextData;
        [SerializeField] RectTransform _canvasRect;
        [SerializeField] Camera _camera;

        [Header("Data")]
        [SerializeField] float _duration = 1f;
        [SerializeField] Vector2 _moveDir = Vector2.up;
        [SerializeField] float _speed = 50f;
        [SerializeField] int _maxEntries = 10;
        [SerializeField] Easing.Ease _ease;

        Pool<TextMeshProUGUI> _pool;
        List<PopEntry> _entries = new();
        private Func<float, float> _easeMethod;

        void Awake()
        {
            // Nullチェック
            if (!_uGUI || _popTextData == null || !_canvasRect || !_camera)
            {
                Debug.LogError("Inspector設定不足！", this);
                enabled = false;
                return;
            }
            _pool = new Pool<TextMeshProUGUI>(_uGUI, 5, _canvasRect, true);
            _easeMethod += Easing.GetEasingMethod(_ease);
        }

        public void Make(int id, Transform target)
        {
            // 上限越えたら一番古いやつを強制リリース
            if (_entries.Count >= _maxEntries)
            {
                var old = _entries[0];
                _entries.RemoveAt(0);
                _pool.Release(old.text);
            }

            var txt = _pool.Get();
            var entry = new PopEntry
            {
                text = txt,
                rect = txt.rectTransform,
                startColor = txt.color,
                elapsed = 0f
            };
            // スクリーン→ローカル座標でanchoredPositionセット
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(_camera, target.position);
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //    _canvasRect, screen, _camera, out var local);
            entry.rect.position = screen;

            var (msg, col) = _popTextData.GetInfo(id);
            txt.SetText(msg);
            entry.startColor = col;
            if (_popTextData.FontAsset != null) txt.font = _popTextData.FontAsset;
            txt.gameObject.SetActive(true);

            _entries.Add(entry);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var e = _entries[i];
                e.elapsed += dt;
                float t = e.elapsed / _duration;

                // 移動
                e.rect.anchoredPosition += _moveDir * _speed * dt;
                // フェードアウト
                e.text.color = Color.Lerp(e.startColor, new Color(e.startColor.r, e.startColor.g, e.startColor.b, 0), _easeMethod(t));

                if (e.elapsed >= _duration)
                {
                    e.text.color = Color.white;
                    _pool.Release(e.text);
                    _entries.RemoveAt(i);
                }
            }
        }
    }
}
// unicode