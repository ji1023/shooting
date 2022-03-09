using UnityEngine;
using System.Collections;

public class Bomb : SubWeapon
{
    /// <summary>
    /// 種類
    /// </summary>
    public override Player.WeaponKind Kind => Player.WeaponKind.Bomb;

    /// <summary>
    /// 移動終了までの時間
    /// </summary>
    [SerializeField]
    private float toMoveEndSecond = 0.5f;

    /// <summary>
    /// 爆発時の音
    /// </summary>
    [SerializeField]
    private AudioClip explosionedSE = null;

    /// <summary>
    /// 移動終了までの時間
    /// </summary>
    private Timer toMoveEnd;

    /// <summary>
    /// 爆風
    /// </summary>
    private Blast blast = null;

    /// <summary>
    /// 発生座標
    /// </summary>
    private Vector3 spawnedPos;

    /// <summary>
    /// 目標座標
    /// </summary>
    private Vector3 targetPos;

    /// <summary>
    /// 移動角度の振れ幅
    /// </summary>
    [SerializeField]
    private Limit moveAngleRange = new Limit(-20.0f, 20.0f);

    /// <summary>
    /// 移動距離の振れ幅
    /// </summary>
    [SerializeField]
    private Limit moveDistanceRange = new Limit(3.0f, 6.0f);

    /// <summary>
    /// 爆発済みか否か
    /// </summary>
    private bool isExplosioned = false;

    /// <summary>
    /// 溜め具合
    /// </summary>
    public override float ChargeRatio
    {
        set
        {
            targetPos = spawnedPos + Direction * moveDistanceRange.Lerp(value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (toMoveEnd.IsCounting)
        {// 移動中の場合
            if (toMoveEnd.Advance())
            {// 移動が終わった場合
                // 座標の修正
                transform.position = targetPos;
                return;
            }

            // 座標の更新
            transform.position = Vector3.Lerp(spawnedPos, targetPos, Easing.Quad.Out(toMoveEnd.Ratio));
        }
    }

    /// <summary>
    /// 爆発
    /// </summary>
    private void Explosion()
    {
        // 爆発済みなら無視
        if (isExplosioned) { return; }

        // 爆風の有効化
        blast.ToUsed();

        // 見えなくする
        Alpha = 0.0f;

        // カメラ振動
        Camera.Instance.Vibrate(0.2f, 1.0f);

        // SE再生
        AudioManager.Instance.PlaySE(explosionedSE);

        // 爆発済みにする
        isExplosioned = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        switch (other.gameObject.tag)
        {
            case Tags.Enemy:
                // 敵情報の取得
                var enemy = other.gameObject.GetComponent<Enemy>();

                // 爆弾以外の弱点を持つ敵の無視
                if (enemy.Weakness != null && enemy.Weakness != Player.WeaponKind.Bomb) { return; }

                // 爆発
                Explosion();
                break;
        }
    }

    public override void OnGenerated()
    {
        base.OnGenerated();

        // 爆発していない
        isExplosioned = false;

        // 透明度を戻す
        Alpha = 1.0f;

        // 爆風の無効化
        blast.ToUnused();

        // タイマー設定
        toMoveEnd.Interval = toMoveEndSecond;
        toMoveEnd.Restart();

        // 座標の記憶
        spawnedPos = transform.position;

        // 目標座標の設定
        targetPos = spawnedPos + Direction * moveDistanceRange.min;

        // 爆風の事前処理
        blast.OnGenerated();
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();

        // まとめる
        transform.parent = Player.Instance.BulletsAggregator.transform;

        // 攻撃判定なし
        IsAttacking = false;

        // 爆風の取得
        blast = transform.GetChild(0).gameObject.GetComponent<Blast>();
        blast.OnInstantiated();
    }
}
