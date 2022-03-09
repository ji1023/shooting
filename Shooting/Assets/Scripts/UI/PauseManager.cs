using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : SingletonMonoBehaviour<PauseManager>
{
    /// <summary>
    /// ポーズ画面に移行するボタンの種類
    /// </summary>
    [SerializeField]
    private KeyCode button = KeyCode.RightShift;

    /// <summary>
    /// 挙動に要する秒数
    /// </summary>
    [SerializeField]
    private float behaviourSecond = 0.5f;

    /// <summary>
    /// ポーズ中か否か
    /// </summary>
    private bool isPausing = false;

    /// <summary>
    /// ポーズされたか否か
    /// </summary>
    public bool IsPaused { get; private set; } = false;

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviourTimer;

    /// <summary>
    /// ポーズ画面に出現するUI一覧
    /// </summary>
    [SerializeField]
    private List<MovingUI> uis = new List<MovingUI>();

    // Start is called before the first frame update
    void Start()
    {
        behaviourTimer.isUnscaled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (behaviourTimer.IsCounting)
        {// 演出中の場合
            if (behaviourTimer.Advance())
            {// 演出が終わった場合
                if (isPausing)
                {// ポーズ中の場合
                    End();
                }
                else
                {// ポーズ中でない場合
                    // ポーズ中にする
                    isPausing = true;
                }
            }
        }
        else
        {// 演出中でない場合
            // 死亡後はポーズ不可
            if (Player.Instance.IsDead) { return; }

            // ポーズ切り替え
            if (Input.GetKeyDown(button))
            {// キー押下
                if (isPausing)
                {// ポーズ中の場合
                    // ゲーム画面復活
                    Fade.Instance.LowerBlackBoard.FadeIn(behaviourSecond);

                    // UIの移動
                    foreach (var ui in uis)
                    {
                        ui.EndToStart(behaviourSecond);
                    }

                    // メニュー退場
                    Gameover.Instance.Exit();
                }
                else
                {// ポーズ中でない場合
                    // 更新停止
                    Time.timeScale = 0.0f;

                    // ゲーム画面暗転
                    var fade = Fade.Instance;
                    fade.LowerBlackBoard.FadeOut(behaviourSecond, fade.LowerBoardDarkness);

                    // UIの移動
                    foreach (var ui in uis)
                    {
                        ui.StartToEnd(behaviourSecond);
                    }

                    // ポーズされた
                    IsPaused = true;

                    // メニュー表示
                    Gameover.Instance.Call(Gameover.MenuType.Pause);
                }
                behaviourTimer.Restart(behaviourSecond);
            }
        }
    }

    public void End()
    {
        // 更新再開
        Time.timeScale = 1.0f;

        // ポーズ解除
        isPausing = false;
        IsPaused = false;
    }
}
