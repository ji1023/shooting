using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Actor
{
    /// <summary>
    /// 種類
    /// </summary>
    public enum Kinds
    {
        Damage,
        Way,
        RapidFire,
        Life,
        Heal
    }
    /// <summary>
    /// 種類
    /// </summary>
    [SerializeField]
    private Kinds kind = Kinds.Damage;

    /// <summary>
    /// 種類
    /// </summary>
    public Kinds Kind => kind;

    /// <summary>
    /// 上昇量
    /// </summary>
    [SerializeField]
    private float riseAmount = 0.0f;

    /// <summary>
    /// 上昇量
    /// </summary>
    public float RiseAmount => (riseAmount * riseAmountBuff);

    /// <summary>
    /// 上昇量の倍率
    /// </summary>
    private float riseAmountBuff;

    /// <summary>
    /// ドロップ挙動が終わるまでの秒数
    /// </summary>
    private Timer toDropEnd;

    /// <summary>
    /// ドロップした座標
    /// </summary>
    private Vector3 dropedPos;

    /// <summary>
    /// ドロップ完了時点の座標
    /// </summary>
    private Vector3 dropTarget;

    /// <summary>
    /// 元々の拡大率
    /// </summary>
    private Vector3 defaultScale;

    private void Update()
    {
        if (toDropEnd.IsCounting)
        {
            // ドロップ位置から目標位置まで補間移動
            transform.position = Vector3.Lerp(dropedPos, dropTarget, Easing.Quad.Out(toDropEnd.Ratio));
            toDropEnd.Advance();
        }
        else
        {
            if (Player.Instance.IsAlive)
            {// プレイヤーが生きている場合
                // プレイヤーめがけて
                LookAt(Player.Instance.transform.position);

                // 移動
                Move();
            }
        }
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();
        transform.parent = ItemManager.Instance.transform;
        defaultScale = transform.localScale;
    }

    public override void OnGenerated()
    {
        base.OnGenerated();

        var manager = ItemManager.Instance;
        dropedPos  = transform.position;
        dropTarget = transform.position + Degree.Rand.XY * manager.DropDistance;
        
        toDropEnd.Interval = manager.DropInterval;
        toDropEnd.Restart();
        
        speed = manager.ItemSpeed;

        riseAmountBuff = manager.AmountBuff;
        transform.localScale = defaultScale * riseAmountBuff;
    }

    public override bool UseCheck()
    {
        // 再利用できる条件は 非アクティブで今からドロップするアイテムと同じ種類であること
        // それの否定で使用中という扱いになる
        return !(!gameObject.activeSelf && kind == ItemManager.Instance.DropItemKind);
    }
}
