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
        [SerializeField]private SpriteLibrary _library;

        public void Change_N()
        {
            _library.spriteLibraryAsset = _normal;
        }
        public void Change_L()
        {
            _library.spriteLibraryAsset = _light;
        }
        public void Change_H()
        {
            _library.spriteLibraryAsset = _heavy;
        }
    }
}