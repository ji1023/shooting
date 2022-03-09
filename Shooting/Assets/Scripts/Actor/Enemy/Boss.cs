using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BeamPool : Pool<Beam> { }

public class Boss : Enemy
{
    [System.Serializable]
    private class Buff<T>
    {
        /// <summary>
        /// 体力の減少によるバフ
        /// </summary>
        [SerializeField]
        protected T life = default;

        /// <summary>
        /// ステージレベルによるバフ
        /// </summary>
        [SerializeField]
        protected T level = default;

        public Buff(T life, T level)
        {
            this.life  = life;
            this.level = level;
        }
    }

    [System.Serializable]
    private class AddBuff : Buff<float>
    {
        public float GetValue(float baseValue, Ratio lifeRatio, Ratio levelRatio)
        {
            return baseValue * (1.0f + life * lifeRatio + level * levelRatio);
        }
        public AddBuff(float life, float level) : 
            base(life, level)
        {

        }
    }

    [System.Serializable]
    private class SubBuff : Buff<Ratio>
    {
        public float GetValue(float baseValue, Ratio lifeRatio, Ratio levelRatio)
        {
            var lifeBuffed = baseValue * (1.0f - life * lifeRatio);
            return lifeBuffed * (1.0f - level * levelRatio);
        }
        public SubBuff(Ratio life, Ratio level) :
            base(life, level)
        {

        }
    }

    /// <summary>
    /// レベルによる体力の増加倍率
    /// </summary>
    [SerializeField]
    private float lifeBuff = 1.0f;

    #region 状態関連
    /// <summary>
    /// 状態
    /// </summary>
    private enum State
    {
        Wait,
        Approach,       // 登場
        MoveWait,       // 左右移動待機
        BeamShot,       // ビーム発射
        MoveShot,       // 移動しながら直進弾
        Summon,         // 雑魚召喚
        AimShot,        // 止まって狙い撃ち
    }

    /// <summary>
    /// 状態
    /// </summary>
    private StateManager<State> states = new StateManager<State>();
    
    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviourTimer;
    #endregion
    
    #region 登場関連
    [Header("登場関連")]
    /// <summary>
    /// 登場に要する秒数
    /// </summary>
    [SerializeField]
    private float approachingSecond = 5.0f;

    /// <summary>
    /// 登場終了時の座標
    /// </summary>
    [SerializeField]
    private Vector3 approachEndPos = Vector3.zero;

    /// <summary>
    /// 登場開始時の処理
    /// </summary>
    private Vector3 approachBeginPos;
    #endregion

    #region 移動関連
    [Header("移動関連")]
    [SerializeField]
    private float moveSecond = 3.0f;

    /// <summary>
    /// 移動秒数の削減率
    /// </summary>
    [SerializeField]
    private SubBuff moveSecondBuff = new SubBuff(0.5f, 0.3f);

    [SerializeField]
    private float limitDistance = 2.5f;
    private Vector3 leftLimitPos;
    private Vector3 rightLimitPos;
    private float directionFlipper = 1.0f;
    #endregion

    #region 射撃関連
    [Header("射撃関連")]
    /// <summary>
    /// 弾一覧
    /// </summary>
    [SerializeField]
    private BulletPool bullets = new BulletPool();

    /// <summary>
    /// 狙い撃ちの回数
    /// </summary>
    [SerializeField]
    private uint aimShotCount = 3;

    /// <summary>
    /// 狙い撃ち回数の増加倍率
    /// </summary>
    [SerializeField]
    private AddBuff aimShotCountBuff = new AddBuff(2.0f, 3.0f);

    /// <summary>
    /// 移動撃ちの回数
    /// </summary>
    [SerializeField]
    private uint moveShotCount = 10;
    
    /// <summary>
    /// 移動撃ち回数の増加倍率
    /// </summary>
    [SerializeField]
    private AddBuff moveShotCountBuff = new AddBuff(1.0f, 2.0f);

    /// <summary>
    /// 撃った回数
    /// </summary>
    private uint shotedCounter = 0;

    /// <summary>
    /// 狙い撃ちの間隔
    /// </summary>
    [SerializeField]
    private float aimShotInterval = 0.5f;

    /// <summary>
    /// 狙い撃ちの間隔の削減率
    /// </summary>
    [SerializeField]
    private SubBuff aimShotIntervalBuff = new SubBuff(0.5f, 0.5f);

