using System.Collections;
using UnityEngine;

public static partial class Extension
{
    /// <summary>
    /// 符号を逆にした場合の値
    /// </summary>
    public static float Reversed(this float self)
    {
        return -self;
    }

    /// <summary>
    /// 符号を逆にする
    /// </summary>
    public static float Reverse(this float self)
    {
        self *= -1.0f;
        return self;
    }

    /// <summary>
    /// 逆数
    /// </summary>
    public static float Inversed(this float self)
    {
        return 1.0f / self;
    }

    /// <summary>
    /// 逆数にする
    /// </summary>
    public static float Inverse(this float self)
    {
        self = self.Inversed();
        return self;
    }

    /// <summary>
    /// 範囲内に収める
    /// </summary>
    /// <param name="range">範囲</param>
    /// <returns>収めた後の値</returns>
    public static float Clamp(this float self, Limit range)
    {
        self = Mathf.Clamp(self, range.min, range.max);
        return self;
    }

    /// <summary>
    /// 半分にした値
    /// </summary>
    public static float Half(this float self)
    {
        return self * 0.5f;
    }

    /// <summary>
    /// 2倍の値
    /// </summary>
    public static float Double(this float self)
    {
        return self * 2.0f;
    }

    /// <summary>
    /// 絶対値にする
    /// </summary>
    /// <returns>処理後の値</returns>
    public static float Unsign(this float self)
    {
        self = Mathf.Abs(self);
        return self;
    }

    /// <summary>
    /// 絶対値
    /// </summary>
    public static float Abs(this float self)
    {
        return Mathf.Abs(self);
    }

    /// <summary>
    /// 引数乗した値
    /// </summary>
    public static float Powered(this float self, int exponent)
    {
        var result = 1.0f;
        for (var i = 0; i < exponent; ++i)
        {
            result *= self;
        }
        return result;
    }

    /// <summary>
    /// 引数乗する
    /// </summary>
    public static float Power(this float self, int exponent)
    {
        self = self.Powered(exponent);
        return self;
    }

    /// <summary>
    /// 引数乗した値
    /// </summary>
    public static float Powered(this float self, float exponent)
    {
        return Mathf.Pow(self, exponent);
    }

    /// <summary>
    /// 引数乗する
    /// </summary>
    public static float Power(this float self, float exponent)
    {
        self = self.Powered(exponent);
        return self;
    }

    public static float Exp(this float self)
    {
        return Mathf.Exp(self);
    }

    /// <summary>
    /// 自然対数
    /// </summary>
    public static float Log(this float self)
    {
        return Mathf.Log(self);
    }

    /// <summary>
    /// 常用対数
    /// </summary>
    public static float Log10(this float self)
    {
        return Mathf.Log10(self);
    }

    /// <summary>
    /// 平方根
    /// </summary>
    public static float Root(this float self)
    {
        return Mathf.Sqrt(self);
    }

    /// <summary>
    /// 小数点以下切り上げした値
    /// </summary>
    public static float Ceiled(this float self)
    {
        return Mathf.Ceil(self);
    }

    /// <summary>
    /// 小数点以下切り上げ
    /// </summary>
    public static float Ceil(this float self)
    {
        self = Mathf.Ceil(self);
        return self;
    }

    /// <summary>
    /// 小数点以下切り捨てした値
    /// </summary>
    public static float Floored(this float self)
    {
        return Mathf.Floor(self);
    }

    /// <summary>
    /// 小数点以下切り捨て
    /// </summary>
    public static float Floor(this float self)
    {
        self = Mathf.Floor(self);
        return self;
    }

    /// <summary>
    /// 四捨五入した値
    /// </summary>
    public static float Rounded(this float self)
    {
        return Mathf.Round(self);
    }

    /// <summary>
    /// 四捨五入
    /// </summary>
    public static float Round(this float self)
    {
        self = Mathf.Round(self);
        return self;
    }

    /// <summary>
    /// 現在の値にacosをかけた角度
    /// </summary>
    public static Radian Acos(this float self)
    {
        return new Radian(Mathf.Acos(self));
    }

    /// <summary>
    /// 現在の値にasinをかけた角度
    /// </summary>
    public static Radian Asin(this float self)
    {
        return new Radian(Mathf.Asin(self));
    }

    /// <summary>
    /// 現在の値にatanをかけた角度
    /// </summary>
    public static Radian Atan(this float self)
    {
        return new Radian(Mathf.Atan(self));
    }

    /// <summary>
    /// ラジアンに変換
    /// </summary>
    public static Radian ToRadian(this float self)
    {
        return new Radian(self);
    }

    /// <summary>
    /// 整数に変換
    /// </summary>
    public static int ToInt(this float self)
    {
        return (int)self;
    }

    /// <summary>
    /// 範囲内に値がある？
    /// </summary>
    /// <param name="range">範囲</param>
    /// <returns>あればtrue</returns>
    public static bool IsInRange(this float self, Limit range)
    {
        return (range.min <= self) && (self <= range.max);
    }

    /// <summary>
    /// 0か？
    /// </summary>
    public static bool IsZero(this float self)
    {
        return self <= Mathf.Epsilon;
    }

    /// <summary>
    /// 正の数？
    /// </summary>
    public static bool IsPlus(this float self)
    {
        return Mathf.Epsilon < self;
    }

    /// <summary>
    /// 負の数？
    /// </summary>
    public static bool IsMinus(this float self)
    {
        return -Mathf.Epsilon > self;
    }

    /// <summary>
    /// 整数？
    /// </summary>
    public static bool IsInteger(this float self)
    {
        var dif = (self - self.Floored()).Abs();
        return dif.IsZero();
    }

    /// <summary>
    /// 無限？
    /// </summary>
    public static bool IsInf(this float self)
    {
        return self == Mathf.Infinity;
    }

    /// <summary>
    /// マイナス無限？
    /// </summary>
    public static bool IsNegaInf(this float self)
    {
        return self == Mathf.NegativeInfinity;
    }
}