using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
   void PerformAttack(Transform originPos, int attackData, Transform target = null);
}
