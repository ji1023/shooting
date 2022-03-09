using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageProgressUI : SingletonMonoBehaviour<StageProgressUI>
{
    [SerializeField]
    private Image playerIcon = null;

    [SerializeField]
    private Bar bar = null;

    /// <summary>
    /// 退場時の移動量
    /// </summary>
    [SerializeField]
    private float exitingMoveDistance = 50.0f;

    /// <summary>
    /// 退場にかける秒数
    /// </summary>
    [SerializeField]
    private float exitingDuration = 0.5f;

    /// <summary>
    /// 退場用タイマー
    /// </summary>
    private Timer exitingTimer;

    /// <summary>
    /// 初期座標
    /// </summary>
    private Vector3 initialPos;

    /// <summary>
    /// 退場終了座標
    /// </summary>
    private Vector3 exitingEndPos;

    /// <summary>
    /// 状態
    /// </summary>
    private enum State
    {
        Advancing,  // 進行中
        Exiting,    // 退場
        Wait,       // 待機
    }
    /// <summary>
    /// 状態
    /// </summary>
    private StateManager<State> states = new StateManager<State>();

    // Start is called before the first frame update
    void Start()
    {
        // 座標保存
        initialPos = transform.position;
        exitingEndPos = initialPos + transform.up * exitingMoveDistance;

        // 状態の登録
        var advancing = states.AddState(State.Advancing, Advance);
        advancing.AddTransition(State.Exiting, ()=>
        {
            return EnemyManager.Instance.BossClosenessRatio >= 1.0f;
        });
        advancing.onTransitioned = () => 
        {
            transform.position = initialPos;
        };
        var exit = states.AddState(State.Exiting, Exit);
        exit.AddTransition(State.Wait, ()=>
        {
            return exitingTimer.IsTermination;
        });
        exit.onTransitioned = () =>
        {
            transform.position = initialPos;
            exitingTimer.Restart(exitingDuration);
        };
        exit.onFinished = () =>
        {
            transform.position = exitingEndPos;
        };
        states.AddState(State.Wait, ()=> { });

        // 開始
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        states.Behave();
    }

    public void Restart()
    {
        states.NowStateCode = State.Advancing;
    }

    private void Exit()
    {
        exitingTimer.Advance();
        transform.position = Vector3.Lerp(initialPos, exitingEndPos, Easing.Quad.Out(exitingTimer.Ratio));
    }

    private void Advance()
    {
        var ratio = EnemyManager.Instance.BossClosenessRatio;
        bar.Ratio = ratio;
        playerIcon.transform.position = Vector3.Lerp(bar.BeginPos, bar.EndPos, ratio);
    }
}
