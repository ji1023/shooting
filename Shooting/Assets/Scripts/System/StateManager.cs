using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 状態管理
/// </summary>
public class StateManager<T>
{
    /// <summary>
    /// 状態
    /// </summary>
    public class State
    {
        /// <summary>
        /// 遷移条件
        /// </summary>
        /// <returns>遷移するならtrue</returns>
        public delegate bool TransitionCondition();

        /// <summary>
        /// 状態中の挙動
        /// </summary>
        public System.Action Behaviour { get; private set; } = null;

        /// <summary>
        /// 状態に入った直後の処理
        /// </summary>
        public System.Action onTransitioned = null;

        /// <summary>
        /// 状態から抜ける直前の処理
        /// </summary>
        public System.Action onFinished = null;
        
        /// <summary>
        /// 遷移先ごとの遷移条件
        /// </summary>
        public Dictionary<T, LinkedList<TransitionCondition>> Transitions { get; private set; } = new Dictionary<T, LinkedList<TransitionCondition>>();

        /// <param name="behaviour">状態中の挙動</param>
        public State(System.Action behaviour)
        {
            Behaviour = behaviour;
        }

        /// <summary>
        /// 遷移先を登録する
        /// </summary>
        /// <param name="destStateCode">遷移先の識別子</param>
        /// <param name="condition">遷移条件</param>
        /// <returns>遷移先の遷移条件一覧</returns>
        public LinkedList<TransitionCondition> AddTransition(T destStateCode, TransitionCondition condition)
        {
            LinkedList<TransitionCondition> conditions = null;
            if (Transitions.ContainsKey(destStateCode))
            {// 遷移先の登録がある場合
                conditions = Transitions[destStateCode];

            }
            else
            {// ない場合
                conditions = new LinkedList<TransitionCondition>();
                Transitions.Add(destStateCode, conditions);
            }
            conditions.AddLast(condition);
            return conditions;
        }

        /// <summary>
        /// ある状態への遷移を削除する
        /// </summary>
        /// <param name="destStateCode">削除する遷移先の識別子</param>
        public void RemoveTransition(T destStateCode)
        {
            Transitions.Remove(destStateCode);
        }
    }

    /// <summary>
    /// 状態一覧
    /// </summary>
    private Dictionary<T, State> states = new Dictionary<T, State>();

    /// <summary>
    /// 今の状態の識別子
    /// </summary>
    private T nowStateCode = default;

    /// <summary>
    /// 今の状態の識別子
    /// </summary>
    public T NowStateCode
    {
        get => nowStateCode;
        set
        {
            // 今の状態があれば終了処理
            nowState?.onFinished?.Invoke();

            // 識別子の反映
            nowStateCode = value;
            nowState = states[value];

            // 開始処理
            nowState.onTransitioned?.Invoke();
        }
    }

    /// <summary>
    /// 現在の状態
    /// </summary>
    private State nowState = null;

    /// <summary>
    /// 現在の状態の動作を実行する
    /// </summary>
    public void Behave()
    {
        // 状態がなければ無視
        if (nowState == null) { return; }

        // 挙動
        nowState.Behaviour();

        // 状態遷移
        foreach (var transition in nowState.Transitions)
        {// 遷移一覧のループ
            foreach (var condition in transition.Value)
            {// 遷移条件一覧のループ
                if (condition())
                {// 遷移条件が成立した場合
                    // 状態変更
                    NowStateCode = transition.Key;  // 条件に紐づいた遷移先

                    // ループ終了
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 状態を追加する
    /// </summary>
    /// <param name="code">状態の識別子</param>
    /// <param name="behaviour">状態中の挙動</param>
    public State AddState(T code, System.Action behaviour)
    {
        // 重複の無視
        if (states.ContainsKey(code)) { return null; }

        // 状態の作成
        var state = new State(behaviour);

        // 一覧への追加
        states.Add(code, state);

        // 返却
        return state;
    }
}