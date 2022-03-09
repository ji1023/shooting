using UnityEngine;
using UnityEditor;

/// <summary>
/// 0から1の間にあることを保証する数値
/// </summary>
[System.Serializable]
public struct Ratio
{
    [SerializeField, Tooltip("割合"), Range(0.0f, 1.0f)]
    private float value;

    /// <summary>
    /// 数値
    /// </summary>
    public float Value => value;

    /// <summary>
    /// 逆の割合（1から引いた数値）
    /// </summary>
    public Ratio Reversed => 1.0f - value;

    public Ratio(float value)
    {
        this.value = Mathf.Clamp01(value);
    }

    public static Ratio operator+(Ratio ratio, float value)
    {
        ratio.value = Mathf.Clamp01(ratio.value + value);
        return ratio;
    }

    public static Ratio operator-(Ratio ratio, float value)
    {
        ratio.value = Mathf.Clamp01(ratio.value - value);
        return ratio;
    }

    public static implicit operator float(Ratio ratio)
    {
        return ratio.value;
    }

    public static implicit operator Ratio(float value)
    {
        return new Ratio(value);
    }
}