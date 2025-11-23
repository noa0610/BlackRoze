using UnityEngine;
using UnityEngine.U2D.Animation;

namespace BlackRose.Core.Models.Units
{
    public class AISpriteResolver : MonoBehaviour
    {
        [Header("Sprite")]
        [SerializeField] private SpriteLibraryAsset _normal;
        [SerializeField] private SpriteLibraryAsset _heavy;
        [SerializeField] private SpriteLibraryAsset _light;
        [SerializeField] private SpriteLibrary _library;
        [SerializeField] private Gradient _normalColor;
        [SerializeField] private Gradient _heavyColor;
        [SerializeField] private Gradient _lightColor;

        private AIController _controller;

        public void Change_N()
        {
            _library.spriteLibraryAsset = _normal;
            _controller.TrailRenderer.colorGradient = _normalColor;
        }
        public void Change_L()
        {
            _library.spriteLibraryAsset = _light;
            _controller.TrailRenderer.colorGradient = _lightColor;
        }
        public void Change_H()
        {
            _library.spriteLibraryAsset = _heavy;
            _controller.TrailRenderer.colorGradient = _heavyColor;
        }

        private void Awake()
        {
            _controller = GetComponent<AIController>();
        }
    }
}