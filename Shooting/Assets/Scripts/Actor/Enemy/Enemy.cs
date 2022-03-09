using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Actor
{
    /// <summary>
    /// 弱点
    /// </summary>
    public Player.WeaponKind? Weakness { get; protected set; }

    /// <summary>
    /// 体力の上限
    /// </summary>
    [SerializeField]
    protected float lifeMax = 1.0f;

    /// <summary>
    /// 体力の上限の逆数
    /// </summary>
    private float invLifeMax = 1.0f;

    /// <summary>
    /// 現在の体力
    /// </summary>
    private float life = 0.0f;

    /// <summary>
    /// 体力の割合
    /// </summary>
    public Ratio LifeRatio { get; private set; }

    /// <summary>
    /// ドロップするアイテムの候補一覧
    /// </summary>
    [SerializeField]
    private List<Item.Kinds> dropItemKinds = new List<Item.Kinds>();

    /// <summary>
    /// アイテムのドロップ数の幅
    /// </summary>
    [SerializeField]
    private Limit dropCountRange = new Limit(0.0f, 1.0f);

    /// <summary>
    /// アイテムをドロップする確率
    /// </summary>
    [SerializeField]
    private Percentage dropPercentage = 0.0f;

    /// <summary>
    /// 基準の色
    /// </summary>
    private Color baseColor;

    /// <summary>
    /// 無敵か否か
    /// </summary>
    protected bool isInvisible = false;

    /// <summary>
    /// 当たった瞬間
    /// </summary>
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 未使用の場合は無視
        if (!UseCheck()) { return; }

        // リアクション
        switch(other.gameObject.tag)
        {// タグによる分岐
            case Tags.PlayerWeapon:
                // 無敵なら当たらない
                if (isInvisible) { return; }

                // 参照
                var weapon = other.gameObject.GetComponent<Weapon>();

                // 弱点を持っている時、弱点以外は無視
                if ((Weakness != null) && (weapon.Kind != Weakness)) { return; }
                
                // 被弾
                if (weapon.IsAttacking)
                {
                    var damage = weapon.Damage;
                    damage *= (Weakness != null) ? EnemyManager.Instance.WeaknessBuff : 1.0f;
                    TakeDamage(damage);
                    OnDamaged();
                }

                // 弾の削除
                var bullet = weapon as Bullet;
                if (bullet)
                {// 弾だった場合
                    if (!bullet.isCharged)
                    {// 溜め発射されていない場合
                        bullet.ToUnused();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 被弾
    /// </summary>
    private void TakeDamage(float damage)
    {
        // 体力減少
        life -= damage;

        // 割合の更新
        LifeRatio = life * invLifeMax;

        // 反応
        if (life < 0.0f)
        {// 死んだ場合
            Death();
        }
        else
        {// 生き延びた場合
            DamageReaction();
        }
    }
    
    private void DamageReaction()
    {
        // 色変更
        var manager = EnemyManager.Instance;
        Color = baseColor * manager.DamagedColor;
        Invoke("ResetColor", manager.ReactionSecond);

        // SE再生
        AudioManager.Instance.PlaySE(EnemyManager.Instance.DamagedSE);
    }

    private void ResetColor()
    {
        Color = baseColor;
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Death()
    {
        // 仮想関数呼び出し
        OnDead();

        // エフェクト発生
        EffectManager.Instance.Spawn(transform.position);

        // 効果音再生
        AudioManager.Instance.PlaySE(EnemyManager.Instance.DestroiedSE);

        // アイテムドロップ
        var dropCountAdd = Weakness.HasValue ? 2 : 0;   // この増分をどこに持たせよう
        if (dropPercentage.Judgement() || Weakness.HasValue)
        {// 確率を引いたか弱点持ちの場合
            var dropCount = dropCountRange.RandInt + dropCountAdd;
            var itemManager = ItemManager.Instance;

            for (int i = 0; i < dropCount; ++i)
            {
                itemManager.Drop(Usual.RandomElem(dropItemKinds), transform.position);
            }
        }

        // 未使用にする
        ToUnused();
    }

    public override void OnUnused()
    {
        base.OnUnused();
        EnemyManager.Instance.SubEnemyCount();
    }

    /// <summary>
    /// 体力の初期化
    /// </summary>
    protected void ResetLife()
    {
        invLifeMax = 1.0f / lifeMax;
        life = lifeMax;
        LifeRatio = 1.0f;
    }

    /// <summary>
    /// 被弾直後の処理（ダメージ反映後）
    /// </summary>
    protected virtual void OnDamaged() { }

    /// <summary>
    /// 死亡した瞬間の処理
    /// </summary>
    protected virtual void OnDead() { }

    public override void OnGenerated()
    {
        base.OnGenerated();
        ResetLife();
        Angle = Degree.mid;

        // 弱点の設定
        var manager = EnemyManager.Instance;
        var weaknessWeapon = manager.Weekness;
        //
        // 弱点を持つ条件
        // 確率を引いていて、選んだ武器が解放済みの場合
        var isHaveWeakness = manager.WeaknessProbability.Judgement() && Player.Instance.IsUnlocked(weaknessWeapon.Kind);
        if (isHaveWeakness)
        {// 弱点を持つ場合
            Weakness = weaknessWeapon.Kind;
            Color = baseColor = weaknessWeapon.Color;
        }
        else
        {// 弱点を持たない場合
            Weakness = null;
            Color = baseColor = InitialColor;
        }
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();
        baseColor = InitialColor;
        transform.parent = EnemyManager.Instance.transform;
    }
}
