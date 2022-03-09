using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyPool : Pool<Enemy> { }

public class EnemyManager : SingletonMonoBehaviour<EnemyManager>
{
    /// <summary>
    /// 弱点になる武器一覧
    /// </summary>
    [SerializeField]
    private List<Weapon> weaponPrefabs = new List<Weapon>();

    /// <summary>
    /// 敵のプール
    /// </summary>
    [SerializeField]
    private EnemyPool enemies = new EnemyPool();

    /// <summary>
    /// 敵発生までの秒数
    /// </summary>
    [SerializeField]
    private float toSpawnSecond = 1.0f;

    /// <summary>
    /// 敵発生タイマー
    /// </summary>
    private Timer toSpawn = new Timer(true, true);

    /// <summary>
    /// 同時に存在していい敵の最大数の基準値
    /// </summary>
    [SerializeField]
    private uint enemyCountMaxDefault = 5;

    /// <summary>
    /// レベルによる敵の最大数の増加率
    /// </summary>
    [SerializeField]
    private float enemyCountMaxLevelBuff = 3.0f;

    /// <summary>
    /// 進行度による敵の最大数の増加率
    /// </summary>
    [SerializeField]
    private float enemyCountMaxProgessBuff = 2.0f;

    /// <summary>
    /// 同時に存在していい敵の最大数
    /// </summary>
    public uint EnemyCountMax
    {
        get
        {
            return (uint)
                    (
                        enemyCountMaxDefault * 
                        (
                            1.0f + 
                            enemyCountMaxLevelBuff * StageManager.Instance.LevelRatio + 
                            enemyCountMaxProgessBuff * toBossSpawnSecond.Ratio
                        )
                    );
        }
    }

    /// <summary>
    /// 発生中の敵の数
    /// </summary>
    public uint EnemyCounter { get; private set; } = 0;

    /// <summary>
    /// 発生させるx座標
    /// </summary>
    [SerializeField]
    private float spawnX = -12.0f;

    /// <summary>
    /// 発生させるy座標の幅
    /// </summary>
    [SerializeField]
    private Limit spawnYRange = new Limit(-4.0f, 4.0f);

    /// <summary>
    /// ボス登場までの時間
    /// </summary>
    [SerializeField]
    private Timer toBossSpawnSecond = new Timer(false, true);

    /// <summary>
    /// 弱点持ちが発生する確率
    /// </summary>
    [SerializeField]
    private Percentage weaknessProbability = 10.0f;
    /// <summary>
    /// 弱点持ちが発生する確率
    /// </summary>
    public Percentage WeaknessProbability => weaknessProbability;

    /// <summary>
    /// 被弾時の色
    /// </summary>
    [SerializeField]
    private Color damagedColor = Color.red;
    /// <summary>
    /// 被弾時の色
    /// </summary>
    public Color DamagedColor => damagedColor;

    /// <summary>
    /// 色が変わり続ける秒数
    /// </summary>
    [SerializeField]
    private float reactionSecond = 0.5f;
    /// <summary>
    /// 色が変わり続ける秒数
    /// </summary>
    public float ReactionSecond => reactionSecond;
    
    [SerializeField]
    private Color bulletColor = Color.red;
    /// <summary>
    /// 弾の色
    /// </summary>
    public Color BulletColor => bulletColor;

    /// <summary>
    /// 弱点
    /// </summary>
    public Weapon Weekness
    {
        get
        {
            return Usual.RandomElem(weaponPrefabs);
        }
    }
    
    [SerializeField]
    private float weaknessBuff = 1.2f;
    /// <summary>
    /// 弱点特攻倍率
    /// </summary>
    public float WeaknessBuff => weaknessBuff;

    /// <summary>
    /// 新規作成する間隔
    /// </summary>
    [SerializeField]
    private uint instantiateCount = 10;

    /// <summary>
    /// 生成数
    /// </summary>
    private uint generationCounter = 0;
    
    [SerializeField]
    private Boss boss = null;
    /// <summary>
    /// ボス
    /// </summary>
    public Boss Boss => boss;

    /// <summary>
    /// ボスとの近さの割合
    /// </summary>
    public Ratio BossClosenessRatio => toBossSpawnSecond.Ratio;

    /// <summary>
    /// 進行度による発生間隔の削減率
    /// </summary>
    [SerializeField]
    private Ratio spawnSecondProgressBuff = 0.3f;

    /// <summary>
    /// レベルによる発生間隔の削減率
    /// </summary>
    [SerializeField]
    private Ratio spawnSecondLevelBuff = 0.5f;

    [SerializeField]
    private AudioClip damagedSE = null;
    /// <summary>
    /// 被弾時の音
    /// </summary>
    public AudioClip DamagedSE => damagedSE;

    [SerializeField]
    private AudioClip destroiedSE = null;
    /// <summary>
    /// 撃破時の音
    /// </summary>
    public AudioClip DestroiedSE => destroiedSE;

    [SerializeField]
    private AudioClip shottedSE = null;
    /// <summary>
    /// 発射時の音
    /// </summary>
    public AudioClip ShottedSE => shottedSE;
    
    /// <summary>
    /// 敵の総数を減らす
    /// </summary>
    public void SubEnemyCount()
    {
        if (EnemyCounter <= 0) { return; }
        --EnemyCounter;
#if UNITY_EDITOR
        Debug.Log($"敵消滅 {EnemyCounter}");
#endif
    }

    private void Start()
    {
        toBossSpawnSecond.Interval = toBossSpawnSecond.Interval;
    }

    // Update is called once per frame
    void Update()
    {
        // ステージ未開始なら発生させない
        if (!StageManager.Instance.IsStarted) { return; }
      
        // 敵を発生させる
        if (!toBossSpawnSecond.IsTermination)
        {// ボスが発生していない場合
            // 発生
            SpawnEnemy();

            // ボス発生
            if (toBossSpawnSecond.Advance())
            {// 秒数経過の場合
                boss.Spawn();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (toSpawn.Advance())
        {// 一定秒数ごと
            // 上限を設ける
            if (EnemyCounter >= EnemyCountMax) { return; }

            // 生成数加算
            ++generationCounter;

            // 総数加算
            ++EnemyCounter;
#if UNITY_EDITOR
            Debug.Log($"敵発生 {EnemyCounter}");
#endif

            // 生成
            var pos = new Vector2(spawnX, spawnYRange.Rand);
            if (generationCounter >= instantiateCount)
            {// 新規作成する場合
                enemies.Instantiate(pos);
            }
            else
            {// 新規作成しない場合
                enemies.Generate(pos);
            }

            // 発生間隔の更新
            var levelBuffedSecond = toSpawnSecond * (1.0f - spawnSecondLevelBuff * StageManager.Instance.LevelRatio);
            toSpawn.Interval = levelBuffedSecond * (1.0f - spawnSecondProgressBuff * toBossSpawnSecond.Ratio);
        }
    }

    /// <summary>
    /// 管理下にない敵を登録する
    /// </summary>
    /// <param name="enemy">登録する敵</param>
    public void Add(Enemy enemy)
    {
        // 登録
        enemy.transform.parent = transform;
        enemies.Add(enemy);
    }

    public void Foreach(EnemyPool.EventHandler proc)
    {
        enemies.Foreach(proc);
    }

    public void Restart()
    {
        enemies.AllToUnused();
        boss.OnStart();
        toSpawn.Restart(toSpawnSecond);
        toBossSpawnSecond.Restart();
    }
}
