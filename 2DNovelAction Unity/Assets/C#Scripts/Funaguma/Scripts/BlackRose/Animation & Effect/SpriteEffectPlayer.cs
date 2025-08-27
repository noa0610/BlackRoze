using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BlackRose
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteEffectPlayer : MonoBehaviour
    {
        private List<SpriteEffectDataHolder> _spriteEffects = new();
        private SpriteRenderer _spriteRenderer;

        public void AddEffect(SpriteEffectHolders.SpriteEffects spriteEffects, float duration, float speed = 1f)
        {
            var effect = SpriteEffectHolders.GetEffect(spriteEffects);
            if (effect != null)
            {
                var item = new SpriteEffectDataHolder();
                item.action = effect;
                item.value.duration = duration;
                item.value.remainingDuration = duration;
                item.value.speed = speed;
                _spriteEffects.Add(item);
            }
        }

        // === Unity Lifecycle ===
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void Update()
        {
            var dT = Time.deltaTime;
            var t = Time.time;
            for (int i = 0; i < _spriteEffects.Count; i++)
            {
                var item = _spriteEffects[i];
                item.value.remainingDuration = Mathf.Max(item.value.remainingDuration - dT, 0);
                item.value.deltaTime = dT;
                item.value.time = t;
                if (item.value.remainingDuration <= 0)
                    item.value.mustRemove = true;
                item.Invoke(_spriteRenderer);
            }
            _spriteEffects.RemoveAll(item => item.value.mustRemove);
        }
#if UNITY_EDITOR
        [Header("Debugs")]
        [SerializeField] private SpriteEffectHolders.SpriteEffects d_target;
        [SerializeField] private float d_duration;
        [SerializeField] private float d_speed;
        public void AddEffect_Debug()
        {
            AddEffect(d_target, d_duration, d_speed);
        }
#endif
    }

    public class SpriteEffectDataHolder
    {
        public Action<SpriteRenderer, SpriteEffectHolders.Value> action;
        public SpriteEffectHolders.Value value;

        public void Invoke(SpriteRenderer renderer) => action?.Invoke(renderer, value);
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SpriteEffectPlayer))]
    public class ExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpriteEffectPlayer t = target as SpriteEffectPlayer;

            if (GUILayout.Button("PlayEffect"))
            {
                t.AddEffect_Debug();
            }
        }
    }
#endif
}