using System.Collections;
using UnityEngine;

public static partial class Extension
{
    /// <summary>
    /// 符号なし整数にする
    /// </summary>
    /// <remarks>
    /// 負の数なら0になる
    /// </remarks>
    public static uint ToUint(this int self)
    {
        if (self < 0) { self = 0; }
        return (uint)self;
    }

    /// <summary>
    /// 今の値までの階乗
    /// </summary>
    public static uint Factorial(this uint self)
    {
        var result = 1u;
        for (var i = 2u; i <= self; ++i)
        {
            result *= i;
        }
        return result;
    }

    /// <summary>
    /// 最小桁数
    /// </summary>
    public static uint Digit(this uint self)
    {
        // 0は無視
        if (self == 0) { return 1u; }

        // 桁数数える
        var digit = 0u;
        while (self > 0u)
        {
            ++digit;
            self /= 10u;
        }
        return digit;
    }

    /// <summary>
    /// 今の数値は引数の倍数？
    /// </summary>
    public static bool IsMultiple(this uint self, uint value)
    {
        return (self % value) == 0u;
    }

    /// <summary>
    /// 奇数？
    /// </summary>
    public static bool IsOdd(this int self)
    {
        return (self & 0b1) == 0b1;
    }

    /// <summary>
    /// 偶数？
    /// </summary>
    public static bool IsEven(this int self)
    {
        return (self & 0b1) == 0b0;
    }

    /// <summary>
    /// 奇数？
    /// </summary>
    public static bool IsOdd(this uint self)
    {
        return (self & 0b1u) == 0b1u;
    }

    /// <summary>
    /// 偶数？
    /// </summary>
    public static bool IsEven(this uint self)
    {
        return (self & 0b1u) == 0b0u;
    }
}