using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Fade : SingletonMonoBehaviour<Fade>
{
    [SerializeField]
    private string UpperBlackBoardName = "UpperBlackBoard";

    [SerializeField]
    private string LowerBlackBoardName = "LowerBlackBoard";

    [SerializeField]
    private float lowerBoardDarkness = 0.75f;
    /// <summary>
    /// UIの下側の黒い板の暗転具合
    /// </summary>
    public float LowerBoardDarkness => lowerBoardDarkness;

    [SerializeField]
    private float fadingSecond = 0.25f;
    /// <summary>
    /// フェードに要する秒数
    /// </summary>
    public float FadingSecond => fadingSecond;

    [SerializeField]
    private Fader upperBlackBoard = null;
    /// <summary>
    /// UIより上の黒い板
    /// </summary>
    public Fader UpperBlackBoard => upperBlackBoard;

    [SerializeField]
    private Fader lowerBlackBoard = null;
    /// <summary>
    /// UIより下の黒い板
    /// </summary>
    public Fader LowerBlackBoard => lowerBlackBoard;

    private void Start()
    {
        FindBoards();
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += SceneLoaded;
    }

    // シーン切り替え時の処理
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        // フェード用板の検索
        FindBoards();

        // フェードイン
        upperBlackBoard.Alpha = 1.0f;
        upperBlackBoard.FadeIn(fadingSecond * 2.0f);

        // 毎回呼び出されてほしいため、関数削除は不要
    }

    private void FindBoards()
    {
        if (upperBlackBoard == null)
        {
            var obj = GameObject.Find(UpperBlackBoardName);
            if (obj)
            {
                upperBlackBoard = obj.GetComponent<Fader>();
            }
        }
        if (lowerBlackBoard == null)
        {
            var obj = GameObject.Find(LowerBlackBoardName);
            if (obj)
            {
                lowerBlackBoard = obj.GetComponent<Fader>();
            }
        }
    }
}
