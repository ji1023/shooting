using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Timer
{
    /// <summary>
    /// タイムスケールを無視するか否か
    /// </summary>
    [SerializeField, Tooltip("タイムスケールを無視するか否か")]
    public bool isUnscaled;

    /// <summary>
    /// 計測する秒数
    /// </summary>
    [SerializeField, Tooltip("計測する秒数")]
    private float interval;

    /// <summary>
    /// 計測する秒数の逆数
    /// </summary>
    private float invInterval;
    /// <summary>
    /// 計測する秒数
    /// </summary>
    public float Interval
    {
        get
        {
            return interval;
        }
        set
        {
            interval = value;
            invInterval = 1.0f / value;
        }
    }

    /// <summary>
    /// 計測が完了しているか否か
    /// </summary>
    public bool IsTermination { get; private set; }

    /// <summary>
    /// 0秒を0、計測秒数を1とした割合
    /// </summary>
    public float Ratio { get; private set; }

    /// <summary>
    /// 計測中か否か
    /// </summary>
    public bool IsCounting { get; private set; }

    /// <summary>
    /// 計測を繰り返すか否か
    /// </summary>
    [SerializeField, Tooltip("計測を繰り返すか否か")]
    public bool isLooping;

    /// <summary>
    /// 計測秒数に達した後に秒数をリセットするか否か（非ループ時のみ）
    /// </summary>
    [SerializeField, Tooltip("計測秒数に達した後に秒数をリセットするか否か（非ループ時のみ）")]
    public bool isReset;

    /// <summary>
    /// 計測した秒数
    /// </summary>
    private float countedSecond;
    
    /// <param name="isLooping">計測を繰り返すか否か</param>
    /// <param name="isInitialCount">最初から計測状態にするか否か</param>
    public Timer(bool isLooping = false, bool isInitialCount = false)
    {
        this.isLooping = isLooping;
        IsCounting = isInitialCount;
        IsTermination = isReset = isUnscaled = false;
        interval    = 
        invInterval =
        Ratio                = 
        countedSecond        = 0.0f;
    }

    /// <summary>
    /// 計測開始
    /// </summary>
    public void Start()
    {
        IsCounting = true;
        IsTermination = false;
    }

    /// <summary>
    /// 計測中断
    /// </summary>
    public void Stop()
    {
        IsCounting = false;
    }

    /// <summary>
    /// 計測秒数を0に戻す
    /// </summary>
    /// <param name="isStop">同時に停止するか否か</param>
    public void Reset(bool isStop = false)
    {
        IsCounting = !isStop;
        Ratio = countedSecond = 0.0f;
        IsTermination = false;
    }

    /// <summary>
    /// 計測秒数を0に戻して計測開始
    /// </summary>
    public void Restart()
    {
        Ratio = countedSecond = 0.0f;
        IsCounting = true;
        IsTermination = false;
    }

    /// <summary>
    /// 目標の秒数を変更し、計測秒数を0に戻して計測開始
    /// </summary>
    public void Restart(float interval)
    {
        Interval = interval;
        Ratio = countedSecond = 0.0f;
        IsCounting = true;
        IsTermination = false;
    }

    /// <summary>
    /// 計測を進める
    /// </summary>
    /// <returns>指定した秒数が経過したらtrue</returns>
    public bool Advance()
    {
        // 未計測は無視
        if (!IsCounting) { return false; }

        // 秒数カウント
        countedSecond += isUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
        Ratio = countedSecond * invInterval;   // 計測秒数 / 目標秒数

        // 計測完了判定
        if (countedSecond >= interval)
        {// 指定された秒数だけカウントした場合
            // リセット
            if (isLooping)
            {// 計測を繰り返す場合
                countedSecond -= interval;
                Ratio = 1.0f;
            }
            else
            {// 繰り返さない場合
                // リセット
                if (isReset)
                {// リセットする場合
                    countedSecond = 0.0f;
                    Ratio = 0.0f;
                }

                // 計測完了状態にする
                else
                {// リセットしない場合
                    IsTermination = true;
                    Ratio = 1.0f;
                }

                // 計測終了
                IsCounting = false;
            }
            return true;    // 満了
        }
        return false;   // 満了せず
    }
}