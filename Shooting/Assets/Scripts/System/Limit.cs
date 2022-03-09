using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Limit
{
    /// <summary>
    /// 最小値
    /// </summary>
    [SerializeField]
    public float min;

    /// <summary>
    /// 最大値
    /// </summary>
    [SerializeField]
    public float max;

    /// <summary>
    /// 最小値から最大値までの範囲
    /// </summary>
    public float Range
    {
        get { return max - min; }
    }

    /// <summary>
    /// 最小値から最大値までの乱数
    /// </summary>
    public float Rand
    {
        get
        {
            return Random.Range(min, max);
        }
    }

    /// <summary>
    /// 最小値（切り捨て）から最大値（切り捨て）までの乱数（整数）
    /// </summary>
    public int RandInt
    {
        get
        {
            return Usual.RandomInt(Mathf.FloorToInt(min), Mathf.FloorToInt(max));
        }
    }

    /// <summary>
    /// 最小値と最大値の中間の値
    /// </summary>
    public float Mid
    {
        get
        {
            return Lerp(0.5f);
        }
    }
    
    /// <summary>
    /// 最小値0、最大値を任意の数値で初期化
    /// </summary>
    /// <param name="max">最大値</param>
    public Limit(float max)
    {
        this.min = 0.0f;
        this.max = max;
    }
    /// <summary>
    /// 最小値、最大値を任意の数値で初期化
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    public Limit(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// 任意の数値が最小値を0、最大値を1としてどこに位置するか
    /// </summary>
    /// <param name="value">任意の数値</param>
    /// <returns>例 min:0 max:100 value:50 → 0.5</returns>
    public float GetRatio(float value)
    {
        return (value - min) / Range;
    }

    /// <summary>
    /// 任意の割合が最小値を0、最大値を1としてどの数値になるか
    /// </summary>
    /// <param name="ratio">任意の割合</param>
    /// <returns>例 min:0 max:100 ratio:0.5 → 50</returns>
    public float Lerp(float ratio)
    {
        return min + Range * ratio;
    }
    
    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }

    /// <summary>
    /// 最小値と最大値を入れ替える
    /// </summary>
    public void Swap()
    {
        var tmp = min;
        min = max;
        max = tmp;
    }
}
