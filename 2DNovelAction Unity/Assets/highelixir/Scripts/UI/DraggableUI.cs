using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HighElixir.UI
{
    public class DraggableUI : MonoBehaviour,
        IDragHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        [SerializeField] private float snapSize = 20f;
        private RectTransform rt;
        private Canvas canvas; // Canvasスケール考慮するなら必要

        // UnityEvents
        public UnityEvent OnBeginDragEvent = new();
        public UnityEvent OnEndDragEvent = new();

        public void OnBeginDrag(PointerEventData e)
            => OnBeginDragEvent.Invoke();

        public void OnDrag(PointerEventData e)
        {
            UpdatePosition(e.position, false);
        }

        public void OnEndDrag(PointerEventData e)
        {
            UpdatePosition(e.position, true);
            OnEndDragEvent.Invoke();
        }

        private void UpdatePosition(Vector2 screenPos, bool snap)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt.parent as RectTransform,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );
            if (snap)
                localPoint = new Vector2(RoundSnap(localPoint.x), RoundSnap(localPoint.y));
            rt.anchoredPosition = localPoint;
        }

        private float RoundSnap(float value)
        {
            float half = snapSize * 0.5f;
            float mod = value % snapSize;
            float baseVal = value - mod;
            return (mod < half) ? baseVal : baseVal + snapSize;
        }

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
        }
    }
}
