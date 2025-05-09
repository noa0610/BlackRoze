// �T�v:
// ���̖��O��ԁiTest�j�ł́A���j�b�g�̍i�荞�݁E�����@�\���\�z���邽�߂̃C���^�[�t�F�[�X�Q���`���Ă��܂��B
// �_��ȃt�B���^�[�\����A���������̊g������ړI�Ƃ����݌v�ł��B

using System.Collections.Generic;

namespace BlackRose
{
    // ISearch:
    // �t�B���^�[��ǉ��E�폜���A���j�b�g�̃��X�g�ɑ΂��Č������������s���邽�߂̃C���^�[�t�F�[�X�B
    // �������͕����̃t�B���^�[�iIFilterComponent�j���Ǘ����A�i�K�I�Ƀ��j�b�g���X�g���������܂��B
    public interface ISearch
    {
        // �t�B���^�[�̒ǉ��B�L�[�ƗD��x���w��\�i���L�[�Ȃ�㏑���z��j�B
        void AddComp(string key, IFilterComponent comp, int priority = 0);

        // �t�B���^�[�̍폜�B�w��L�[�̃R���|�[�l���g�������B
        void RemoveComp(string id);

        // �������������s�B�t�B���^�[���Ƀ��j�b�g���������A���ʂ�ԋp�B
        List<IUnit> Execute(List<IUnit> pool);
    }

    // IFilterComponent:
    // �C�ӂ̃��j�b�g���X�g�ɑ΂��āA����̏����Ńt�B���^�����O���s���R���|�[�l���g�p�̃C���^�[�t�F�[�X�B
    // �����ɂ��A�U���͂�HP�A�^�O�ȂǂɊ�Â������������\�B
    // �����K���FFilterBy����
    public interface IFilterComponent
    {
        List<IUnit> Execute(List<IUnit> pool);
    }
}
// unicode