using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    /// <summary>
    /// 無くなった部分
    /// </summary>
    [SerializeField]
    private Image frame = null;

    /// <summary>
    /// 元々の長さ
    /// </summary>
    private float defaultLength;

    /// <summary>
    /// 割合
    /// </summary>
    public float Ratio
    {
        set
        {
            var scale = transform.localScale;
            scale.x = value;
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// 開始座標
    /// </summary>
    public Vector3 BeginPos => frame.transform.position;

    /// <summary>
    /// 終端の座標
    /// </summary>
    public Vector3 EndPos
    {
        get
        {
            return frame.transform.position + transform.right * FrameLength;
        }
    }

    /// <summary>
    /// フレームの長さ
    /// </summary>
    public float FrameLength => frame.transform.lossyScale.x * frame.rectTransform.sizeDelta.x;

    /// <summary>
    /// 長さの倍率
    /// </summary>
    public float LengthScale
    {
        set
        {
            var scale = frame.transform.localScale;
            scale.x = defaultLength * value;
            frame.transform.localScale = scale;
        }
    }

    private void Start()
    {
        defaultLength = frame.transform.localScale.x;
    }
}
