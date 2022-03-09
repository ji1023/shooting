using UnityEditor;
using UnityEngine;

/// <summary>
/// ラジアン
/// </summary>
public struct Radian
{
    public static Radian Min => new Radian(min);
    private const float min = 0.0f;
    public static Radian Mid => new Radian(mid);
    private const float mid = Mathf.PI;

    public static Radian Max => new Radian(max);
    private const float max = mid * 2.0f;

    /// <summary>
    /// 値
    /// </summary>
    public float Value { get; private set; }

    /// <summary>
    /// デグリー
    /// </summary>
    public Degree Degree => new Degree(Value * Mathf.Rad2Deg);

    /// <summary>
    /// sin(value)
    /// </summary>
    public float Sin => Mathf.Sin(Value);

    /// <summary>
    /// cos(value)
    /// </summary>
    public float Cos => Mathf.Cos(Value);

    /// <summary>
    /// tan(value)
    /// </summary>
    public float Tan => Mathf.Tan(Value);

    /// <summary>
    /// 1.0f / sin(value)
    /// </summary>
    public float Csc => 1.0f / Sin;

    /// <summary>
    /// 1.0f / cos(value)
    /// </summary>
    public float Sec => 1.0f / Cos;

    /// <summary>
    /// 1.0f / tan(value)
    /// </summary>
    public float Cot => 1.0f / Tan;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="angle">角度</param>
    public Radian(float angle = 0.0f)
    {
        this.Value = angle;
    }

    public static implicit operator float(Radian deg)
    {
        return deg.Value;
    }

    public static implicit operator Radian(float angle)
    {
        return new Radian(angle);
    }

    public static Radian operator +(Radian rad, float value)
    {
        rad.Value += value;
        rad.Fix();
        return rad;
    }

    public static Radian operator -(Radian rad, float value)
    {
        rad.Value -= value;
        rad.Fix();
        return rad;
    }

    public static Radian operator *(Radian rad, float value)
    {
        rad.Value *= value;
        rad.Fix();
        return rad;
    }

    public static Degree operator -(Radian rad)
    {
        return new Degree(-rad.Value);
    }

    private void Fix()
    {
        if (Value < min)
        {
            do
            {
                Value += max;
            } while (Value < min);
        }
        else if (Value >= max)
        {
            do
            {
                Value -= max;
            } while (Value >= min);
        }
    }
}