using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Gameover : SingletonMonoBehaviour<Gameover>
{
    private enum State
    {
        Wait,           // 待機
        FadeIn,         // フェードイン
        Approaching,    // 登場
        Exit,           // 退場
        Select,         // 選択
        Decisioned,     // 決定後
    }

    private enum Actors
    {
        Text,
        ToTitle,
        Retry,
        Cursor
    }

    private class Actor
    {
        public Vector3 start;
        public Vector3 end;
        private RectTransform rect;
        public GameObject Obj { get; private set; }

        /// <summary>
        /// 左端の座標
        /// </summary>
        public Vector3 LeftEdge
        {
            get
            {
                return rect.gameObject.transform.position + Vector3.left * rect.sizeDelta.x * 0.5f;
            }
        }

        public Actor(GameObject obj)
        {
            Obj = obj;
            rect = obj.GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// カーソルの不透明度のカーブ
    /// </summary>
    [SerializeField]
    private SinCurve cursorAlphaCurve = new SinCurve(1.0f, new Limit(0.0f, 1.0f));

    /// <summary>
    /// 登場に要する秒数
    /// </summary>
    [SerializeField]
    private float approachingSecond = 0.75f;

    /// <summary>
    /// 決定後の挙動までの待ち秒数
    /// </summary>
    [SerializeField]
    private float decisionedWaitSecond = 0.5f;

    /// <summary>
    /// 選択肢の登場時の移動距離
    /// </summary>
    [SerializeField]
    private float selectorMoveDistance = 20.0f;

    /// <summary>
    /// カーソルと選択肢の距離
    /// </summary>
    [SerializeField]
    private float cursorBlank = 5.0f;

    [SerializeField]
    private TextMeshProUGUI textUi = null;

    [SerializeField]
    private TextMeshProUGUI toTitleUi = null;

    [SerializeField]
    private TextMeshProUGUI retryUi = null;

    [SerializeField]
    private Image cursorUi = null;

    private Timer behaviourTimer;

    private StateManager<State> states = new StateManager<State>();

    private Dictionary<Actors, Actor> actors = new Dictionary<Actors, Actor>();

    /// <summary>
    /// 選んでいる選択肢の種類
    /// </summary>
    private Actors activeSelectorKind = Actors.Retry;

    /// <summary>
    /// ゲームオーバー時の文章
    /// </summary>
    [SerializeField]
    private string overedText = "Game Over";

    /// <summary>
    /// クリア時の文章
    /// </summary>
    [SerializeField]
    private string clearedText = "Clear!";

    /// <summary>
    /// ポーズ時の文章
    /// </summary>
    [SerializeField]
    private string pausedText = "Pause Menu";

    /// <summary>
    /// ゲームオーバー時の右側の文章
    /// </summary>
    [SerializeField]
    private string overedRightSelectorText = "Retry";

    /// <summary>
    /// クリア時の右側の文章
    /// </summary>
    [SerializeField]
    private string clearedRightSelectorText = "To Next Level";

    /// <summary>
    /// カーソル移動時の音
    /// </summary>
    [SerializeField]
    private AudioClip moveCursorSE = null;

    /// <summary>
    /// 決定時の音
    /// </summary>
    [SerializeField]
    private AudioClip decisionedSE = null;

    /// <summary>
    /// メニューの種類
    /// </summary>
    public enum MenuType
    {
        Gameover,
        Clear,
        Pause
    }
    private MenuType menuType;

    /// <summary>
    /// 動作開始
    /// </summary>
    public void Call(MenuType menuType)
    {   
        states.NowStateCode = State.FadeIn;
        this.menuType = menuType;
        switch (menuType)
        {
            case MenuType.Clear:
                retryUi.text = clearedRightSelectorText;
                textUi .text = clearedText;
                break;

            case MenuType.Gameover:
                retryUi.text = overedRightSelectorText;
                textUi .text = overedText;
                break;

            case MenuType.Pause:
                retryUi.text = overedRightSelectorText;
                textUi .text = pausedText;
                break;
        }
    }

    /// <summary>
    /// 動作終了
    /// </summary>
    public void Exit()
    {
        states.NowStateCode = State.Exit;
    }

    // Use this for initialization
    void Start()
    {
        // タイムスケールを考慮しない
        behaviourTimer  .isUnscaled = true;
        cursorAlphaCurve.isUnscaled = true;

        // 動くものの登録
        var text    = new Actor(textUi   .gameObject);
        var toTitle = new Actor(toTitleUi.gameObject);
        var retry   = new Actor(retryUi  .gameObject);
        var cursor  = new Actor(cursorUi .gameObject);
        actors.Add(Actors.Text   , text);
        actors.Add(Actors.ToTitle, toTitle);
        actors.Add(Actors.Retry  , retry);
        actors.Add(Actors.Cursor , cursor);

        // 座標の登録
        text   .end   = textUi.transform.position;
        text   .start = text.end + Vector3.down * selectorMoveDistance;
        toTitle.end   = toTitleUi.transform.position;
        toTitle.start = toTitle.end + Vector3.left * selectorMoveDistance;
        retry  .end   = retryUi.transform.position;
        retry  .start = retry.end + Vector3.right * selectorMoveDistance;
        cursor .end   = GetCursorPos(retry);
        cursor .start = cursor.end + Vector3.left * selectorMoveDistance;

        // 状態の登録
        // 待機
        var wait = states.AddState(State.Wait, ()=> { });
        wait.onTransitioned = ()=>
        {
            // 不透明度のリセット
            Usual.UI.ChangeAlpha(textUi   , 0.0f);
            Usual.UI.ChangeAlpha(toTitleUi, 0.0f);
            Usual.UI.ChangeAlpha(retryUi  , 0.0f);
            Usual.UI.ChangeAlpha(cursorUi , 0.0f);

            // カーソル位置のリセット
            activeSelectorKind = Actors.Retry;

            // カーソル点滅のリセット
            cursorAlphaCurve.SetMax();
        };
        //
        // フェードイン
        var fadeIn = states.AddState(State.FadeIn, () => { });
        fadeIn.onTransitioned = () =>
        {
            // ゲーム画面暗転
            var ui = Fade.Instance;
            var fader = ui.LowerBlackBoard;
            fader.FadeOut(0.2f, ui.LowerBoardDarkness);

            // 暗転終了時の処理の登録
            fader.onFadeFinished = () =>
            {
                // 更新停止
                Time.timeScale = 0.0f;

                // 登場状態にする
                states.NowStateCode = State.Approaching;
            };
        };
        //
        // 登場
        var approaching = states.AddState(
            State.Approaching, ()=>
            {
                // 計測
                behaviourTimer.Advance();

                // 進み具合の取得
                var ratio = Easing.Quad.Out(behaviourTimer.Ratio);

                // 各オブジェクトの移動
                foreach (var actorPair in actors)
                {
                    // 参照
                    var actor = actorPair.Value;

                    // 移動
                    actor.Obj.transform.position = Vector3.Lerp(actor.start, actor.end, ratio);
                }

                // 不透明度の反映
                Usual.UI.ChangeAlpha(textUi   , ratio);
                Usual.UI.ChangeAlpha(toTitleUi, ratio);
                Usual.UI.ChangeAlpha(retryUi  , ratio);
                Usual.UI.ChangeAlpha(cursorUi , ratio);
            });
        approaching.onTransitioned = () =>
        {
            // タイマー設定
            behaviourTimer.Restart(approachingSecond);

            // 各オブジェクトの座標補正
            foreach (var actorPair in actors)
            {
                // 参照
                var actor = actorPair.Value;

                // 移動
                actor.Obj.transform.position = actor.start;
            }
        };
        approaching.AddTransition(State.Select, ()=> { return behaviourTimer.IsTermination; });
        approaching.onFinished = () =>
        {
            // 各オブジェクトの座標補正
            foreach (var actorPair in actors)
            {
                // 参照
                var actor = actorPair.Value;

                // 移動
                actor.Obj.transform.position = actor.end;
            }
        };
        //
        // 退場
        var exit = states.AddState(
           State.Exit, ()=> 
           {
               // 計測
               behaviourTimer.Advance();

               // 進み具合の取得
               var ratio = Easing.Quad.Out(behaviourTimer.Ratio);

               // 各オブジェクトの移動
               foreach (var actorPair in actors)
               {
                   // 参照
                   var actor = actorPair.Value;

                   // 移動
                   actor.Obj.transform.position = Vector3.Lerp(actor.end, actor.start, ratio);
               }

               // 不透明度の反映
               var revRatio = 1.0f - ratio;
               Usual.UI.ChangeAlpha(textUi   , revRatio);
               Usual.UI.ChangeAlpha(toTitleUi, revRatio);
               Usual.UI.ChangeAlpha(retryUi  , revRatio);
               Usual.UI.ChangeAlpha(cursorUi , revRatio);
           });
        exit.onTransitioned = () =>
        {
            // タイマー設定
            behaviourTimer.Restart(approachingSecond);

            // 各オブジェクトの座標補正
            foreach (var actorPair in actors)
            {
                // 参照
                var actor = actorPair.Value;

                // 移動
                actor.Obj.transform.position = actor.end;
            }
        };
        exit.onFinished = () =>
        {
            switch(menuType)
            {
                case MenuType.Pause:
                    PauseManager.Instance.End();
                    break;

                default:
                    break;
            }
        };
        exit.AddTransition(State.Wait, ()=> { return behaviourTimer.IsTermination; });
        //
        // 選択
        var select = states.AddState(
            State.Select, ()=> 
            {
                // カーソル点滅
                Usual.UI.ChangeAlpha(cursorUi, cursorAlphaCurve);
                cursorAlphaCurve.Advance();

                // カーソルが移動するか判別
                var isMoved = (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow));

                // 移動
                if (isMoved)
                {// 移動した場合
                    // 現在位置の更新
                    switch (activeSelectorKind)
                    {// 現在位置による分岐
                        case Actors.Retry:
                            activeSelectorKind = Actors.ToTitle;
                            break;

                        case Actors.ToTitle:
                            activeSelectorKind = Actors.Retry;
                            break;

                        default:
                            break;
                    }

                    // カーソル位置の更新
                    cursor.Obj.transform.position = GetCursorPos(actors[activeSelectorKind]);

                    // 点滅リセット
                    cursorAlphaCurve.SetMax();

                    // 効果音再生
                    AudioManager.Instance.PlaySE(moveCursorSE);
                }

                // 決定
                if (Input.GetKeyDown(KeyCode.Space))
                {// キー押下
                    // 効果音再生
                    AudioManager.Instance.PlaySE(decisionedSE);

                    // カーソル強制出現
                    Usual.UI.ChangeAlpha(cursorUi, 1.0f);
                    
                    // 決定後に遷移
                    states.NowStateCode = State.Decisioned;
                }
            });
        //
        // 決定後
        var decisioned = states.AddState(
            State.Decisioned, () =>
            {
                if (behaviourTimer.Advance())
                {
                    OnDecisioned();
                }
            });
        decisioned.onTransitioned = () =>
        {
            behaviourTimer.Restart(decisionedWaitSecond);
        };
        
        // 状態の設定
        states.NowStateCode = State.Wait;
    }

    // Update is called once per frame
    void Update()
    {
        states.Behave();
    }

    /// <summary>
    /// 選択肢決定時
    /// </summary>
    private void OnDecisioned()
    {
        // フェードアウト呼び出し
        var ui = Fade.Instance;
        ui.UpperBlackBoard.FadeOut(ui.FadingSecond);

        // フェードアウト終了時の処理の登録
        ui.UpperBlackBoard.onFadeFinished = () =>
        {
            // レベル加算
            switch (menuType)
            {
                case MenuType.Clear:
                    ++StageManager.Instance.Level;
                    break;

                case MenuType.Pause:
                    PauseManager.Instance.End();
                    break;

                default:
                    break;
            }

            // 下側のフェードボード削除
            ui.LowerBlackBoard.Alpha = 0.0f;
            
            // 遷移先による処理
            switch (activeSelectorKind)
            {// 現在位置による分岐
                case Actors.Retry:
                    // 全てのオブジェクトの初期化
                    StageManager.Instance.Restart();
                    break;

                case Actors.ToTitle:
                    // タイトルへ遷移
                    SceneManager.LoadScene("Title");
                    break;

                default:
                    break;
            }

            // 待機状態にする
            states.NowStateCode = State.Wait;

            // フェードイン呼び出し
            ui.UpperBlackBoard.FadeIn(ui.FadingSecond);

            // フェードイン終了時の処理
            ui.UpperBlackBoard.onFadeFinished = () =>
            {
                // タイムスケール復元
                Time.timeScale = 1.0f;
            };
        };
    }

    private Vector3 GetCursorPos(Actor actor)
    {
        return actor.LeftEdge + Vector3.left * (cursorUi.rectTransform.sizeDelta.x + cursorBlank);
    }
}
