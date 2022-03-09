using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Degree
{
    public const float MIN = 0.0f;
    public const float MID = 180.0f;
    public const float MAX = 360.0f;

    /// <summary>
    /// ランダムな角度
    /// </summary>
    public static Degree Rand
    {
        get
        {
            return new Degree(Random.Range(MIN, MAX - Mathf.Epsilon));
        }
    }

    [SerializeField, Range(MIN, MAX)]
    private float angle;

    public float Radian
    {
        get
        {
            return angle * Mathf.Deg2Rad;
        }
    }

    public Vector3 XY
    {
        get
        {
            var rad = Radian;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }
    }

    public Degree(float angle = 0.0f)
    {
        this.angle = angle;
        Fix();
    }

    /// <summary>
    /// あるベクトルがxy平面上でx軸を0度とした反時計周りの円のどの角度になるか
    /// </summary>
    /// <param name="v">あるベクトル</param>
    /// <returns>ベクトルの角度</returns>
    public static Degree FromVector(Vector3 v)
    {
        var angleX = Degree.TwoVector(Vector3.right, v);
        return (Vector3.Dot(Vector3.up, v) > 0) ? angleX : -angleX;
    }

    /// <summary>
    /// 2ベクトル間の角度を求める
    /// </summary>
    public static Degree TwoVector(Vector3 v1, Vector3 v2)
    {
        return new Degree(Mathf.Acos(Vector3.Dot(v1.normalized, v2.normalized)) * Mathf.Rad2Deg);
    }

    public static implicit operator float(Degree deg)
    {
        return deg.angle;
    }

    public static implicit operator Degree(float angle)
    {
        return new Degree(angle);
    }

    public static Degree operator+(Degree deg, float value)
    {
        deg.angle += value;
        deg.Fix();
        return deg;
    }

    public static Degree operator-(Degree deg, float value)
    {
        deg.angle -= value;
        deg.Fix();
        return deg;
    }

    public static Degree operator*(Degree deg, float value)
    {
        deg.angle *= value;
        deg.Fix();
        return deg;
    }

    public static Degree operator-(Degree deg)
    {
        return new Degree(-deg.angle);
    }

    private void Fix()
    {
        if (angle < MIN)
        {
            do
            {
                angle += MAX;
            } while (angle < MIN);
        }
        else if (angle >= MAX)
        {
            do
            {
                angle -= MAX;
            } while (angle >= MIN);
        }
    }
}
