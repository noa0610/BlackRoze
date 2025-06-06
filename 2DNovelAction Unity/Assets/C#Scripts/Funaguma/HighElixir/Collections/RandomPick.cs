using System.Collections.Generic;
using UnityEngine;

namespace HighElixr
{
    public static class RandomPick
    {
        public static T PickFromList<T>(List<T> from)
        {
            return from[Random.Range(0, from.Count - 1)];
        }
    }
}