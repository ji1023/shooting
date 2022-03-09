using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使い回せるオブジェクト
/// </summary>
public abstract class PoolableObject : MonoBehaviour
{
    /// <summary>
    /// 使用中判定
    /// </summary>
    /// <returns>使用中ならtrue</returns>
    public virtual bool UseCheck()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// 未使用化
    /// </summary>
    public virtual void ToUnused()
    {
        OnUnused();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 使用状態にする
    /// </summary>
    public virtual void ToUsed()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 未使用にする直前の付加処理
    /// </summary>
    public virtual void OnUnused() { }

    /// <summary>
    /// 再利用時の処理
    /// </summary>
    public virtual void OnReused() { }

    /// <summary>
    /// 生成直後の処理
    /// </summary>
    public virtual void OnGenerated() { }

    /// <summary>
    /// 新規生成直後の処理
    /// </summary>
    public virtual void OnInstantiated() { }
}
