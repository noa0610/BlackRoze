using UnityEngine;
namespace BlackRose
{
    [System.Serializable]
    public class SearchCompInfo
    {
        public string Key;
        [SerializeReference,SubclassSelector]
        public IFilterComponent Comp;
        public int Priority;
    }
}