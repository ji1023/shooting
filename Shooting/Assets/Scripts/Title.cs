using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Title : SingletonMonoBehaviour<Title>
{
    /// <summary>
    /// 決定ボタン
    /// </summary>
    [SerializeField]
    private KeyCode decisionButton = KeyCode.Space;

    /// <summary>
    /// BGM
    /// </summary>
    [SerializeField]
    private AudioClip bgm = null;

    /// <summary>
    /// カーソル移動時の音
    /// </summary>
    [SerializeField]
    private AudioClip movedCursorSE = null;

    /// <summary>
    /// 決定時の音
    /// </summary>
    [SerializeField]
    private AudioClip decisionedSE = null;

    /// <summary>
    /// 選択できる最大レベル
    /// </summary>
    [SerializeField]
    public uint levelMax = 1;

    /// <summary>
    /// 選んでいるレベル
    /// </summary>
    [SerializeField]
    private uint selectingLevel = 1;

    [SerializeField]
    private TextMeshProUGUI levelUi = null;

    [SerializeField]
    private Image[] cursors = new Image[2];
    /// <summary>
    /// カーソルの基準座標
    /// </summary>
    private List<Vector3> cursorBasePos = new List<Vector3>();

    [SerializeField]
    private SinCurve cursorMoveValue;

    [SerializeField]
    private float cursorMoveInterval = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        // BGM再生
        AudioManager.Instance.PlayBGM(bgm);

        // 選択レベルを最大にする
        selectingLevel = levelMax;

        // 秒数反映
        cursorMoveValue.Interval = cursorMoveInterval;

        // UIへ反映
        levelUi.text = selectingLevel.ToString();

        // カーソル基準位置を保存
        foreach (var cursor in cursors)
        {
            cursorBasePos.Add(cursor.transform.position);   // StartなのでList.Add()やっちゃう　要素も少ないし
        }

        // イベント登録
        SceneManager.sceneLoaded += SceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        // レベル選択
        var isMoved = false;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selectingLevel;
            if (selectingLevel <= 0)
            {
                selectingLevel = levelMax;
            }
            isMoved = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selectingLevel;
            if (selectingLevel > levelMax)
            {
                selectingLevel = 1;
            }
            isMoved = true;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectingLevel = levelMax;
            isMoved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectingLevel = 1;
            isMoved = true;
        }

        // 反応
        if (isMoved)
        {// 選択レベルが変わった場合
            // UIへ反映
            levelUi.text = selectingLevel.ToString();

            // 効果音再生
            AudioManager.Instance.PlaySE(movedCursorSE);
        }

        // カーソル挙動
        cursorMoveValue.Advance();
        var index = 0;
        foreach (var cursor in cursors)
        {
            var sign = (index % 2) == 0 ? 1 : -1;
            cursor.transform.position = cursorBasePos[index] + Vector3.right * (cursorMoveValue * sign);
            ++index;
        }

        // 決定
        if (Input.GetKeyDown(decisionButton))
        {
            // 効果音再生
            AudioManager.Instance.PlaySE(decisionedSE);

            // フェード
            var fade = Fade.Instance;
            fade.UpperBlackBoard.FadeOut(fade.FadingSecond);
            fade.UpperBlackBoard.onFadeFinished = () =>
            {
                SceneManager.LoadScene("MainGame");
            };
        }
    }

    // シーン切り替え時の処理
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        // レベルの伝達
        StageManager.Instance.Level = selectingLevel;

        // 削除
        SceneManager.sceneLoaded -= SceneLoaded;
    }
}
