using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.Helper 
{
    public static class MoveHelpers
    {
        public static void CalcVerocity(UnitBase unit, Rigidbody2D rigidbody, float moveDir, Status status, float delta, float accel = 20f, float friction = 10f)
        {
            if (rigidbody == null) return;

            var input = Normalize(moveDir); 
            if (input == 0) return;
            float desiredDir = Mathf.Sign(input);
            float absInput = Mathf.Abs(input);

            float maxSpeed = unit.StatusManager.ReadValue(status);
            float targetVx = desiredDir * maxSpeed;

            // 目標Vxへ滑らかに近づける
            float newVx = absInput > 0.0001f ?
                Mathf.MoveTowards(rigidbody.velocity.x, targetVx, accel * delta) :
                Mathf.MoveTowards(rigidbody.velocity.x, 0f, friction * delta);

            // 縦速度は保持、横だけ更新
            rigidbody.velocity = new Vector2(newVx, rigidbody.velocity.y);
        }

        private static int Normalize(float i)
        {
            if (i > 1f) return 1;
            if (i < 0f) return -1;
            return 0;
        }
    }
}
