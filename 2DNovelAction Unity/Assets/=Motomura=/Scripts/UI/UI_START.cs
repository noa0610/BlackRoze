using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_START : MonoBehaviour
{
    [HideInInspector] public enum EffectProperty
    {
        [InspectorName("演出なし")] 
        Pop,

        [InspectorName("スライド")] 
        slide,

        [InspectorName("フェード（開発中）")] 
        fade
    }

    [System.Serializable]
    public class TargetObject
    {
        public EffectProperty Effect_Property;  
        
        [InspectorName("ターゲット")] 
        public GameObject target;

        [Header("スタートポジション")]
        public Vector2 S_Position;

        [Header("エンドポジション")]
        public Vector2 E_Position;

        [Header("表示にかかる時間")]
        public float _Time = 1.0f; 

        [Header("遅延")] 
        public float delayTime;
    }

    [SerializeField]
    public List<TargetObject> objects;

    private void Awake()
    {
        foreach (var obj in objects)
        {
            switch (obj.Effect_Property)
            {
                case EffectProperty.slide:
                    Slide_UI(obj);
                    break;

                case EffectProperty.fade:
                    Fade_UI(obj);
                    break;
                
                case EffectProperty.Pop:
                    SetActive_UI(obj);
                    break;
            }
        }
    }

    private void Slide_UI(TargetObject obj)
    {
        RectTransform rt = obj.target.GetComponent<RectTransform>();
        rt.anchoredPosition = obj.S_Position; // 初期位置を設定
        rt.DOAnchorPos(obj.E_Position, obj._Time).SetDelay(obj.delayTime);
    }

    private void Fade_UI(TargetObject obj)
    {
        obj.target.GetComponent<CanvasGroup>().DOFade(1.0f, obj._Time).SetDelay(obj.delayTime);
    }

    private void SetActive_UI(TargetObject obj)
    {
        RectTransform rt = obj.target.GetComponent<RectTransform>();
        rt.anchoredPosition = obj.E_Position;
    }
}