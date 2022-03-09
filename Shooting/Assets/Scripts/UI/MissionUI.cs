using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MissionUI : PoolableObject
{
    [SerializeField]
    private Image coloredFrame = null;
    /// <summary>
    /// 進行率
    /// </summary>
    public Ratio Ratio
    {
        set
        {
            coloredFrame.fillAmount = value;
        }
    }

    [SerializeField]
    private Image weaponIcon = null;
    /// <summary>
    /// 報酬のアイコン
    /// </summary>
    public Image RewardIcon => weaponIcon;

    [SerializeField]
    private Image targetIcon = null;
    /// <summary>
    /// 目標のアイコン
    /// </summary>
    public Sprite TargetIcon
    {
        get => targetIcon.sprite;
        set
        {
            targetIcon.sprite = value;
        }
    }

    [SerializeField]
    private TextMeshProUGUI count = null;
    /// <summary>
    /// 表示する文章
    /// </summary>
    public uint Count
    {
        set
        {
            count.text = value.ToString();
        }
    }

    /// <summary>
    /// 横幅
    /// </summary>
    public float Width => image.rectTransform.sizeDelta.x * image.rectTransform.localScale.x;

    /// <summary>
    /// 登場時の移動距離
    /// </summary>
    [SerializeField]
    private float approachingDistance = 100.0f;
    /// <summary>
    /// 登場にかける秒数
    /// </summary>
    [SerializeField]
    private float approachingDuration = 0.3f;
    /// <summary>
    /// 登場時の移動方向
    /// </summary>
    [SerializeField]
    private Vector3 approachingDirection = Vector3.up;
    /// <summary>
    /// 登場開始座標
    /// </summary>
    private Vector3 approachBeginPos;

    /// <summary>
    /// 退場時の移動距離
    /// </summary>
    [SerializeField]
    private float exitingDistance = 200.0f;
    /// <summary>
    /// 退場にかける秒数
    /// </summary>
    [SerializeField]
    private float exitDuration = 0.5f;
    /// <summary>
    /// 退場時の移動方向
    /// </summary>
    [SerializeField]
    private Vector3 exitingDirection = Vector3.down;
    /// <summary>
    /// 退場終了座標
    /// </summary>
    private Vector3 exitingEndPos;

    /// <summary>
    /// 停止する座標
    /// </summary>
    private Vector3 stayPos;
    /// <summary>
    /// 停止する座標
    /// </summary>
    public Vector3 StayPos
    {
        get => stayPos;
        set
        {
            stayPos = value;
            approachBeginPos = stayPos + -approachingDirection * approachingDistance;
            exitingEndPos    = stayPos +  exitingDirection     * exitingDistance;
        }
    }

    /// <summary>
    /// 移動始めの座標
    /// </summary>
    private Vector3 moveBeginPos;

    /// <summary>
    /// 状態
    /// </summary>
    private enum State
    {
        Approach,
        Stay,
        Exit,
        Move,
    }
    /// <summary>
    /// 状態
    /// </summary>
    private StateManager<State> states = new StateManager<State>();

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviourTimer;

    /// <summary>
    /// イメージ
    /// </summary>
    private Image image = null;

    /// <summary>
    /// アイコンの移動開始ワールド座標
    /// </summary>
    private Vector3 iconBeginWorldPos;

    /// <summary>
    /// アイコンの移動開始ローカル座標
    /// </summary>
    private Vector3 iconBeginLocalPos;

    /// <summary>
    /// アイコンの移動開始ワールド拡大率
    /// </summary>
    private Vector3 iconBeginLossyScale;

    /// <summary>
    /// アイコンの移動開始ローカル拡大率
    /// </summary>
    private Vector3 iconBeginLocalScale;

    /// <summary>
    /// アイコンの移動終了座標
    /// </summary>
    [System.NonSerialized]
    public GameObject iconTarget = null;

    /// <summary>
    /// 退場終了時に呼び出す処理
    /// </summary>
    public System.Action onExited = null;

    // Use this for initialization
    public override void OnInstantiated()
    {
        // イメージの取得
        image = GetComponent<Image>();
        
        // 座標を求める
        StayPos = transform.position;
        
        // 状態の登録
        var approach = states.AddState(State.Approach, Approach);
        approach.AddTransition(State.Stay, () => { return behaviourTimer.IsTermination; });
        approach.onTransitioned = () => 
        {
            transform.position = approachBeginPos;
            behaviourTimer.Restart(approachingDuration);
        };
        approach.onFinished = () => 
        {
            transform.position = stayPos;
        };
        var exit = states.AddState(State.Exit, Exit);
        exit.AddTransition(State.Stay, () => { return behaviourTimer.IsTermination; });
        exit.onTransitioned = () =>
        {
            // タイマー設定
            behaviourTimer.Restart(exitDuration);
            
            // アイコンの移動前情報の保存
            iconBeginWorldPos   = RewardIcon.transform.position;
            iconBeginLocalPos   = RewardIcon.transform.localPosition;
            iconBeginLossyScale = RewardIcon.transform.lossyScale;
            iconBeginLocalScale = RewardIcon.transform.localScale;

            // 報酬アイコンを同じ階層にする
            // 親の拡大率の影響を受けられると都合が悪いので
            RewardIcon.transform.SetParent(transform.parent, false);
        };
        exit.onFinished = () =>
        {
            // 移動終了座標にする
            transform.position = exitingEndPos;

            // イベント呼び出し
            onExited?.Invoke();

            // 報酬アイコンの設定
            RewardIcon.transform.SetParent(transform, true);
            RewardIcon.transform.localPosition = iconBeginLocalPos;
            RewardIcon.transform.localScale    = iconBeginLocalScale;

            // 無効化
            ToUnused();
        };
        var move = states.AddState(State.Move, Move);
        move.AddTransition(State.Stay, () => { return behaviourTimer.IsTermination; });
        states.AddState(State.Stay, ()=> { });
    }

    public override void OnGenerated()
    {
        base.OnGenerated();

        // 状態の設定
        states.NowStateCode = State.Stay;
    }

    // Update is called once per frame
    void Update()
    {
        states.Behave();
    }

    /// <summary>
    /// 登場状態へ移行
    /// </summary>
    public void ToApproach()
    {
        states.NowStateCode = State.Approach;
    }

    /// <summary>
    /// 退場状態へ移行
    /// </summary>
    public void ToExit()
    {
        states.NowStateCode = State.Exit;
    }

    /// <summary>
    /// 登場
    /// </summary>
    private void Approach()
    {
        behaviourTimer.Advance();
        transform.position = Vector3.Lerp(approachBeginPos, stayPos, Easing.Quad.Out(behaviourTimer.Ratio));
    }

    /// <summary>
    /// 退場
    /// </summary>
    private void Exit()
    {
        // タイマー計測
        behaviourTimer.Advance();

        // フレーム移動
        var ratio = Easing.Quad.Out(behaviourTimer.Ratio);
        transform.position = Vector3.Lerp(stayPos, exitingEndPos, ratio);

        // 報酬アイコン移動
        RewardIcon.transform.position = Vector3.Lerp(iconBeginWorldPos, iconTarget.transform.position, ratio);

        // 報酬アイコン拡大率変更
        RewardIcon.transform.localScale = Vector3.Lerp(iconBeginLossyScale, iconTarget.transform.lossyScale, ratio);
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        behaviourTimer.Advance();
        transform.position = Vector3.Lerp(moveBeginPos, stayPos, Easing.Quad.Out(behaviourTimer.Ratio));
    }

    /// <summary>
    /// 移動させる
    /// </summary>
    /// <param name="target">移動先座標</param>
    /// <param name="duration">移動にかける秒数</param>
    /// <param name="delay">移動までの待機秒数</param>
    public void Move(Vector3 target, float duration, float delay)
    {
        moveBeginPos = transform.position;
        StayPos = target;
        behaviourTimer.Restart(duration);
        Invoke("ToMoveState", delay);
    }

    private void ToMoveState()
    {
        states.NowStateCode = State.Move;
    }
}
