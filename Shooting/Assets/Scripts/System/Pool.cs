using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// プール機能
/// </summary>
/// <typeparam name="T">管理するオブジェクトの型</typeparam>
[System.Serializable]
public class Pool<T> where T : PoolableObject
{
    /// <summary>
    /// オブジェクト一覧
    /// </summary>
    private LinkedList<T> objects = new LinkedList<T>();
    
    public delegate void EventHandler(T obj);
    /// <summary>
    /// 生成直後の処理（仮想関数よりも前）
    /// </summary>
    public EventHandler beforeOnGenerated = null;

    /// <summary>
    /// 新規作成直後の処理（仮想関数よりも前）
    /// </summary>
    public EventHandler beforeOnInstantiated = null;

    /// <summary>
    /// 新規作成時に使うプレハブ
    /// </summary>
    [SerializeField, Tooltip("新規作成時に使うプレハブ")]
    public List<T> prefabs = new List<T>();

    /// <summary>
    /// 生成する
    /// </summary>
    /// <returns>新しく使えるオブジェクト</returns>
    /// <param name="pos">生成座標</param>
    public T Generate(Vector3 pos)
    {
        return Generate(pos, Usual.RandomIndex(prefabs));
    }

    /// <summary>
    /// 生成する
    /// </summary>
    /// <returns>新しく使えるオブジェクト</returns>
    /// <param name="pos">生成座標</param>
    /// <param name="prefabIndex">新規作成に使うプレハブの添え字</param>
    public T Generate(Vector3 pos, int prefabIndex)
    {
        // 使い回し
        T newObj = null;
        newObj = Reuse(pos);

        // 新規作成
        if (newObj == null)
        {// 使い回せなかった場合
            newObj = Instantiate(pos, prefabIndex);
        }

        // 返却
        return newObj;
    }

    /// <summary>
    /// オブジェクトを新規作成する
    /// </summary>
    /// <param name="pos">生成する座標</param>
    /// <returns>生成されたオブジェクト</returns>
    public T Instantiate(Vector3 pos)
    {
        return Instantiate(pos, Usual.RandomIndex(prefabs));
    }

    /// <summary>
    /// オブジェクトを新規作成する
    /// </summary>
    /// <param name="pos">生成する座標</param>
    /// <param name="prefabIndex">作成に使うプレハブの添え字</param>
    /// <returns>生成されたオブジェクト</returns>
    public T Instantiate(Vector3 pos, int prefabIndex)
    {
        // 新規作成
        var obj = GameObject.Instantiate(prefabs[prefabIndex]);

        // 一覧へ登録
        objects.AddLast(obj);

        // 新規作成直後の処理
        beforeOnInstantiated?.Invoke(obj);
        obj.OnInstantiated();

        // 生成後の処理
        OnGenerated(obj, pos);

        // 参照を返す
        return obj;
    }

    /// <summary>
    /// オブジェクトを使い回す
    /// </summary>
    /// <param name="pos">生成する座標</param>
    /// <returns>生成されたオブジェクト</returns>
    public T Reuse(Vector3 pos)
    {
        foreach (var obj in objects)
        {
            // 使用中オブジェクトの無視
            if (obj.UseCheck()) { continue; }
            
            // 再利用時の処理
            obj.OnReused();

            // 生成後の処理
            OnGenerated(obj, pos);

            // 参照を返す
            return obj;
        }
        return null;
    }

    /// <summary>
    /// 生成後の処理
    /// </summary>
    /// <param name="obj">生成されたオブジェクト</param>
    /// <param name="pos">生成する座標</param>
    private void OnGenerated(T obj, Vector3 pos)
    {
        // 座標の設定
        obj.transform.position = pos;

        // 生成直後の処理
        beforeOnGenerated?.Invoke(obj);
        obj.OnGenerated();

        // 使用状態にする
        obj.ToUsed();
    }

    /// <summary>
    /// 使用中のオブジェクトに対して処理を行う
    /// </summary>
    /// <param name="proc">処理内容</param>
    public void Foreach(EventHandler proc)
    {
        foreach (var obj in objects)
        {// オブジェクト一覧のループ
            if (obj.UseCheck())
            {// 使用中の場合
                proc(obj);
            }
        }
    }

    /// <summary>
    /// 管理下にあるオブジェクト全てを未使用にする
    /// </summary>
    public void AllToUnused()
    {
        foreach (var obj in objects)
        {// オブジェクト一覧のループ
            if (obj.UseCheck())
            {// 使用中の場合
                obj.ToUnused();
            }
        }
    }

    /// <summary>
    /// あるオブジェクトをプールの管理下に登録する
    /// </summary>
    /// <param name="obj">登録したいオブジェクト</param>
    public void Add(T obj)
    {
        objects.AddLast(obj);
    }

    /// <summary>
    /// プールの内容を削除する
    /// </summary>
    public void Clear()
    {
        objects.Clear();
    }
}
