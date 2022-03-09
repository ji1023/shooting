using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class StageManager : SingletonMonoBehaviour<StageManager>
{
    /// <summary>
    /// UIに表示する文章
    /// </summary>
    private const string UI_TEXT = "Stage Level ";

    /// <summary>
    /// 難易度
    /// </summary>
    private uint level = 1;
    public uint Level
    {
        get => level;
        set
        {
            // レベル変更
            var fixedValue = (value == 0) ? 1 : value;
            if (level != fixedValue)
            {// 現在のレベルと異なる場合
                // 更新
                level = fixedValue;

                // UIへの反映
                levelText.text = $"{UI_TEXT}{level}";
            }
        }
    }

    /// <summary>
    /// 難易度の上限
    /// </summary>
    [SerializeField]
    private uint levelMax = 10;
    
    /// <summary>
    /// レベルの割合（1を上回ってOK）
    /// </summary>
    public float LevelRatio => (float)level / levelMax;

    /// <summary>
    /// BGM
    /// </summary>
    [SerializeField]
    private AudioClip bgm = null;

    /// <summary>
    /// 開始時の待機秒数
    /// </summary>
    [SerializeField]
    private float startWaitSecond = 0.5f;

    /// <summary>
    /// UIの移動に要する秒数
    /// </summary>
    [SerializeField]
    private float uiMoveSecond = 0.2f;

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviourTimer;

    /// <summary>
    /// ステージが始まっているか否か
    /// </summary>
    public bool IsStarted { get; private set; } = false;

    /// <summary>
    /// ステージレベルを表示するUI
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI levelText = null;

    /// <summary>
    /// UIの移動先座標
    /// </summary>
    private Vector3 uiTargetPos;

    /// <summary>
    /// UIの初期座標
    /// </summary>
    [SerializeField]
    private Vector3 uiInitialPos = Vector3.zero;

    /// <summary>
    /// UIの初期拡大率
    /// </summary>
    [SerializeField]
    private Vector3 uiInitialScale = new Vector3(2.0f, 2.0f, 2.0f);

    /// <summary>
    /// 挙動
    /// </summary>
    private System.Action behave;

    /// <summary>
    /// ステージの状態を初期化する
    /// </summary>
    public void Restart()
    {
        // 各オブジェクトの初期化
        Player         .Instance.OnStart();
        EnemyManager   .Instance.Restart();
        ItemManager    .Instance.Restart();
        EffectManager  .Instance.Restart();
        StageProgressUI.Instance.Restart();
        Camera         .Instance.StopVibration();

        // 挙動開始
        StartBehaviour();

        // BGM再生
        AudioManager.Instance.PlayBGM(bgm);
    }

    private void Start()
    {
        // ゲームを動かす
        Time.timeScale = 1.0f;

        // 目標座標の保存
        uiTargetPos = levelText.rectTransform.position;

        // 挙動開始
        StartBehaviour();

        // BGM再生
        AudioManager.Instance.PlayBGM(bgm);

        // イベント登録
        SceneManager.sceneLoaded += SceneLoaded;
    }

    // シーン切り替え時の処理
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        // タイムスケール復元
        Time.timeScale = 1.0f;

        // レベルの伝達
        var title = Title.Instance;
        if (level > title.levelMax)
        {// 最大レベル以上のレベルだった場合
            title.levelMax = level;
        }

        // 削除
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void StartBehaviour()
    {
        // 未開始
        IsStarted = false;

        // タイマー設定
        behaviourTimer.Restart(startWaitSecond);

        // UI状態のリセット
        levelText.transform.position   = uiInitialPos;
        levelText.transform.localScale = uiInitialScale;

        // 待機処理の登録
        behave = () =>
        {
            // UI移動処理の登録
            if (behaviourTimer.Advance())
            {// 待機時間が終わった場合
                // タイマー設定
                behaviourTimer.Restart(uiMoveSecond);
                
                // 処理登録
                behave = () =>
                {
                    // 計測
                    behaviourTimer.Advance();

                    // UIの移動
                    var ratio = Easing.Quad.Out(behaviourTimer.Ratio);
                    levelText.transform.position = Vector3.Lerp(uiInitialPos, uiTargetPos, ratio);

                    // UIの拡縮
                    levelText.transform.localScale = Vector3.Lerp(uiInitialScale, Vector3.one, ratio);

                    // 挙動終了
                    if (behaviourTimer.IsTermination)
                    {// 計測終了の場合
                        // 座標の補正
                        levelText.rectTransform.position = uiTargetPos;

                        // ステージ開始
                        IsStarted = true;
                    }
                };
            }
        };
    }

    public void Update()
    {
        // 開始済みなら無視
        if (IsStarted) { return; }

        // 挙動
        behave();
    }
}
