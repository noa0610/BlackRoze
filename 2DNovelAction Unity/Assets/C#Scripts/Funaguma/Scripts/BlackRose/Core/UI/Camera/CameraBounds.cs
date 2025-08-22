using UnityEngine;

namespace BlackRose.Core
{
    public class CameraBounds : MonoBehaviour
    {
        [Header("Camera Limit")]
        [SerializeField] private Rect _bounds;
        public Vector2 Left => new(_bounds.xMin, 0);
        public Vector2 Right => new(_bounds.xMax, 0);
        public Vector2 Up => new(0, _bounds.yMax);
        public Vector2 Down => new(0, _bounds.yMin);

        public bool IsInBounds(Vector2 pos) => _bounds.Contains(pos);

        public bool IsWithinX(Vector2 pos)
        {
            return Left.x <= pos.x && pos.x <= Right.x;
        }

        public bool IsWithinY(Vector2 pos)
        {
            return Down.y <= pos.y && pos.y <= Up.y;
        }
    }
}
// unicode