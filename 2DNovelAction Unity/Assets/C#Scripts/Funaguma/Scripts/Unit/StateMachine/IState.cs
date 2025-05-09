using System;
using System.Collections.Generic;

namespace BlackRose
{

    /// <summary>
    /// ��ԁi�X�e�[�g�j���Ƃ̃C���^�[�t�F�[�X�B
    /// �e�X�e�[�g�i�s���p�^�[���j�ɂ��̃C���^�[�t�F�[�X������������B
    /// �Ԃ�l��bool�͏����̐����^���s�������B
    /// </summary>
    public interface IState
    {
        bool Enter(IState previousState, IUnit parent);

        bool Stay(IUnit parent);

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
