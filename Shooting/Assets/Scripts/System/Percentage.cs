using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct Percentage
{
    public const float MIN = 0.0f;
    public const float MID = 50.0f;
    public const float MAX = 100.0f;

    /// <summary>
    /// 割合
    /// </summary>
    public float Ratio
    {
        get
        {
            return probability * 0.01f;
        }
    }

    /// <summary>
    /// 確率
    /// </summary>
    [SerializeField, Range(MIN, MAX)]
    private float probability;

    /// <summary>
    /// 確率
    /// </summary>
    public float Probability
    {
        get => probability;
        set
        {
            probability = Mathf.Clamp(value, MIN, MAX);
        }
    }

    public Percentage(float probability)
    {
        this.probability = Mathf.Clamp(probability, MIN, MAX);
    }

    public bool Judgement()
    {
        return Judgement(probability);
    }

    public static bool Judgement(float probability)
    {
        var value = Random.Range(MIN + Mathf.Epsilon, MAX);
        if (value <= probability)
        {
            return true;
        }
        return false;
    }

    public static implicit operator Percentage(float value)
    {
        return new Percentage(value);
    }
}