    /// <summary>
    /// 移動撃ちの間隔
    /// </summary>
    [SerializeField]
    private float moveShotInterval = 0.25f;

    /// <summary>
    /// 移動撃ちの間隔の削減率
    /// </summary>
    [SerializeField]
    private SubBuff moveShotIntervalBuff = new SubBuff(0.5f, 0.7f);

    /// <summary>
    /// 移動撃ち中の移動速度
    /// </summary>
    [SerializeField]
    private float moveShotingSpeed = 6.0f;

    /// <summary>
    /// 弾が発生する座標一覧
    /// </summary>
    [SerializeField]
    private GameObject[] shotPoses = new GameObject[2];
    #endregion

    #region ビーム関連
    [Header("ビーム関連")]
    /// <summary>
    /// ビーム一覧
    /// </summary>
    [SerializeField]
    private BeamPool beams = new BeamPool();

    /// <summary>
    /// ビームの発射位置
    /// </summary>
    [SerializeField]
    private GameObject beamShotPos = null;

    /// <summary>
    /// ビーム火力調整
    /// </summary>
    [SerializeField, Range(0.0f, 1.0f)]
    private float beamDamageDebuff = 0.5f;

    /// <summary>
    /// ビームのダメージ増加倍率
    /// </summary>
    [SerializeField]
    private AddBuff beamDamageBuff = new AddBuff(0.5f, 2.5f);

    /// <summary>
    /// ビーム発射後の硬直時間
    /// </summary>
    [SerializeField]
    private float beamShotedWaitSecond = 1.5f;

    /// <summary>
    /// 硬直時間の削減率
    /// </summary>
    [SerializeField]
    private SubBuff beamShotedWaitSecondBuff = new SubBuff(0.75f, 0.5f);

    /// <summary>
    /// ビーム発射時の音
    /// </summary>
    [SerializeField]
    private List<AudioClip> beamSes = new List<AudioClip>();
    #endregion

    #region 召喚関連
    [Header("召喚関連")]
    /// <summary>
    /// 召喚数
    /// </summary>
    [SerializeField]
    private uint summonCount = 2;

    /// <summary>
    /// 召喚数の増加倍率
    /// </summary>
    [SerializeField]
    private AddBuff summonCountBuff = new AddBuff(2.0f, 2.0f);

    /// <summary>
    /// 召喚後の硬直時間
    /// </summary>
    [SerializeField]
    private float summonedWaitSecond = 3.0f;

    /// <summary>
    /// 硬直時間の削減率
    /// </summary>
    [SerializeField]
    private SubBuff summonedWaitSecondBuff = new SubBuff(0.5f, 0.5f);

    /// <summary>
    /// 召喚する敵との距離
    /// </summary>
    [SerializeField]
    private float summonDistance = 10.0f;

    /// <summary>
    /// 召喚する位置の角度
    /// </summary>
    [SerializeField]
    private Degree summonAngle = 90.0f;

    /// <summary>
    /// 召喚する敵の候補一覧
    /// </summary>
    [SerializeField]
    private List<Enemy> summonEnemies = new List<Enemy>();
    #endregion

    #region UI関連
    [Header("UI関連")]
    /// <summary>
    /// 体力バー
    /// </summary>
    [SerializeField]
    private Bar lifeBar = null;
    #endregion
    
    /// <summary>
    /// 最初の体力の最大値
    /// </summary>
    private float initialLifeMax;

    /// <summary>
    /// 登場中か否か
    /// </summary>
    public bool IsApproaching { get; private set; } = false;

    /// <summary>
    /// 登場済みか否か
    /// </summary>
    public bool IsApproached { get; private set; } = false;

    /// <summary>
    /// 倒されたか否か
    /// </summary>
    public bool IsDestroied { get; private set; } = false;
    
    /// <summary>
    /// 開始
    /// </summary>
    public void OnStart()
    {
        // 復活
        IsDestroied = false;

        // 体力設定
        lifeMax = initialLifeMax * (1.0f + lifeBuff);
        ResetLife();

        // UIを隠す
        lifeBar.LengthScale = 0.0f;

        // 状態の設定
        states.NowStateCode = State.Approach;

        // プール初期化
        beams  .AllToUnused();
        bullets.AllToUnused();

        // 動かない
        ToUnused();
    }

    /// <summary>
    /// 発生させる
    /// </summary>
    public void Spawn()
    {
        OnStart();
        ToUsed();
    }

