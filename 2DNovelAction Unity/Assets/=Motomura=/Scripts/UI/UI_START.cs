using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UISTART : MonoBehaviour
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

        [Header("スタートポジション")] [Tooltip("ポジションはワールド座標を入力してください。")]
        public Vector3 S_Position;

        [Header("エンドポジション")] [Tooltip("ポジションはワールド座標を入力してください。")]
        public Vector3 E_Position;

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
        obj.target.transform.DOMove(obj.E_Position, obj._Time).SetDelay(obj.delayTime); 
    }

    private void Fade_UI(TargetObject obj)
    {
        obj.target.GetComponent<CanvasGroup>().DOFade(1.0f, obj._Time).SetDelay(obj.delayTime);
    }

    private void SetActive_UI(TargetObject obj)
    {
        obj.target.transform.DOMove(obj.E_Position, obj._Time).SetDelay(obj.delayTime);
    }
}
