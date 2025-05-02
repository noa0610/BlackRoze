using System;
using System.Collections.Generic;

namespace Test
{

    /// <summary>
    /// ��ԁi�X�e�[�g�j���Ƃ̃C���^�[�t�F�[�X�B
    /// �e�X�e�[�g�i�s���p�^�[���j�ɂ��̃C���^�[�t�F�[�X������������B
    /// �Ԃ�l��bool�͏����̐����^���s�������B
    /// </summary>
    public interface IState
    {
        // �X�e�[�g�ɓ������Ƃ��̏����i���s������false��Ԃ��ăG���[�n���h�����O�ցj
        bool Enter(IState previousState, IUnit parent);

        // �X�e�[�g���ɖ��t���[���Ă΂�鏈���ifalse�Ȃ烍�O�o�����Ǒ��s�j
        bool Stay(IUnit parent);

        // �X�e�[�g���甲����Ƃ��̏����ifalse�ł����O�o���đ�����j
        bool Exit(IState nextState, IUnit parent);
    }

    // �X�e�[�g�}�V���{�̂̃C���^�[�t�F�[�X
    public interface IStateMachine
    {
        Dictionary<string, IState> StateMap { get; }  // �X�e�[�g�̈ꗗ�i���O�ƑΉ�����C���X�^���X�j
        Tuple<string, IState> CurrentState { get; }                  // ���݂̃X�e�[�g

        string DefaultStateKey { get; }

        void SetCondition(Func<string> condition);
        void ChangeState(string newStateKey);         // �����X�e�[�g�ύX
        void ChangeRequest(string requestKey);        // �X�e�[�g�ύX�̗\�� or �����t���ύX
        void Update();                                // �X�e�[�g�}�V���̍X�V�����iStay�̌Ăяo���Ȃǁj
        void AddState(string newStateKey, IState state);  // �X�e�[�g�̒ǉ�
    }
}