    // Use this for initialization
    void Start()
    {
        // 初期化
        OnInstantiated();

        // 体力の保存
        initialLifeMax = lifeMax;

        // 弾の色を変える処理の登録
        bullets.beforeOnInstantiated = (Bullet bullet) =>
        {
            bullet.Color = EnemyManager.Instance.BulletColor;
            bullet.gameObject.tag = Tags.EnemyWeapon;
        };
        beams.beforeOnInstantiated = (Beam beam) =>
        {
            beam.Color = EnemyManager.Instance.BulletColor;
            beam.gameObject.tag = Tags.EnemyWeapon;
        };

        // 状態の作成
        // 
        //  登場
        var approach = states.AddState(State.Approach, Approach);
        approach.AddTransition(State.MoveWait, ()=> { return behaviourTimer.IsTermination; });
        approach.onTransitioned = () => 
        {
            IsApproaching = true;
            isInvisible = true;
            behaviourTimer.Restart(approachingSecond);
            transform.position = approachBeginPos;
        };
        approach.onFinished = () =>
        {
            IsApproaching = false;
            IsApproached = true;
            isInvisible = false;
            transform.position = approachEndPos;
        };
        //
        //  移動
        var move = states.AddState(State.MoveWait, HorizontalMove);
        move.AddTransition(State.AimShot, ()=> { return behaviourTimer.Advance(); });
        move.onTransitioned = () => 
        {
            speed = moveSecond;
            behaviourTimer.Restart(moveSecondBuff.GetValue(moveSecond, LifeRatio.Reversed, StageManager.Instance.LevelRatio));
        };
        //
        //  狙い撃ち
        var aimShot = states.AddState(State.AimShot, AimShot);
        aimShot.AddTransition(State.MoveShot, ()=> 
        {
            var count = (int)aimShotCountBuff.GetValue(aimShotCount, LifeRatio.Reversed, StageManager.Instance.LevelRatio);
            return shotedCounter >= count;
        });
        aimShot.onTransitioned = () => 
        {
            shotedCounter = 0;
            behaviourTimer.isLooping = true;
            behaviourTimer.Restart(aimShotIntervalBuff.GetValue(aimShotInterval, LifeRatio.Reversed, StageManager.Instance.LevelRatio));
        };
        aimShot.onFinished = () => 
        {
            behaviourTimer.isLooping = false;
        };
        //
        //  移動撃ち
        var moveShot = states.AddState(State.MoveShot, MoveShot);
        moveShot.AddTransition(State.BeamShot, ()=> 
        {
            var count = (int)moveShotCountBuff.GetValue(moveShotCount, LifeRatio.Reversed, StageManager.Instance.LevelRatio);
            return shotedCounter >= moveShotCount;
        });
        moveShot.onTransitioned = () =>
        {
            speed = moveShotingSpeed;
            shotedCounter = 0;
            behaviourTimer.isLooping = true;
            behaviourTimer.Restart(moveShotIntervalBuff.GetValue(moveShotInterval, LifeRatio.Reversed, StageManager.Instance.LevelRatio));
        };
        moveShot.onFinished = () =>
        {
            behaviourTimer.isLooping = false;
        };
        //
        //  ビーム発射
        var beamShot = states.AddState(State.BeamShot, ()=> { });
        beamShot.AddTransition(State.Summon, ()=> { return behaviourTimer.Advance(); });
        beamShot.onTransitioned = BeamShot;
        //
        // 召喚
        var summon = states.AddState(State.Summon, ()=> { });
        summon.AddTransition(State.MoveWait, ()=> { return behaviourTimer.Advance(); });
        summon.onTransitioned = Summon;

        // 左を向く
        Angle = Degree.MID;

        // 初期座標の保存
        approachBeginPos = transform.position;

        // 限界座標を求める
        leftLimitPos  = approachEndPos + Left  * limitDistance;
        rightLimitPos = approachEndPos + Right * limitDistance;

        // 体力バーを隠す
        lifeBar.LengthScale = 0.0f;

        // 開始
        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        states.Behave();
        FlipDirection();
        lifeBar.Ratio = LifeRatio;
    }

    protected override void OnDead()
    {
        IsDestroied = true;
        Gameover.Instance.Call(Gameover.MenuType.Clear);
    }

