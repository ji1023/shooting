using System.Collections;
using UnityEngine;

public static partial class Extension
{
    /// <summary>
    /// trueにする
    /// </summary>
    public static bool On(this bool self)
    {
        self = true;
        return self;
    }

    /// <summary>
    /// falseにする
    /// </summary>
    public static bool Off(this bool self)
    {
        self = false;
        return self;
    }

    /// <summary>
    /// フラグを反転
    /// </summary>
    /// <returns>反転後のフラグ</returns>
    public static bool Flip(this bool self)
    {
        self ^= true;
        return self;
    }
}