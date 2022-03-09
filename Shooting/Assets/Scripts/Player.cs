using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : SingletonMonoBehaviour<Player>
{
    #region 移動関連
    [Header("移動関連")]
    /// <summary>
    /// 移動速度（毎秒）
    /// </summary>
    [SerializeField]
    private float moveSpeed = 2.0f;

    /// <summary>
    /// 移動制限
    /// </summary>
    [SerializeField]
    private float moveLimit = 4.0f;

    /// <summary>
    /// 初期座標
    /// </summary>
    private Vector3 initialPos;
    #endregion

    #region 武器関連
    /// <summary>
    /// 武器の種類
    /// </summary>
    public enum WeaponKind
    {
        Straight = 0,   // 直進
        Chase,          // 追尾
        Wave,           // sin波
        MainSubBorder,  // メイン武器とサブ武器の境目
        Bomb = 100,     // 爆弾
        Beam,           // ビーム
        Count
    }

    /// <summary>
    /// メイン武器の種類
    /// </summary>
    public enum MainWeaponKind
    {
        Straight = 0,
        Chase,
        Wave,
        Count
    }

    /// <summary>
    /// サブ武器の種類
    /// </summary>
    public enum SubWeaponKind
    {
        Bomb = 100,
        Beam,
        Count
    }

    /// <summary>
    /// 武器のプール
    /// </summary>
    [System.Serializable]
    public class WeaponPool : Pool<Weapon> { }

    /// <summary>
    /// 武器の情報
    /// </summary>
    [System.Serializable]
    private abstract class WeaponStatus
    {
        /// <summary>
        /// 規定値
        /// </summary>
        [System.Serializable]
        public class Defaults
        {
            /// <summary>
            /// タイマーの秒数
            /// </summary>
            [SerializeField]
            private float interval = 0.1f;
            /// <summary>
            /// タイマーの秒数
            /// </summary>
            public float Interval => interval;

            /// <summary>
            /// 強化によるタイマーの削減率
            /// </summary>
            [SerializeField]
            private Percentage reductionRate = 80.0f;
            /// <summary>
            /// 強化によるタイマーの削減率
            /// </summary>
            public Percentage ReducationRate => reductionRate;

            /// <summary>
            /// 強化による最大削減秒数
            /// </summary>
            public float ReducationMaxSecond
            {
                get
                {
                    return Interval * reductionRate.Ratio;
                }
            }
        }

        /// <summary>
        /// 武器の種類
        /// </summary>
        [SerializeField]
        private WeaponKind kind = WeaponKind.Straight;
        /// <summary>
        /// 武器の種類
        /// </summary>
        public WeaponKind Kind => kind;

        /// <summary>
        /// 解放済みか否か
        /// </summary>
        [SerializeField]
        public bool isUnlocked = false;

        /// <summary>
        /// 最初から解放済みだったか否か
        /// </summary>
        public bool IsInitialUnlocked { get; private set; } = false;

        /// <summary>
        /// 規定値一覧
        /// </summary>
        [SerializeField]
        public Defaults defaults = new Defaults();

        /// <summary>
        /// 本体一覧
        /// </summary>
        [SerializeField]
        public WeaponPool pool = new WeaponPool();

        /// <summary>
        /// 発射までの秒数（リキャストまでの秒数）
        /// </summary>
        [System.NonSerialized]
        public Timer toShot = new Timer(true, true);

        /// <summary>
        /// 色
        /// </summary>
        public Color Color => pool.prefabs[0].Color;

        /// <summary>
        /// タイマーの計測秒数をリセットして計測開始
        /// </summary>
        public void ResetTimer()
        {
            toShot.Interval = defaults.Interval;
            toShot.Restart();
        }

        /// <summary>
        /// タイマーの計測秒数を削減する
        /// </summary>
        /// <param name="ratio">削減割合</param>
        public void ReducationInterval(float ratio)
        {
            toShot.Interval = defaults.Interval - defaults.ReducationMaxSecond * ratio;
        }

        /// <summary>
        /// 解放状態を退避
        /// </summary>
        public void SaveUnlocked()
        {
            IsInitialUnlocked = isUnlocked;
        }

        /// <summary>
        /// 解放状態を復元
        /// </summary>
        public void LoadUnlocked()
        {
            isUnlocked = IsInitialUnlocked;
        }
    }

    /// <summary>
    /// メイン武器の情報
    /// </summary>
    [System.Serializable]
    private class MainWeaponStatus : WeaponStatus
    {
        [SerializeField]
        private Image ui = null;

        /// <summary>
        /// UI
        /// </summary>
        public Image Ui => ui;
        
        [SerializeField]
        private AudioClip shottedSE = null;

        /// <summary>
        /// 撃った時の効果音
        /// </summary>
        public AudioClip ShottedSE => shottedSE;

        /// <summary>
        /// UIを表示
        /// </summary>
        public void ShowUI()
        {
            ui.gameObject.SetActive(true);
            ui.color = pool.prefabs[0].Color;
        }

        /// <summary>
        /// UIを非表示
        /// </summary>
        public void HideUI()
        {
            ui.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// サブ武器の情報
    /// </summary>
    [System.Serializable]
    private class SubWeaponStatus : WeaponStatus
    {
        /// <summary>
        /// ストック数
        /// </summary>
        [System.NonSerialized]
        public int stockCount = 0;

        /// <summary>
        /// UI
        /// </summary>
        [SerializeField]
        private SubWeaponUI ui = null;
        /// <summary>
        /// UI
        /// </summary>
        public SubWeaponUI Ui => ui;

        /// <summary>
        /// 溜めゲージ
        /// </summary>
        [SerializeField]
        private Image chargeGauge = null;

        /// <summary>
        /// 溜め完了する秒数
        /// </summary>
        [SerializeField]
        private float chargedSecond = 1.0f;

        /// <summary>
        /// 溜めタイマー
        /// </summary>
        private Timer chargeTimer;

        /// <summary>
        /// 溜め具合
        /// </summary>
        public float ChargeRatio => chargeTimer.Ratio;

        /// <summary>
        /// 使用するボタン
        /// </summary>
        [SerializeField]
        public KeyCode useButton = KeyCode.Return;

        /// <summary>
        /// 溜める
        /// </summary>
        public bool Charging()
        {
            var isCharged = chargeTimer.Advance();
            chargeGauge.fillAmount = chargeTimer.Ratio;
            return isCharged;
        }

        /// <summary>
        /// 溜めリセット
        /// </summary>
        public void ResetCharge()
        {
            chargeTimer.Restart(chargedSecond);
            chargeGauge.fillAmount = 0.0f;
        }
    }

    /// <summary>
    /// 武器の情報
    /// </summary>
    [System.Serializable]
    private class MainWeapons
    {
        /// <summary>
        /// 種類ごとの情報一覧
        /// </summary>
        [System.NonSerialized]
        public Dictionary<MainWeaponKind, MainWeaponStatus> statuses = new Dictionary<MainWeaponKind, MainWeaponStatus>();

        /// <summary>
        /// 選択されている武器の種類
        /// </summary>
        [SerializeField]
        public MainWeaponKind selectedKind = MainWeaponKind.Straight;

        /// <summary>
        /// 選択されている武器の情報
        /// </summary>
        public MainWeaponStatus SelectedWeaponStatus
        {
            get
            {
                return statuses[selectedKind];
            }
        }
    }

    /// <summary>
    /// メイン武器一覧
    /// </summary>
    [Header("武器関連")]
    [SerializeField]
    private MainWeapons mainWeapons = new MainWeapons();
    /// <summary>
    /// メイン武器の情報一覧（エディタ設定用）
    /// </summary>
    [SerializeField]
    private List<MainWeaponStatus> mainWeaponStatus = new List<MainWeaponStatus>();

    /// <summary>
    /// サブ武器一覧
    /// </summary>
    private Dictionary<SubWeaponKind, SubWeaponStatus> subWeapons = new Dictionary<SubWeaponKind, SubWeaponStatus>();
    /// <summary>
    /// サブ武器の情報一覧（エディタ設定用）
    /// </summary>
    [SerializeField]
    private List<SubWeaponStatus> subWeaponStatus = new List<SubWeaponStatus>();

    /// <summary>
    /// 最大ストック数
    /// </summary>
    private int StockMax => Mathf.FloorToInt(buffs.way.value) + 1;

    /// <summary>
    /// 溜めの情報
    /// </summary>
    [System.Serializable]
    private class ChargeStatus
    {
        /// <summary>
        /// 最大溜め時間
        /// </summary>
        [SerializeField]
        public float maxSecond = 4.0f;

        /// <summary>
        /// 最大溜めでのダメージ係数
        /// </summary>
        [SerializeField]
        public float damageBuff = 2.0f;

        /// <summary>
        /// 溜め発射に使うキー
        /// </summary>
        [SerializeField]
        public KeyCode key = KeyCode.Space;

        /// <summary>
        /// 溜めているか否か
        /// </summary>
        [System.NonSerialized]
        public bool isCharging = false;

        /// <summary>
        /// 溜めタイマー
        /// </summary>
        [System.NonSerialized]
        public Timer timer;
    }

    /// <summary>
    /// 溜めの情報
    /// </summary>
    [SerializeField]
    private ChargeStatus chargeStatus = new ChargeStatus();

    /// <summary>
    /// 複数発射時の弾同士の角度
    /// </summary>
    [SerializeField]
    private Degree bulletAngle = 10.0f;

    /// <summary>
    /// 弾を生成する時の距離
    /// </summary>
    [SerializeField]
    private float toBulletDistance = 0.5f;

    /// <summary>
    /// 溜めた追尾弾の追加火力倍率
    /// </summary>
    [SerializeField]
    private float chargedChaseBuff = 0.1f;

    /// <summary>
    /// 弾をまとめるオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject bulletsAggregator = null;

    /// <summary>
    /// 弾をまとめるオブジェクト
    /// </summary>
    public GameObject BulletsAggregator => bulletsAggregator;

    /// <summary>
    /// 武器のカーソル
    /// </summary>
    [SerializeField]
    private Image weaponCursor = null;

    /// <summary>
    /// 最初に選択されていたメイン武器の種類
    /// </summary>
    private MainWeaponKind initialSelectedWeaponKind;
    #endregion

    #region 体力関連
    [Header("体力関連")]
    /// <summary>
    /// 体力の最大値の規定値
    /// </summary>
    [SerializeField]
    private float defaultLifeMax = 100.0f;

    /// <summary>
    /// 体力の最大値
    /// </summary>
    [SerializeField]
    private float lifeMax = 0.0f;

    /// <summary>
    /// 現在の体力
    /// </summary>
    [SerializeField]
    private float lifeNow = 0.0f;

    /// <summary>
    /// ゲームオーバー表示までの秒数
    /// </summary>
    [SerializeField]
    private float toGameoverSecond = 3.0f;

    /// <summary>
    /// 体力の割合
    /// </summary>
    public float LifeRatio => lifeNow / lifeMax;

    /// <summary>
    /// 体力が最大か否か
    /// </summary>
    public bool IsLifeMax => (lifeNow >= lifeMax);

    /// <summary>
    /// レンダラー
    /// </summary>
    public SpriteRenderer Renderer
    {
        get
        {
            if (renderer == null)
            {
                renderer = GetComponent<SpriteRenderer>();
            }
            return renderer;
        }
    }
    private new SpriteRenderer renderer = null;

    /// <summary>
    /// 最初の色
    /// </summary>
    private Color initialColor;

    /// <summary>
    /// 被弾時の色
    /// </summary>
    [SerializeField]
    private Color damagedColor = Color.red;

    /// <summary>
    /// 不透明度
    /// </summary>
    public float Alpha
    {
        get => Renderer.color.a;
        set
        {
            var color = Renderer.color;
            color.a = value;
            Renderer.color = color;
        }
    }

    /// <summary>
    /// 色が変わり続ける秒数
    /// </summary>
    [SerializeField]
    private float reactionSecond = 0.1f;

    /// <summary>
    /// 敵との接触ダメージ
    /// </summary>
    [SerializeField]
    private float contactDamage = 3.0f;

    /// <summary>
    /// 生きているか否か
    /// </summary>
    public bool IsAlive => !IsDead;

    /// <summary>
    /// 死亡したか否か
    /// </summary>
    public bool IsDead { get; private set; } = false;
    #endregion

    #region 強化関連
    /// <summary>
    /// 強化情報
    /// </summary>
    [System.Serializable]
    private class Buffs
    {
        /// <summary>
        /// 連射速度
        /// </summary>
        public Gauge rapidFire = new Gauge(new Limit(0.0f, 1.0f));

        /// <summary>
        /// 火力
        /// </summary>
        public Gauge damage = new Gauge(new Limit(1.0f, 2.0f));

        /// <summary>
        /// 弾数
        /// </summary>
        public Gauge way = new Gauge(5.0f);

        /// <summary>
        /// 体力
        /// </summary>
        public Gauge life = new Gauge(50.0f);

        /// <summary>
        /// バフを初期化する
        /// </summary>
        public void Reset()
        {
            rapidFire.SetMin();
            damage   .SetMin();
            way      .SetMin();
            life     .SetMin();
        }
    }
    [Header("強化関連")]
    /// <summary>
    /// 強化情報
    /// </summary>
    [SerializeField]
    private Buffs buffs = new Buffs();

    /// <summary>
    /// アイテム獲得リアクション終了までの時間
    /// </summary>
    [SerializeField]
    private Timer toItemCatchedReactionEnd;

    /// <summary>
    /// 獲得したアイテムの色
    /// </summary>
    private Color catchedItemColor;
    #endregion

    #region UI関連
    [Header("UI関連")]
    /// <summary>
    /// 体力バー
    /// </summary>
    [SerializeField]
    private Bar lifeBar = null;

    /// <summary>
    /// 溜めバー
    /// </summary>
    [SerializeField]
    private Image chargeBar = null;

    /// <summary>
    /// ダメージバー
    /// </summary>
    [SerializeField]
    private Image damageBar = null;

    /// <summary>
    /// 連射速度バー
    /// </summary>
    [SerializeField]
    private Image rapidFireBar = null;

    /// <summary>
    /// 弾数バー
    /// </summary>
    [SerializeField]
    private Image wayBar = null;

    /// <summary>
    /// 体力増加量バー
    /// </summary>
    [SerializeField]
    private Image lifeAddBar = null;

    /// <summary>
    /// 武器のアイコン
    /// </summary>
    [SerializeField]
    private Image weaponIcon = null;
    #endregion

    #region ミッション関連
    [Header("ミッション関連")]
    /// <summary>
    /// 同時に発生する最大ミッション数
    /// </summary>
    [SerializeField]
    private uint missionMax = 4;

    /// <summary>
    /// 敵の目標撃破数の幅
    /// </summary>
    [SerializeField]
    private Limit targetDestroyCountRange = new Limit(5, 9);

    /// <summary>
    /// 発生中ミッション一覧
    /// </summary>
    private LinkedList<Mission> missions = new LinkedList<Mission>();

    /// <summary>
    /// 解放されていない武器の種類
    /// </summary>
    private LinkedList<WeaponKind> lockedWeaponKinds = new LinkedList<WeaponKind>();

    /// <summary>
    /// 初期状態の解放されていない武器の種類
    /// </summary>
    private LinkedList<WeaponKind> initialLockedWeaponKinds = new LinkedList<WeaponKind>();

    /// <summary>
    /// 敵のアイコン
    /// </summary>
    [System.Serializable]
    private class EnemyIcon
    {
        [SerializeField]
        private Mission.EnemyKind kind = Mission.EnemyKind.Straight;
        public Mission.EnemyKind Kind => kind;

        [SerializeField]
        private Sprite icon = null;
        public Sprite Icon => icon;
    }

    /// <summary>
    /// エディタ設定用、敵のアイコン一覧
    /// </summary>
    [SerializeField]
    private List<EnemyIcon> enemyIconList = new List<EnemyIcon>();

    /// <summary>
    /// 敵の種類別のアイコン一覧
    /// </summary>
    private Dictionary<Mission.EnemyKind, Sprite> enemyIcons = new Dictionary<Mission.EnemyKind, Sprite>();

    /// <summary>
    /// 敵の名前
    /// </summary>
    private Dictionary<Mission.EnemyKind, string> enemyNames = new Dictionary<Mission.EnemyKind, string>();

    /// <summary>
    /// 報酬の名前
    /// </summary>
    private Dictionary<WeaponKind, string> rewardNames = new Dictionary<WeaponKind, string>();

    /// <summary>
    /// ミッションUIの親になるオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject missionParent = null;

    [System.Serializable]
    public class MissionUIPool : Pool<MissionUI> { }
    /// <summary>
    /// ミッションUIのプレハブ
    /// </summary>
    [SerializeField]
    private MissionUIPool missionUis = new MissionUIPool();

    /// <summary>
    /// ミッションUIの基準位置
    /// </summary>
    [SerializeField]
    private Vector3 missionUiBasePos = Vector3.zero;

    /// <summary>
    /// ミッションUI同士の間隔
    /// </summary>
    [SerializeField]
    private float missionUiBlank = 30.0f;
    #endregion

    #region 効果音関連
    [Header("音関連")]
    /// <summary>
    /// 被弾時の音
    /// </summary>
    [SerializeField]
    private AudioClip damagedSE = null;

    /// <summary>
    /// アイテム獲得時の音
    /// </summary>
    [SerializeField]
    private AudioClip catchedItemSE = null;

    /// <summary>
    /// 武器選択時の音
    /// </summary>
    [SerializeField]
    private AudioClip selectedWeaponSE = null;

    /// <summary>
    /// 溜め撃ちの音
    /// </summary>
    [SerializeField]
    private AudioClip chargeShotSE = null;

    /// <summary>
    /// ミッションクリア時の音
    /// </summary>
    [SerializeField]
    private AudioClip clearedMissionSE = null;

    /// <summary>
    /// ビーム発射時の音
    /// </summary>
    [SerializeField]
    private AudioClip beamSE = null;

    /// <summary>
    /// 爆弾使用時の音
    /// </summary>
    [SerializeField]
    private AudioClip bombSE = null;

    /// <summary>
    /// サブ武器リキャスト完了時の音
    /// </summary>
    [SerializeField]
    private AudioClip recastedSE = null;

    /// <summary>
    /// 最大溜め発射時の音
    /// </summary>
    [SerializeField]
    private AudioClip maxChargeShotSE = null;

    /// <summary>
    /// 溜め切った音
    /// </summary>
    [SerializeField]
    private AudioClip chargedSE = null;

    /// <summary>
    /// 溜め用オーディオソース
    /// </summary>
    [SerializeField]
    private AudioSource chargeAudioSource = null;

    /// <summary>
    /// 溜め音のループ開始サンプル位置
    /// </summary>
    [SerializeField]
    private uint chargeAudioLoopSampleBegin = 168694;

    /// <summary>
    /// 溜め音のループ終了サンプル位置
    /// </summary>
    [SerializeField]
    private uint chargeAudioLoopSampleEnd = 207446;

    /// <summary>
    /// 溜め音のループ開始位置
    /// </summary>
    private float chargeAudioLoopBegin;

    /// <summary>
    /// 溜め音のループ終了位置
    /// </summary>
    private float chargeAudioLoopEnd;
    #endregion

    /// <summary>
    /// 一番近い敵
    /// </summary>
    public Enemy NearEnemy { get; private set; }

    /// <summary>
    /// 倒した数を増やす
    /// </summary>
    /// <param name="kind">倒した敵の種類</param>
    /// <param name="weakness">倒した敵の弱点</param>
    public void AddDestroyCount(Mission.EnemyKind kind, WeaponKind? weakness)
    {
        var isCleared = false;
        var node = missions.First;
        while (node != null)
        {
            // 参照
            var mission = node.Value;

            // 加算
            mission.AddDestroyCount(kind, weakness);

            // 進捗率更新
            mission.ui.Ratio = mission.getRatio();

            // 次のノードを参照
            var nextNode = node.Next;   // 下で消す可能性があるので、消す前に退避

            // 達成判定
            if (mission.Judgement())
            {// 達成した場合
                // クリア
                isCleared = true;

                // UI退場
                mission.ui.ToExit();

                // 一覧から削除
                missions.Remove(node);

                // 効果音再生
                AudioManager.Instance.PlaySE(clearedMissionSE);
            }

            // 次のノードへ移動
            node = nextNode;
        }

        // 隙間を詰める
        if (isCleared)
        {// 何かしらのミッションを達成した場合
            var index = 0;
            foreach(var mission in missions)
            {
                mission.ui.Move(
                    GetMissionUIPos(mission.ui, index), 
                    0.8f,
                    0.3f * index);
                ++index;
            }
        }
    }

    /// <summary>
    /// 武器が解放されているか否か
    /// </summary>
    /// <param name="kind">検査する武器の種類</param>
    /// <returns>解放されていればtrue</returns>
    public bool IsUnlocked(WeaponKind kind)
    {
        if (IsMainWeapon(kind))
        {// メイン武器の場合
            return IsUnlocked((MainWeaponKind)kind);
        }
        else
        {// サブ武器の場合
            return IsUnlocked((SubWeaponKind)kind);
        }
    }

    /// <summary>
    /// メイン武器が解放されているか否か
    /// </summary>
    /// <param name="kind">検査する武器の種類</param>
    /// <returns>解放されていればtrue</returns>
    public bool IsUnlocked(MainWeaponKind kind)
    {
        return mainWeapons.statuses[kind].isUnlocked;
    }

    /// <summary>
    /// サブ武器が解放されているか否か
    /// </summary>
    /// <param name="kind">検査する武器の種類</param>
    /// <returns>解放されていればtrue</returns>
    public bool IsUnlocked(SubWeaponKind kind)
    {
        return subWeapons[kind].isUnlocked;
    }

    /// <summary>
    /// 武器を解放する
    /// </summary>
    /// <param name="kind">解放する武器の種類</param>
    private void Unlock(MainWeaponKind kind)
    {
        // 解放
        mainWeapons.statuses[kind].isUnlocked = true;

        // UIへの反映
        mainWeapons.statuses[kind].ShowUI();
    }

    /// <summary>
    /// 武器を解放する
    /// </summary>
    /// <param name="kind">解放する武器の種類</param>
    private void Unlock(SubWeaponKind kind)
    {
        var status = subWeapons[kind];
        status.isUnlocked = true;
        status.stockCount = 1;
        status.Ui.Icon.fillAmount = 1.0f;
    }
    
    /// <summary>
    /// 特定の武器がメイン武器か否か検査する
    /// </summary>
    /// <param name="kind">調べたい武器の種類</param>
    /// <returns>メイン武器ならtrue</returns>
    public bool IsMainWeapon(WeaponKind kind)
    {
        return kind < WeaponKind.MainSubBorder;
    }

    /// <summary>
    /// 特定の武器がサブ武器か否か検査する
    /// </summary>
    /// <param name="kind">調べたい武器の種類</param>
    /// <returns>サブ武器ならtrue</returns>
    public bool IsSubWeapon(WeaponKind kind)
    {
        return kind > WeaponKind.MainSubBorder;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // 色の取得
        initialColor = Renderer.color;

        // 座標の保存
        initialPos = transform.position;

        // エディタ側の設定値の反映
        foreach (var status in mainWeaponStatus)
        {
            mainWeapons.statuses.Add((MainWeaponKind)status.Kind, status);
        }
        foreach (var status in subWeaponStatus)
        {
            subWeapons.Add((SubWeaponKind)status.Kind, status);
        }

        // デフォルトの武器のみ最初から解放
        mainWeapons.SelectedWeaponStatus.isUnlocked = true;
        initialSelectedWeaponKind = mainWeapons.selectedKind;

        // 新規作成時の処理の登録／未解放武器の登録
        foreach (var weapon in mainWeapons.statuses)
        {
            weapon.Value.SaveUnlocked();
            weapon.Value.pool.beforeOnInstantiated = (Weapon obj) => { obj.tag = Tags.PlayerWeapon; };
            if (!weapon.Value.isUnlocked)
            {// 未開放の場合
                initialLockedWeaponKinds.AddLast(weapon.Value.Kind);
            }
        }
        foreach (var weapon in subWeapons)
        {
            weapon.Value.SaveUnlocked();
            weapon.Value.pool.beforeOnInstantiated = (Weapon obj) => { obj.tag = Tags.PlayerWeapon; };
            if (!weapon.Value.isUnlocked)
            {// 未開放の場合
                initialLockedWeaponKinds.AddLast(weapon.Value.Kind);
            }
        }

        // 追従オブジェクトの登録処理の登録
        subWeapons[SubWeaponKind.Beam].pool.beforeOnInstantiated +=
            (Weapon obj) => 
            {
                var beam = (Beam)obj;
                beam.clingObject = gameObject;
            };
        
        // 敵のアイコン一覧の整形
        foreach(var enemyIcon in enemyIconList)
        {
            enemyIcons.Add(enemyIcon.Kind, enemyIcon.Icon);
        }

        // 報酬名の登録
        rewardNames[WeaponKind.Straight] = "直進弾";
        rewardNames[WeaponKind.Chase   ] = "追尾弾";
        rewardNames[WeaponKind.Wave    ] = "ウェーブ弾";
        rewardNames[WeaponKind.Bomb    ] = "爆弾";
        rewardNames[WeaponKind.Beam    ] = "ビーム";

        // 敵の名前の登録
        enemyNames[Mission.EnemyKind.Straight] = "直進する敵";
        enemyNames[Mission.EnemyKind.Aimer   ] = "突進してくる敵";
        enemyNames[Mission.EnemyKind.StayShot] = "固定砲台の敵";
        enemyNames[Mission.EnemyKind.MoveShot] = "移動砲台の敵";
        enemyNames[Mission.EnemyKind.Weakness] = "弱点を持つ敵";
        enemyNames[Mission.EnemyKind.All     ] = "いずれかの敵";

        // ループ位置を求める
        var invSample = 1.0f / chargeAudioSource.clip.samples;
        var beginRatio = chargeAudioLoopSampleBegin * invSample;
        var endRatio   = chargeAudioLoopSampleEnd   * invSample;
        chargeAudioLoopBegin = chargeAudioSource.clip.length * beginRatio;
        chargeAudioLoopEnd   = chargeAudioSource.clip.length * endRatio;

        // 開始時の処理
        OnStart();
    }

    // Update is called once per frame
    private void Update()
    {
        // 音量の反映
        chargeAudioSource.volume = AudioManager.Instance.seVolume;

        // ステージ未開始なら更新しない
        if (!StageManager.Instance.IsStarted) { return; }

        // ポーズ中は更新しない
        if (PauseManager.Instance.IsPaused) { return; }

        // ボス撃破後は更新しない
        if (EnemyManager.Instance.Boss.IsDestroied) { return; }

        // 操作受付
        Operate();

        // アイテム獲得リアクション
        ItemCatchedReaction();

        // 一番近い敵の更新
        var minDistance = float.MaxValue;
        EnemyManager.Instance.Foreach(
            (Enemy enemy) =>
            {
                // 追尾弾が効かない敵を除外
                if (enemy.Weakness != null && enemy.Weakness != WeaponKind.Chase) { return; }

                // 距離を測る
                var distance = (transform.position - enemy.transform.position).sqrMagnitude;    // 2乗でも大小関係は変わらないのでこれで（軽量化）

                // 対象の更新
                if (distance < minDistance)
                {// 距離が最短距離よりも短い場合
                    minDistance = distance;
                    NearEnemy = enemy;
                }
            }
        );

        // UIへの反映
        lifeBar.Ratio = LifeRatio;
        chargeBar.fillAmount = chargeStatus.timer.Ratio;
    }

    /// <summary>
    /// 開始時の処理
    /// </summary>
    public void OnStart()
    {
        // 蘇生
        IsDead = false;

        // 溜めのリセット
        chargeStatus.isCharging = false;
        chargeStatus.timer.Reset(true);
        
        // 色のリセット
        Renderer.color = initialColor;
        toItemCatchedReactionEnd.Reset(true);

        // 座標のリセット
        transform.position = initialPos;

        // 体力リセット
        lifeNow = lifeMax = defaultLifeMax;
        
        // 発射タイマーの設定
        chargeStatus.timer.Interval = chargeStatus.maxSecond;
        foreach(var status in mainWeapons.statuses)
        {
            status.Value.ResetTimer();
        }
        foreach (var status in subWeapons)
        {
            status.Value.ResetTimer();
        }

        // 選択を戻す
        mainWeapons.selectedKind = initialSelectedWeaponKind;

        // 武器アイコン変更
        UpdateWeaponIcon();

        // 溜めゲージの色変更
        chargeBar.color = mainWeapons.SelectedWeaponStatus.pool.prefabs[0].Color;

        // 強化の初期化
        buffs.Reset();

        // サブ武器の設定
        foreach (var subWeapon in subWeapons)
        {// サブ武器一覧のループ
            // 参照
            var status = subWeapon.Value;

            // 解放状態の初期化
            status.LoadUnlocked();

            // 個数の設定
            var fillAmount = 0.0f;
            status.stockCount = 0;
            if (status.isUnlocked)
            {// エディタで解放されている場合
                Unlock(subWeapon.Key);
                fillAmount = 1.0f;
            }

            // UIへの反映
            status.Ui.Icon.fillAmount = fillAmount;
            status.Ui.Text.text = status.stockCount.ToString();

            // 溜めリセット
            status.ResetCharge();
        }

        // プールのリセット
        foreach(var mainWeapon in mainWeapons.statuses)
        {
            // 参照
            var status = mainWeapon.Value;

            // プール初期化
            status.pool.AllToUnused();

            // 解放状態の初期化
            status.LoadUnlocked();

            // ついでにUI表示
            if (status.isUnlocked)
            {// 解放されている場合
                status.ShowUI();
            }
            else
            {// 未解放の場合
                status.HideUI();
            }
        }
        foreach (var subWeapon in subWeapons)
        {
            subWeapon.Value.pool.AllToUnused();
        }

        // 未解放武器のリセット
        lockedWeaponKinds.Clear();
        foreach(var kind in initialLockedWeaponKinds)
        {
            lockedWeaponKinds.AddLast(kind);
        }

        // ミッション追加
        missionUis.AllToUnused();
        missions.Clear();
        for (int i = 0; i < missionMax; ++i)
        {
            AddMission();
        }

        // UIリセット
        damageBar   .fillAmount =
        rapidFireBar.fillAmount =
        wayBar      .fillAmount =
        lifeAddBar  .fillAmount = 0.0f;
        lifeBar.Ratio       = 
        lifeBar.LengthScale = 1.0f;
        weaponCursor.transform.position = mainWeapons.SelectedWeaponStatus.Ui.transform.position;
    }

    /// <summary>
    /// ミッションを追加する
    /// </summary>
    private void AddMission()
    {
        // 最大数に達していたら追加しない
        if (missions.Count >= missionMax) { return; }

        // 作成
        Mission mission = null;

        // 処理の登録
        if (lockedWeaponKinds.Count > 0)
        {// 未開放の武器がある場合
            mission = new Mission();
            mission.ui = missionUis.Generate(Vector3.zero);
            mission.ui.transform.SetParent(missionParent.transform, false);

            // 条件の設定
            //
            // 倒す敵の種類
            var enemyKind = (Mission.EnemyKind)Usual.RandomInt((int)Mission.EnemyKind.Straight, (int)Mission.EnemyKind.All);
            //
            // 倒す敵の数
            var destroyCount = (uint)targetDestroyCountRange.RandInt;
            //
            // 条件式の設定
            mission.condition = () =>
            {
                return (mission.GetDestroiedCount(enemyKind) >= destroyCount);
            };
            //
            // 進捗率取得処理の登録
            mission.getRatio = () =>
            {
                return (float)mission.GetDestroiedCount(enemyKind) / destroyCount;
            };
            //
            // UIへ反映
            mission.ui.TargetIcon = enemyIcons[enemyKind];
            mission.ui.Count = destroyCount;

            // 解放武器の選択
            var unlockWeaponNode = Usual.RandomNode(lockedWeaponKinds);
            var unlockWeaponKind = unlockWeaponNode.Value;
            //
            // 候補から削除
            lockedWeaponKinds.Remove(unlockWeaponNode);

            // 報酬の設定
#if UNITY_EDITOR
            mission.earnReward = () => 
            {
                Debug.Log($"[達成 => {mission.conditionText}] [解放 => {rewardNames[unlockWeaponKind]}]");
            };
#endif
            if (IsMainWeapon(unlockWeaponKind))
            {// メイン武器の場合
                // キャスト
                var mainKind = (MainWeaponKind)unlockWeaponKind;

                // 処理の登録
                mission.earnReward += () =>
                {
                    Unlock(mainKind);
                };

                // UIへ反映
                var status = mainWeapons.statuses[mainKind];
                mission.ui.RewardIcon.sprite = status.Ui.sprite;
                mission.ui.RewardIcon.color  = status.Color;
                mission.ui.iconTarget        = status.Ui.gameObject;
            }
            else
            {// サブ武器の場合
                // キャスト
                var subKind = (SubWeaponKind)unlockWeaponKind;

                // 処理の登録
                var status = subWeapons[subKind];
                mission.earnReward += () =>
                {
                    Unlock(subKind);
                    
                    status.stockCount = 1;
                    status.Ui.Text.text = status.stockCount.ToString();
                };

                // UIへ反映
                mission.ui.RewardIcon.sprite = status.Ui.Icon.sprite;
                mission.ui.RewardIcon.color  = status.Color;
                mission.ui.iconTarget        = status.Ui.Icon.gameObject;
            }
            
            // 条件文の生成
            mission.conditionText = $"{enemyNames[enemyKind]} を {destroyCount}体 撃破せよ";

#if UNITY_EDITOR
            Debug.Log($"[発生 => {mission.conditionText}] [報酬 => {rewardNames[unlockWeaponKind]}]");
#endif
        }

        // 一覧へ登録
        if (mission != null)
        {// ミッションが作成された場合
            // 退場終了時に報酬が貰えるようにする
            mission.ui.onExited = () => { mission.earnReward(); };

            // UIの移動
            mission.ui.StayPos = GetMissionUIPos(mission.ui, missions.Count);
            mission.ui.Ratio = 0.0f;
            mission.ui.ToApproach();

            // 一覧へ登録
            missions.AddLast(mission);
        }
    }

    private Vector3 GetMissionUIPos(MissionUI ui, int index)
    {
        return missionUiBasePos + Vector3.right * (ui.Width + missionUiBlank) * index;
    }

    /// <summary>
    /// 武器アイコンを更新する
    /// </summary>
    private void UpdateWeaponIcon()
    {
        weaponIcon.sprite = mainWeapons.SelectedWeaponStatus.Ui.sprite;
        weaponIcon.color  = mainWeapons.SelectedWeaponStatus.pool.prefabs[0].Color;
    }

    /// <summary>
    /// 操作を伴う挙動
    /// </summary>
    private void Operate()
    {
        // 死亡したら操作できない
        if (IsDead) { return; }

        // 移動
        Move();

        // 武器発射
        Shot();

        // サブ武器使用
        UseSubWeapon();

        // 武器変更受付
        ChangeMainWeapon();
    }

    /// <summary>
    /// サブ武器を使う
    /// </summary>
    private void UseSubWeapon()
    {
        foreach (var subWeaponStatus in subWeapons)
        {// サブ武器一覧のループ
            // 情報の取得
            var status = subWeaponStatus.Value;
            
            // 未開放の無視
            if (!status.isUnlocked) { continue; }

            // 溜め
            if (status.stockCount >= StockMax)
            {// ストックが満タンの場合
                if (status.Charging())
                {// 最大になった瞬間
                    AudioManager.Instance.PlaySE(chargedSE);
                }
            }

            // 生成
            if (status.stockCount > 0)
            {// ストックがある場合
                if (Input.GetKeyDown(status.useButton))
                {// ボタンが押された場合
                    // 武器の生成
                    var weapon = status.pool.Generate(transform.position);
                    weapon.damageBuff = buffs.damage.value;

                    // ストック数減少
                    --status.stockCount;
                    status.Ui.Text.text = status.stockCount.ToString();

                    // 溜めの反映
                    var subWeapon = (SubWeapon)weapon;
                    subWeapon.ChargeRatio = status.ChargeRatio;

                    // 溜めリセット
                    status.ResetCharge();

                    // 演出
                    switch (weapon.Kind)
                    {
                        case WeaponKind.Beam:
                            // カメラ振動
                            Camera.Instance.Vibrate(0.1f, 1.5f);

                            // 効果音再生
                            AudioManager.Instance.PlaySE(beamSE);
                            break;

                        case WeaponKind.Bomb:
                            // 効果音再生
                            AudioManager.Instance.PlaySE(bombSE);
                            break;
                    }
                }
            }

            // リキャスト
            if (status.stockCount < StockMax)
            {// ストックに空きがある場合
                RecastSubWeapon(status);
            }
        }
    }

    /// <summary>
    /// サブ武器のリキャスト
    /// </summary>
    private void RecastSubWeapon(SubWeaponStatus status)
    {
        if (status.toShot.Advance())
        {// 指定秒数が経過した場合
            // ストック数増加
            ++status.stockCount;

            // UIに反映
            status.Ui.Text.text = status.stockCount.ToString();

            // 効果音再生
            AudioManager.Instance.PlaySE(recastedSE);
        }

        // 溜め具合をUIに反映
        var fillAmount = 1.0f;
        if (status.stockCount < StockMax)
        {// ストックが最大ではない場合
            fillAmount = status.toShot.Ratio;
        }
        status.Ui.Icon.fillAmount = fillAmount;
    }

    /// <summary>
    /// 上下移動
    /// </summary>
    private void Move()
    {
        // 上移動
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }

        // 下移動
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }

        // 範囲制限
        var pos = transform.position;
        if (pos.y > moveLimit)
        {// 上に行き過ぎた
            pos.y = moveLimit;
            transform.position = pos;
        }
        else if (pos.y < -moveLimit)
        {// 下に行き過ぎた
            pos.y = -moveLimit;
            transform.position = pos;
        }
    }

    /// <summary>
    /// 弾発射の制御
    /// </summary>
    private void Shot()
    {
        // メイン武器の情報
        var mainStatus = mainWeapons.SelectedWeaponStatus;

        // 弾が未解放なら無視
        if (!mainStatus.isUnlocked) { return; }

        // 溜め発射
        if (chargeStatus.isCharging)
        {// 溜めている場合
            // 溜める
            if (chargeStatus.timer.Advance())
            {// 溜め切った瞬間
                AudioManager.Instance.PlaySE(chargedSE);
            }
            
            // 音のループ
            if (chargeAudioSource.time >= chargeAudioLoopEnd)
            {
                chargeAudioSource.time = chargeAudioLoopBegin;
            }

            // 発射
            if (Input.GetKeyUp(chargeStatus.key))
            {// 離された瞬間
                // 生成
                OnShotBullet();
            }
        }

        // 連射
        else
        {// 溜めていない場合
            // 連射
            if (mainStatus.toShot.Advance())
            {// 打つタイミングになった場合
                OnShotBullet();
                mainStatus.ReducationInterval(buffs.rapidFire);
            }

            // 溜め移行
            if (Input.GetKeyDown(chargeStatus.key))
            {// 押された瞬間
                // エディタ情報の反映
                chargeStatus.timer.Interval = chargeStatus.timer.Interval;

                // 計測開始
                chargeStatus.timer.Start();

                // 音の開始
                chargeAudioSource.time = 0.0f;
                chargeAudioSource.Play();

                // 溜め状態にする
                chargeStatus.isCharging = true;
            }
        }
    }

    /// <summary>
    /// 弾発射時
    /// </summary>
    private void OnShotBullet()
    {
        // 生成
        var wayCount = Mathf.FloorToInt(buffs.way.value);
        var bulletCount = 1 + wayCount * 2;  // 弾の数
        var angleMin = -bulletAngle * wayCount;   // 弾同士の角度の幅
        for (int i = 0; i < bulletCount; ++i)
        {
            // 移動角度を求める
            Degree moveAngle = angleMin + bulletAngle * i;

            // 生成
            var bullet = (Bullet)mainWeapons.SelectedWeaponStatus.pool.Generate(transform.position + moveAngle.XY * toBulletDistance);

            // 角度の設定
            bullet.Angle = moveAngle;

            // ダメージの設定
            bullet.damageBuff = buffs.damage.value;

            // ステータスの設定
            if (chargeStatus.isCharging)
            {// 溜めている場合
                // 貫通
                bullet.isCharged = true;

                // ダメージ倍率の上乗せ
                bullet.damageBuff += chargeStatus.damageBuff * chargeStatus.timer.Ratio;

                // でかくする
                bullet.transform.localScale *= 1.0f + chargeStatus.timer.Ratio;

                // 無限追尾対策
                if (mainWeapons.selectedKind == MainWeaponKind.Chase)
                {// 追尾弾を選んでいる場合
                    // 貫通しない
                    bullet.isCharged = false;

                    // さらにダメージ倍率上乗せ
                    bullet.damageBuff += chargedChaseBuff;
                }
            }
        }

        // ステータスの設定
        if (chargeStatus.isCharging)
        {// 溜めている場合
            // 溜め終了
            chargeStatus.isCharging = false;

            // カメラ振動
            Camera.Instance.Vibrate(0.025f * chargeStatus.timer.Ratio, 0.5f);

            // 効果音再生
            var audio = AudioManager.Instance;
            audio.PlaySE(chargeShotSE);
            if (chargeStatus.timer.Ratio >= 1.0f)
            {
                audio.PlaySE(maxChargeShotSE); ;
            }

            // タイマーリセット
            mainWeapons.SelectedWeaponStatus.toShot.Reset();
            chargeStatus.timer.Reset();

            // 溜め音停止
            chargeAudioSource.Stop();
        }

        // 効果音再生
        AudioManager.Instance.PlaySE(mainWeapons.SelectedWeaponStatus.ShottedSE);
    }

    /// <summary>
    /// メイン武器変更
    /// </summary>
    private void ChangeMainWeapon()
    {
        // 操作受付
        var moveValue = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {// 左入力
            moveValue = (int)MainWeaponKind.Count - 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {// 右入力
            moveValue = 1;
        }

        // 操作されなかった
        if (moveValue == 0) { return; }

        // 有効な武器を探す
        var changedKind = GetMovedMainWeaponKind(moveValue);

        // 武器変更
        if (mainWeapons.selectedKind != changedKind)
        {// 武器が変わった場合
            // 種類の変更
            mainWeapons.selectedKind = changedKind;

            // カーソル移動
            weaponCursor.transform.position = mainWeapons.SelectedWeaponStatus.Ui.transform.position;

            // サウンド再生
            AudioManager.Instance.PlaySE(selectedWeaponSE);

            // アイコン更新
            UpdateWeaponIcon();

            // 溜めゲージの色変更
            chargeBar.color = mainWeapons.SelectedWeaponStatus.pool.prefabs[0].Color;
        }
    }

    private MainWeaponKind GetMovedMainWeaponKind(int moveValue)
    {
        var prevKind = mainWeapons.selectedKind;
        var kind = prevKind;
        do
        {// 種類を一周するまで
            // 次の武器へ移動
            kind = (MainWeaponKind)(((int)kind + moveValue) % (int)MainWeaponKind.Count);

            // 移動終了
            if (mainWeapons.statuses[kind].isUnlocked)
            {// 解放されている武器の場合
                return kind;
            }
        } while (kind != prevKind);

        // 移動失敗
        return mainWeapons.selectedKind;
    }

    #region 被弾
    private void OnCollisionEnter2D(Collision2D other)
    {
        switch(other.gameObject.tag)
        {// タグによる分岐
            case Tags.Enemy:
                TakeDamage(contactDamage);
                break;

            case Tags.EnemyWeapon:
                var weapon = other.gameObject.GetComponent<Weapon>();
                TakeDamage(weapon.Damage);
                break;

            case Tags.Item:
                var item = other.gameObject.GetComponent<Item>();
                OnCatchedItem(item);
                break;
        }
    }

    /// <summary>
    /// アイテム獲得時
    /// </summary>
    /// <param name="item">獲得したアイテム</param>
    private void OnCatchedItem(Item item)
    {
        // 演出
        toItemCatchedReactionEnd.Interval = toItemCatchedReactionEnd.Interval;
        toItemCatchedReactionEnd.Restart();
        catchedItemColor = item.Color;

        // 数値への反映
        switch (item.Kind)
        {// 種類による分岐
            case Item.Kinds.Damage:
                buffs.damage += item.RiseAmount;
                damageBar.fillAmount = buffs.damage.Ratio;
                break;

            case Item.Kinds.RapidFire:
                buffs.rapidFire += item.RiseAmount;
                rapidFireBar.fillAmount = buffs.rapidFire.Ratio;
                break;

            case Item.Kinds.Way:
                buffs.way += item.RiseAmount;
                wayBar.fillAmount = buffs.way.Ratio;
                break;

            case Item.Kinds.Life:
                // 上限アップ
                buffs.life += item.RiseAmount;
                lifeMax = defaultLifeMax + buffs.life.value;
                lifeBar.LengthScale = lifeMax / defaultLifeMax;
                lifeAddBar.fillAmount = buffs.life.Ratio;

                // アップ分回復
                lifeNow += item.RiseAmount;
                break;

            case Item.Kinds.Heal:
                // 回復
                lifeNow += item.RiseAmount;

                // 上限の反映
                lifeNow = Mathf.Clamp(lifeNow, 0.0f, lifeMax);
                break;
        }

        // アイテム削除
        item.ToUnused();

        // 効果音再生
        AudioManager.Instance.PlaySE(catchedItemSE);
    }

    /// <summary>
    /// アイテム獲得リアクション
    /// </summary>
    private void ItemCatchedReaction()
    {
        // 死亡中は無視
        if (IsDead) { return; }

        // リアクション
        if (toItemCatchedReactionEnd.IsCounting)
        {// 演出中の場合
            Renderer.color = Color.Lerp(catchedItemColor, initialColor, toItemCatchedReactionEnd.Ratio);
            toItemCatchedReactionEnd.Advance();
        }
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    public void TakeDamage(float damage)
    {
        // 死んだら無視
        if (IsDead) { return; }

        // カメラ振動
        Camera.Instance.Vibrate(0.05f, 0.5f);

        // 効果音再生
        AudioManager.Instance.PlaySE(damagedSE);

        // 体力減少
        lifeNow -= damage;
        if (lifeNow < 0.0f)
        {// 死んだ場合
            Death();
        }
        else
        {// 死んでいない場合
            Reaction();
        }
    }

    /// <summary>
    /// 被弾
    /// </summary>
    private void Reaction()
    {
        // 色変更
        Renderer.color = damagedColor;

        // 一定時間後に元に戻す
        Invoke("ResetColor", reactionSecond);
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Death()
    {
        // 体力の補正
        lifeNow = 0.0f;

        // 死亡状態にする
        IsDead = true;

        // 透明にする
        Alpha = 0.0f;

        // エフェクト発生
        EffectManager.Instance.Spawn(transform.position);

        // カメラ振動
        Camera.Instance.Vibrate(0.5f, 1.5f);

        // ゲームオーバー移行
        Invoke("ToGameover", toGameoverSecond);
    }

    /// <summary>
    /// ゲームオーバー表示
    /// </summary>
    private void ToGameover()
    {
        Gameover.Instance.Call(Gameover.MenuType.Gameover);
    }

    private void ResetColor()
    {
        Renderer.color = initialColor;
    }
    #endregion
}
