using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    /// <summary>
    /// 不透明度
    /// </summary>
    public float Alpha
    {
        get
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }
            return image.color.a;
        }
        set
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }
            var color = image.color;
            color.a = value;
            image.color = color;
        }
    }
    private Image image = null;

    /// <summary>
    /// フェード終了までの時間
    /// </summary>
    private Timer toFadeEnd;

    /// <summary>
    /// 不透明度の開始値、終了値
    /// </summary>
    private Limit alphaRange;

    /// <summary>
    /// <para>フェードが終わった直後の処理</para>
    /// <para>処理が終わると登録が解除される</para>
    /// </summary>
    public System.Action onFadeFinished = null;

    private void Start()
    {
        image = GetComponent<Image>();
        toFadeEnd.isUnscaled = true;
    }

    private void Update()
    {
        // フェードしていない場合は無視
        if (!toFadeEnd.IsCounting) { return; }

        // 不透明度の更新
        toFadeEnd.Advance();
        Alpha = alphaRange.Lerp(toFadeEnd.Ratio);

        // 不透明度の調整
        if (toFadeEnd.IsTermination)
        {// 計測が終わった場合
            // 不透明度の補正
            Alpha = alphaRange.max;

            // タイミング処理
            var lastProc = onFadeFinished;
            onFadeFinished?.Invoke();
            if (lastProc == onFadeFinished)
            {// タイミング処理内でタイミング処理の内容が書き換わっていない場合
                onFadeFinished = null;
            }
        }
    }

    /// <summary>
    /// ゲーム画面を見えなくする
    /// </summary>
    /// <param name="second">フェードに要する秒数</param>
    /// <param name="alphaMax">最終的な不透明度</param>
    public void FadeOut(float second, float alphaMax = 1.0f)
    {
        toFadeEnd.Restart(second);
        alphaRange.min = Alpha;
        alphaRange.max = alphaMax;
    }

    /// <summary>
    /// ゲーム画面が見えるようにする
    /// </summary>
    /// <param name="second">フェードに要する秒数</param>
    public void FadeIn(float second)
    {
        toFadeEnd.Restart(second);
        alphaRange.min = Alpha;
        alphaRange.max = 0.0f;
    }
}
