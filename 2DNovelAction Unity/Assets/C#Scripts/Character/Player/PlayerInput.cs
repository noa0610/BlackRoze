using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 入力状況を渡すスクリプト
/// </summary>
public class PlayerInput: MonoBehaviour
{
    private InputInfo _inputInfo;
    private Player _player;

    private void Awake()
    {
        _inputInfo = new InputInfo();     // InputInfo取得
        _player = GetComponent<Player>(); // Playerコンポーネント取得
    }

    private void Update()
    {
        _player.StateExecute(_inputInfo); // 入力の状況を渡す
        _player.ModeExecute(_inputInfo);
        
        // Debug.Log($"Move{_inputInfo.Move}");
        // Debug.Log($"Attack:{_inputInfo.Attack}");
        // Debug.Log($"Jump:{_inputInfo.Jump}");
        // Debug.Log($"Charge:{_inputInfo.Charge}");

        InputClear(); // 入力のクリア
        
    }

    private void OnMove(InputValue value) // 移動入力受け取り
    {
        var input = value.Get<Vector2>();
        _inputInfo.Move = new Vector3(input.x, 0, 0);
    }

    private void OnJump() // ジャンプ入力受け取り
    {
        _inputInfo.Jump = true;
    }

    private void OnAttack() // 攻撃入力受け取り
    {
        _inputInfo.Attack = true;
    }

    private void OnSkill() // スキル入力受け取り
    {
        _inputInfo.Skill = true;
    }

    private void OnChange() // 変身入力受け取り
    {
        _inputInfo.Change = true;
    }

    private void OnSpecial() // スペシャル入力受け取り
    {
        _inputInfo.Special = true;
    }

    private void InputClear()
    {
        _inputInfo.Jump = false;
        _inputInfo.Attack = false;
        _inputInfo.Skill = false;
        _inputInfo.Change = false;
        _inputInfo.Special = false;
    }
}