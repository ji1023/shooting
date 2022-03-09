using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using TMPro;

public static class Usual
{
    /// <summary>
    /// 最大値が全く出ない問題を解消した整数の乱数
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <returns></returns>
    public static int RandomInt(int min, int max)
    {
        var result = Random.Range(min, max + 1);
        if (result > max) { result = max; }
        return result;
    }

    /// <summary>
    /// <para>配列構造のコレクションからランダムで要素を選ぶ</para>
    /// <para>0(1)</para>
    /// </summary>
    public static T RandomElem<T>(List<T> list)
    {
        var index = RandomInt(0, list.Count - 1);
        return list[index];
    }

    public static int RandomIndex<T>(List<T> list)
    {
        return RandomInt(0, list.Count - 1);
    }

    /// <summary>
    /// <para>リスト構造のコレクションからランダムでノードを選ぶ</para>
    /// <para>O(n)</para>
    /// </summary>
    public static LinkedListNode<T> RandomNode<T>(LinkedList<T> list)
    {
        // 目的の要素の番号をランダムで出す
        var index = RandomInt(0, list.Count - 1);

        // 目標ノードまで移動
        var elem = list.First;
        for (int i = 0; i < index; ++i)
        {
            elem = elem.Next;
        }

        // 要素を返す
        return elem;
    }

    /// <summary>
    /// <para>リスト構造のコレクションからランダムで要素を選ぶ</para>
    /// <para>O(n)</para>
    /// </summary>
    public static T RandomElem<T>(LinkedList<T> list)
    {
        return RandomNode(list).Value;
    }

    public static class UI
    {
        public static void ChangeAlpha(Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        public static void ChangeAlpha(TextMeshProUGUI tmPro, float alpha)
        {
            var color = tmPro.color;
            color.a = alpha;
            tmPro.color = color;
        }
    }
}