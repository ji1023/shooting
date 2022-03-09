using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemPool : Pool<Item> { }

public class ItemManager : SingletonMonoBehaviour<ItemManager>
{
    [SerializeField]
    private ItemPool items = new ItemPool();

    /// <summary>
    /// 散らばる距離の幅
    /// </summary>
    [SerializeField]
    private Limit DropDistanceRange = new Limit(1.0f, 2.5f);

    /// <summary>
    /// 散らばる距離
    /// </summary>
    public float DropDistance
    {
        get
        {
            return DropDistanceRange.Rand;
        }
    }

    /// <summary>
    /// 散らばり終わるまでの秒数
    /// </summary>
    [SerializeField, Range(0.0f, 5.0f)]
    private float dropInterval = 0.5f;

    /// <summary>
    /// 散らばり終わるまでの秒数
    /// </summary>
    public float DropInterval => dropInterval;

    /// <summary>
    /// アイテムの移動速度
    /// </summary>
    [SerializeField]
    private float itemSpeed = 20.0f;

    /// <summary>
    /// アイテムの移動速度
    /// </summary>
    public float ItemSpeed => itemSpeed;

    /// <summary>
    /// ドロップ予定のアイテムの種類
    /// </summary>
    public Item.Kinds DropItemKind { get; private set; }

    /// <summary>
    /// 増加量の倍率の幅
    /// </summary>
    private Limit amountBuffRange = new Limit(1.0f, 2.0f);

    /// <summary>
    /// 増加量の倍率
    /// </summary>
    public float AmountBuff => amountBuffRange.Rand;

    /// <summary>
    /// アイテムをドロップさせる
    /// </summary>
    /// <param name="kind">ドロップさせるアイテムの種類</param>
    /// <param name="pos">ドロップさせる位置</param>
    /// <returns>ドロップしたアイテム</returns>
    public Item Drop(Item.Kinds kind, Vector3 pos)
    {
        DropItemKind = kind;
        return items.Generate(pos, (int)kind);
    }

    public void Restart()
    {
        items.AllToUnused();
    }
}
