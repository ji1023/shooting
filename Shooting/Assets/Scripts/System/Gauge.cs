using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Gauge
{
    /// <summary>
    /// 限界値
    /// </summary>
    [SerializeField]
    public Limit limit;

    /// <summary>
    /// 現在値
    /// </summary>
    [SerializeField]
    public float value;

    /// <summary>
    /// 割合
    /// </summary>
    public float Ratio
    {
        get
        {
            return limit.GetRatio(value);
        }
    }

    public Gauge(Limit limit, float value = 0.0f)
    {
        this.limit = limit;
        this.value = value;
    }

    public Gauge(float max, float min = 0.0f, float value = 0.0f)
    {
        limit.min = min;
        limit.max = max;
        this.value = value;
    }

    /// <summary>
    /// ランダムな数値をvalueに設定する
    /// </summary>
    public void SetRandom()
    {
        value = limit.Rand;
    }

    /// <summary>
    /// 最小値を設定する
    /// </summary>
    public void SetMin()
    {
        value = limit.min;
    }

    /// <summary>
    /// 最大値を設定する
    /// </summary>
    public void SetMax()
    {
        value = limit.max;
    }

    public static Gauge operator +(Gauge gauge, float other)
    {
        gauge.value = Mathf.Clamp(gauge.value + other, gauge.limit.min, gauge.limit.max);
        return gauge;
    }

    public static Gauge operator -(Gauge gauge, float other)
    {
        gauge.value = Mathf.Clamp(gauge.value - other, gauge.limit.min, gauge.limit.max);
        return gauge;
    }

    public static Gauge operator *(Gauge gauge, float other)
    {
        gauge.value = Mathf.Clamp(gauge.value * other, gauge.limit.min, gauge.limit.max);
        return gauge;
    }

    public static implicit operator Gauge(Limit limit)
    {
        return new Gauge(limit);
    }

    public static implicit operator float(Gauge gauge)
    {
        return gauge.value;
    }
}
