using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのクラス
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private CharacterParameter _parameter; // キャラクターのパラメータを取得
    private PlayerStateManager _stateManager;   // ステートマネージャーの取得　（ステート管理）
    private PlayerModeManager _modeManager;     // モードマネージャーの取得　　（モード管理）
    private ChargeSystem _chargeSystem;         // チャージシステムの取得　　　（チャージ管理）
    private HealthController _healthController; // ヘルスコントローラーの取得　（HP管理）

    public int Attack => _parameter.Attack; // キャラクターの攻撃力を取得
    public float MoveSpeedMultiplef => _parameter.MoveSpeedMultiplef; // キャラクターの移動速度倍率を取得
    
    /// <summary>
    /// 移動のクラス
    /// </summary>
    public PlayerMove Move {get; private set;}

    /// <summary>
    /// アニメーションのクラス
    /// </summary>
    public CharacterAnime Anime {get; private set;}

    /// <summary>
    /// カメラのクラス
    /// </summary>
    // public PlayerCamera Camera {get; private set;}
    

    void Awake()
    {
        // "PlayerMove"と"CharacterAnime"をオブジェクトに追加
        Move = gameObject.AddComponent<PlayerMove>();      
        Anime = gameObject.AddComponent<CharacterAnime>();

        // "PlayerStateManager"を取得(ピュアクラスなのでAddComponentは使えない)
        _stateManager = new PlayerStateManager();
        _stateManager.Init(this);

        // "PlayerModeManager"を取得
        _modeManager = new PlayerModeManager();
        _modeManager.InitMode(this);

        // "ChargeSystem"を取得
        _chargeSystem = new ChargeSystem();

        // "HealthController"を取得
        _healthController = new HealthController();
        _healthController.Init(_parameter.MaxHP, _parameter.DamageMultiplef); // ヒットポイントを初期化
    }

    /// <summary>
    /// 現在のステートの処理を継続する
    /// </summary>
    /// <param name="inputInfo"></param>
    public void StateExecute(InputInfo inputInfo)
    {
        _stateManager.CurrentState.Execute(this, inputInfo);
    }
    
    /// <summary>
    /// 現在のステートから次のステートへの切り替えを行う
    /// </summary>
    /// <param name="fromState"></param>
    /// <param name="nextState"></param>
    public void StateTransition(IPlayerState fromState, IPlayerState nextState)
    {
        _stateManager.Transition(this, fromState, nextState);
    }

    /// <summary>
    /// モードの処理を継続させる
    /// </summary>
    public void ModeExecute(InputInfo inputInfo)
    {
        _modeManager.CurrentMode.ExecuteMode(this, inputInfo);
    }

    /// <summary>
    /// 現在のモードから次のモードへの切り替えを行う
    /// </summary>
    /// <param name="fromState"></param>
    /// <param name="nextState"></param>
    public void ModeTransition(IPlayerMode fromMode, IPlayerMode nextMode)
    {
        _modeManager.TransitionMode(this, fromMode, nextMode);
    }

    /// <summary>
    /// 死亡したときの処理
    /// </summary>
    public void PlayerDeadHealth()
    {
        // _stateManager.CurrentState.Exit(this); // 現在のステートが終了するときの処理を行う
        // _stateManager.CurrentState = new PlayerDeadState(); // 死亡ステートに移行する
    }
}