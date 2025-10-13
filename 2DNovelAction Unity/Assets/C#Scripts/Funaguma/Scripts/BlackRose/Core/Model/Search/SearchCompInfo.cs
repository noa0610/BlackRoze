using System;
using UnityEngine;
namespace BlackRose.Core.Models.SearchSystems
{
    [Serializable]
    public class SearchCompInfo
    {
        public string id;
        [SerializeReference, SubclassSelector]
        public IFilterComponent Comp;
        public int Priority;
    }
}