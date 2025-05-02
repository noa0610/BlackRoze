using System;
using UnityEngine;

namespace Test
{
    // ���j�b�g�̏�Ԃ�\���\���́iSerializable�Ȃ̂�Unity��Inspector�ł�������I�j
    [Serializable]
    public struct UnitStatus
    {
        public int id;
        public string name;
        public string description;
        public float hp;         // �̗�
        public Vector2 direction; // �����i2D�x�N�g���j
        public float speed;      // �ړ����x
        public float speedInAir;
        public float jumpPower;
        public UnitTags tags;
    }

    // �e�̏�Ԃ�\���\����
    [Serializable]
    public struct BulletStatus
    {
        public float hp;          // �e�̑ϋv�l�i�G�̒e������Ƃ��H�j
        public float time;        // ���ݎ��ԁi�����j
        public float damage;      // �_���[�W��
        public float speed;       // �e��
        public Vector2 direction; // ��ԕ���
    }
}