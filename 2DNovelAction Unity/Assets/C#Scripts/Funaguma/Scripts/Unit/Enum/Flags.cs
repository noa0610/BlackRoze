using System;

namespace BlackRose
{
    // ��Ԃ��r�b�g�t���O�ŕ\���񋓌^�i������Ԃ����Ă�悤��[Flags]�t���j
    [Flags]
    public enum StateFlags
    {
        None = 0,            // ��ԂȂ�
        InMove = 1 << 0,     // �ړ���
        InFall = 1 << 1,     // ������
        InShoot = 1 << 2,    // �ˌ���
        InJump = 1 << 3,     // �W�����v
        // �����ƃr�b�g�𑝂₹�Ε����̃t���O�Ǘ��ł���I
    }
}