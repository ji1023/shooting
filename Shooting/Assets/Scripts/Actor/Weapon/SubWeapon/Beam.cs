using UnityEngine;
using System.Collections;

public class Beam : SubWeapon
{
    /// <summary>
    /// 状態
    /// </summary>
    private class StateNames
    {
        /// <summary>
        /// 発射まで
        /// </summary>
        public const string ToAttacking = "ToAttacking";

        /// <summary>
        /// 発射中
        /// </summary>
        public const string Shotting = "Shotting";

        /// <summary>
        /// 消滅中
        /// </summary>
        public const string Fading = "Fading";
    }

    /// <summary>
    /// 種類
    /// </summary>
    public override Player.WeaponKind Kind => Player.WeaponKind.Beam;

    /// <summary>
    /// 撃つまでの秒数
    /// </summary>
    [SerializeField]
    private float toShottingSecond = 0.15f;

    /// <summary>
    /// 撃ち続ける秒数
    /// </summary>
    [SerializeField]
    private float toShotEndSecond = 0.5f;

    /// <summary>
    /// 消えるまでの秒数
    /// </summary>
    [SerializeField]
    private float toFadeOutSeocnd = 0.2f;

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviorTimer;

    /// <summary>
    /// 状態管理
    /// </summary>
    private StateManager<string> stateManager = new StateManager<string>();

    /// <summary>
    /// 最初の太さ
    /// </summary>
    [SerializeField]
    private float initialWidth = 0.01f;

    /// <summary>
    /// 発射中の最大の太さ
    /// </summary>
    [SerializeField]
    private Limit maxWidthRange = new Limit(0.6f, 1.0f);

    /// <summary>
    /// 溜め具合
    /// </summary>
    public override float ChargeRatio
    {
        set
        {
            targetScale.y = maxWidthRange.Lerp(value);
        }
    }

    /// <summary>
    /// 元々の拡大率
    /// </summary>
    private Vector3 defaultScale;

    /// <summary>
    /// 目標拡大率
    /// </summary>
    private Vector3 targetScale;

    /// <summary>
    /// 終了時の拡大率
    /// </summary>
    private Vector3 endScale;

    /// <summary>
    /// くっつくオブジェクト
    /// </summary>
    [System.NonSerialized]
    public GameObject clingObject = null;

    // Update is called once per frame
    void Update()
    {
        stateManager.Behave();

        // 追従
        transform.position = clingObject.transform.position;
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();

        // 拡大率の保存
        defaultScale = new Vector3(0.0f, initialWidth);
        targetScale = transform.localScale;
        endScale = targetScale;
        endScale.y = 0.0f;

        // 状態の追加
        // 発射まで
        var toShotting = stateManager.AddState(
            StateNames.ToAttacking, ()=> 
            {
                // 拡大率の更新
                behaviorTimer.Advance();
                transform.localScale = Vector3.Lerp(defaultScale, targetScale, behaviorTimer.Ratio);
            });
        toShotting.AddTransition(
            StateNames.Shotting, ()=> 
            {
                return behaviorTimer.IsTermination;
            });
        toShotting.onTransitioned = () =>
        {
            // タイマー設定
            behaviorTimer.Restart(toShottingSecond);

            // 拡大率設定
            transform.localScale = defaultScale;
        };
        toShotting.onFinished = () =>
        {
            transform.localScale = targetScale;
        };
        //
        // 発射中
        var shotting = stateManager.AddState(
            StateNames.Shotting, ()=>{ });
        shotting.AddTransition(
            StateNames.Fading, ()=>
            {
                return behaviorTimer.Advance();
            });
        shotting.onTransitioned = () =>
        {
            // 攻撃判定発生
            IsAttacking = true;

            // タイマー設定
            behaviorTimer.Restart(toShotEndSecond);
        };
        //
        // 発射後
        var fading = stateManager.AddState(
            StateNames.Fading, () =>
            {
                // 拡大率の更新
                behaviorTimer.Advance();
                transform.localScale = Vector3.Lerp(targetScale, endScale, behaviorTimer.Ratio);
            });
        fading.AddTransition(
            StateNames.ToAttacking, ()=>
            {
                return behaviorTimer.IsTermination;
            });
        fading.onTransitioned = () =>
        {
            // タイマー設定
            behaviorTimer.Restart(toFadeOutSeocnd);
        };
        fading.onFinished = () =>
        {
            // 拡大率の補正
            transform.localScale = endScale;

            // 未使用化
            ToUnused();
        };
    }

    public override void OnGenerated()
    {
        base.OnGenerated();
        targetScale.y = maxWidthRange.min;

        // 状態の設定
        stateManager.NowStateCode = StateNames.ToAttacking;
    }
}