    /// <summary>
    /// 水平移動
    /// </summary>
    private void HorizontalMove()
    {
        transform.position += Left * directionFlipper * SpeedPerFrame;
    }
    
    /// <summary>
    /// 狙い撃ち
    /// </summary>
    private void AimShot()
    {
        if (behaviourTimer.Advance())
        {
            // 発射数加算
            ++shotedCounter;

            // 弾の生成
            var bullet = bullets.Generate(shotPoses[shotedCounter % 2].transform.position);

            // プレイヤーを狙う
            bullet.LookAt(Player.Instance.transform.position);

            // 効果音再生
            AudioManager.Instance.PlaySE(EnemyManager.Instance.ShottedSE);
        }
    }

    /// <summary>
    /// 動きながら撃つ
    /// </summary>
    private void MoveShot()
    {
        // 左右移動
        HorizontalMove();

        // 弾の発射
        if (behaviourTimer.Advance())
        {
            // 発射数加算
            ++shotedCounter;

            // 弾の生成
            var bullet = bullets.Generate(shotPoses[shotedCounter % 2].transform.position);
            
            // 直進
            bullet.Angle = Angle;

            // 効果音再生
            AudioManager.Instance.PlaySE(EnemyManager.Instance.ShottedSE);
        }
    }

    /// <summary>
    /// ビーム発射
    /// </summary>
    private void BeamShot()
    {
        // タイマー設定
        var levelRatio = StageManager.Instance.LevelRatio;
        behaviourTimer.Restart(beamShotedWaitSecondBuff.GetValue(beamShotedWaitSecond, LifeRatio.Reversed, levelRatio));

        // ビーム作成
        var beam = beams.Generate(beamShotPos.transform.position);
        beam.clingObject = beamShotPos;
        beam.Angle = Angle + 90.0f;
        beam.damageBuff = beamDamageDebuff + beamDamageBuff.GetValue(1.0f, LifeRatio.Reversed, levelRatio);
        beam.ChargeRatio = levelRatio;

        // カメラ振動
        Camera.Instance.Vibrate(0.1f, 1.5f);

        // 効果音再生
        var audio = AudioManager.Instance;
        foreach (var beamSE in beamSes)
        {
            audio.PlaySE(beamSE);
        }
    }

    /// <summary>
    /// 召喚
    /// </summary>
    private void Summon()
    {
        // タイマー設定
        var levelRatio = StageManager.Instance.LevelRatio;
        behaviourTimer.Restart(summonedWaitSecondBuff.GetValue(summonedWaitSecond, LifeRatio.Reversed, levelRatio));

        // 敵の生成
        var count = (int)summonCountBuff.GetValue(summonCount, LifeRatio.Reversed, levelRatio);
        var angleAdd = summonAngle / (count + 1);
        var angle = Angle - (summonAngle * 0.5f) + angleAdd;
        for (int i = 0; i < count; ++i)
        {// 生成数のループ
            // 生成
            var enemy = Instantiate(Usual.RandomElem(summonEnemies));

            // 管理下に置く
            enemy.OnInstantiated();
            enemy.OnGenerated();
            EnemyManager.Instance.Add(enemy);

            // 座標の設定
            enemy.transform.position = transform.position + angle.XY * summonDistance;

            // 角度の設定
            enemy.Angle = Angle;

            // 角度の更新
            angle += angleAdd;
        }
    }

    /// <summary>
    /// 登場
    /// </summary>
    private void Approach()
    {
        behaviourTimer.Advance();
        var ratio = Easing.Quad.Out(behaviourTimer.Ratio);
        transform.position = Vector3.Lerp(approachBeginPos, approachEndPos, ratio);
        lifeBar.LengthScale = ratio;
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    private void FlipDirection()
    {
        if (directionFlipper == 1.0f)
        {// 左向きの場合
            var toLimitPos = leftLimitPos - transform.position;
            var dot = Vector3.Dot(Left, toLimitPos);
            
            // 方向転換
            if (dot < 0.0f)
            {// 限界座標を超えていた場合
                transform.position = leftLimitPos;
                directionFlipper *= -1.0f;
            }
        }
        else
        {// 右向きの場合
            var toLimitPos = rightLimitPos - transform.position;
            var dot = Vector3.Dot(Right, toLimitPos);

            // 方向転換
            if (dot < 0.0f)
            {// 限界座標を超えていた場合
                transform.position = rightLimitPos;
                directionFlipper *= -1.0f;
            }
        }
    }
}
