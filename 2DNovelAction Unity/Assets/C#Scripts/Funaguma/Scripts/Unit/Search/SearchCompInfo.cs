using System;
using UnityEngine;
namespace BlackRose
{
    [Serializable]
    public class SearchCompInfo
    {
        public string Key;
        [SerializeReference, SubclassSelector]
        public IFilterComponent Comp;
        public int Priority;
    }
}