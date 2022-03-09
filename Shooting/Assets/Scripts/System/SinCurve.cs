using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct SinCurve
{
    /// <summary>
    /// カーブの最小値・最大値
    /// </summary>
    [SerializeField]
    public Limit range;

    /// <summary>
    /// タイムスケールを無視するか否か
    /// </summary>
    public bool isUnscaled;

    private float counter;
    private float interval;
    private float invInterval;

    /// <summary>
    /// 周期（秒）
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
    /// 現在の数値
    /// </summary>
    public float Value { get; private set; }

    public SinCurve(float interval, Limit range)
    {
        isUnscaled = false;
        this.interval = interval;
        this.invInterval = 1.0f / interval;
        this.range = range;
        this.Value = range.Mid;
        this.counter = 0.0f;
    }

    public void Advance()
    {
        counter += isUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
        if (counter >= interval)
        {
            counter -= interval;
        }
        var ratio = counter * invInterval;
        Value = range.Lerp(Mathf.Sin(Mathf.PI * 2.0f * ratio) + 1.0f * 0.5f);
    }

    /// <summary>
    /// 最小値にする
    /// </summary>
    public void SetMin()
    {
        counter = (3.0f / 4.0f) * interval;
        Value = range.min;
    }

    /// <summary>
    /// 最大値にする
    /// </summary>
    public void SetMax()
    {
        counter = (1.0f / 4.0f) * interval;
        Value = range.max;
    }

    public void Reset()
    {
        counter = 0.0f;
        Value = range.Mid;
    }

    public static implicit operator float(SinCurve curve)
    {
        return curve.Value;
    }
}