using UnityEngine;
using System.Collections;

public class Blast : Weapon
{
    /// <summary>
    /// 種類
    /// </summary>
    public override Player.WeaponKind Kind => Player.WeaponKind.Bomb;

    /// <summary>
    /// 大きくなり終わるまでの時間
    /// </summary>
    [SerializeField]
    private float toBiggestSecond = 0.2f;

    /// <summary>
    /// 消えるまでの時間
    /// </summary>
    [SerializeField]
    private float toFadeEndSecond = 1.0f;

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviorTimer;

    /// <summary>
    /// 自分を持っている爆弾
    /// </summary>
    private Bomb bomb = null;

    /// <summary>
    /// 大きくなり終わったか否か
    /// </summary>
    private bool isBiggest = false;

    /// <summary>
    /// 目標の拡大率
    /// </summary>
    private Vector3 targetScale;

    // Update is called once per frame
    void Update()
    {
        if (isBiggest)
        {// 大きくなり終わっている場合
            // 挙動終了
            if (behaviorTimer.Advance())
            {// 完全透明になった場合
                if (bomb)
                {// 爆弾がある場合
                    bomb.ToUnused();
                }
                else
                {// ない場合
                    ToUnused();
                }
            }

            // 不透明度の更新
            Alpha = Easing.Quad.Out(1.0f - behaviorTimer.Ratio);
        }
        else
        {// 大きくなり終わっていない場合
            // 状態終了
            if (behaviorTimer.Advance())
            {// 大きくなり切った場合
                // 状態変更
                isBiggest = true;

                // 拡大率補正
                transform.localScale = targetScale;

                // タイマー設定
                behaviorTimer.Interval = toFadeEndSecond;
                behaviorTimer.Restart();

                // 強制終了
                return;
            }

            // 拡大率の更新
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, Easing.Quad.Out(behaviorTimer.Ratio));
        }
    }

    public override void OnGenerated()
    {
        base.OnGenerated();

        // タイマー設定
        behaviorTimer.Interval = toBiggestSecond;
        behaviorTimer.Restart();

        // 不透明度リセット
        Alpha = 1.0f;

        // 状態リセット
        isBiggest = false;
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();

        // 爆弾本体の取得
        if (transform.parent != null)
        {// 親がいる場合
            bomb = transform.parent.gameObject.GetComponent<Bomb>();
        }

        // 拡大率の保存
        targetScale = transform.localScale;
    }
}
