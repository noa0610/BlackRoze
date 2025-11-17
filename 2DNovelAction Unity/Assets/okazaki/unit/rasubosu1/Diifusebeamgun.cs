using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Objects;
using System.Collections;


namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]

    public partial class Diifusebeamgun : UnitBase
    {
        [Tooltip("同時シュートする際の最大角度")]
        [SerializeField, Min(0)] private float _range;
        [Tooltip("同時に発射する弾数")]
        [SerializeField, Min(1)] private int _shootCount;
        [SerializeField] private MultiShoot _multiShoot;

        // Start is called before the first frame update
    }
}
