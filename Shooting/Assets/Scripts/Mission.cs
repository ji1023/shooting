using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// ミッション
/// </summary>
public class Mission
{
    /// <summary>
    /// 敵の種類
    /// </summary>
    public enum EnemyKind
    {
        Straight,   // 直進
        Aimer,      // 突進
        MoveShot,   // 直進射撃
        StayShot,   // 停止射撃
        Weakness,   // 弱点持ち
        All         // 全ての敵
    }
    /// <summary>
    /// 敵の種類ごとの倒した数
    /// </summary>
    private Dictionary<EnemyKind, uint> destroiedEnemyCounters = new Dictionary<EnemyKind, uint>();

    /// <summary>
    /// 達成条件
    /// </summary>
    /// <returns>条件を満たしたらtrue</returns>
    public delegate bool Condition();

    /// <summary>
    /// 達成条件
    /// </summary>
    public Condition condition = null;

    /// <summary>
    /// 報酬を獲得する処理
    /// </summary>
    public System.Action earnReward = null;

    /// <summary>
    /// 進捗率取得
    /// </summary>
    /// <returns>進捗率</returns>
    public delegate Ratio GetRatio();
    /// <summary>
    /// 進捗率取得
    /// </summary>
    public GetRatio getRatio = null;

    /// <summary>
    /// 達成条件の文章
    /// </summary>
    [System.NonSerialized]
    public string conditionText = string.Empty;

    /// <summary>
    /// 対応するUI
    /// </summary>
    [System.NonSerialized]
    public MissionUI ui = null;

    /// <summary>
    /// 初期化
    /// </summary>
    public Mission()
    {
        for (int i = 0; i <= (int)EnemyKind.All; ++i)
        {
            destroiedEnemyCounters.Add((EnemyKind)i, 0);
        }
    }

    /// <summary>
    /// 報酬を獲得できるか判定
    /// </summary>
    /// <returns>報酬を獲得したらtrue</returns>
    public bool Judgement()
    {
        if (condition())
        {// 条件を達成した場合
            return true;
        }
        return false;
    }

    /// <summary>
    /// 倒した敵の数を取得する
    /// </summary>
    /// <param name="kind">倒した数を見たい敵の種類</param>
    public uint GetDestroiedCount(EnemyKind kind)
    {
        return destroiedEnemyCounters[kind];
    }

    /// <summary>
    /// 倒した数を増やす
    /// </summary>
    /// <param name="kind">倒した敵の種類</param>
    /// <param name="weakness">倒した敵の弱点</param>
    public void AddDestroyCount(EnemyKind kind, Player.WeaponKind? weakness)
    {
        // カウント対象外の除外
        switch (kind)
        {
            case EnemyKind.Weakness:
            case EnemyKind.All:
                return;
        }

        ++destroiedEnemyCounters[EnemyKind.All];
        ++destroiedEnemyCounters[kind];
        if (weakness != null)
        {
            ++destroiedEnemyCounters[EnemyKind.Weakness];
        }
    }
}