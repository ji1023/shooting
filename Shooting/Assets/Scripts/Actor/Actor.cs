using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Actor : PoolableObject
{
    /// <summary>
    /// 基本移動速度
    /// </summary>
    [SerializeField]
    protected float speed = 0.0f;

    /// <summary>
    /// 実際の速度
    /// </summary>
    public virtual float Speed
    {
        get
        {
            return speed;
        }
    }

    /// <summary>
    /// フレームあたりの速度
    /// </summary>
    public float SpeedPerFrame
    {
        get
        {
            return Speed * Time.deltaTime;
        }
    }

    private Degree angle;
    /// <summary>
    /// 向いている角度
    /// </summary>
    public Degree Angle
    {
        get
        {
            return angle;
        }
        set
        {
            var rad = value.Radian;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);
            direction = new Vector2(cos, sin).normalized;
            Left  = new Vector2(-sin, cos).normalized;
            angle = value;
            transform.rotation = Quaternion.AngleAxis(angle - 90.0f, Vector3.forward);
        }
    }

    /// <summary>
    /// レンダラー
    /// </summary>
    protected SpriteRenderer Renderer { get; private set; } = null;

    /// <summary>
    /// 元々の色
    /// </summary>
    public Color InitialColor { get; private set; }

    /// <summary>
    /// 色
    /// </summary>
    public Color Color
    {
        get
        {
            if (Renderer == null)
            {
                Renderer = GetComponent<SpriteRenderer>();
                InitialColor = Renderer.color;
            }
            return Renderer.color;
        }
        set
        {
            if (Renderer == null)
            {
                Renderer = GetComponent<SpriteRenderer>();
                InitialColor = Renderer.color;
            }
            Renderer.color = value;
        }
    }

    /// <summary>
    /// 不透明度
    /// </summary>
    public float Alpha
    {
        get => Color.a;
        set
        {
            var color = Color;
            color.a = value;
            Color = color;
        }
    }

    /// <summary>
    /// 向いている方向（前方向）
    /// </summary>
    public Vector3 Direction
    {
        get => direction;
        set
        {
            // 方向ベクトルの更新
            direction = value.normalized;
            Left = Vector3.Cross(Vector3.forward, direction).normalized;

            // 角度の更新
            angle = Degree.FromVector(value);
            transform.rotation = Quaternion.AngleAxis(angle - 90.0f, Vector3.forward);
        }
    }
    private Vector3 direction = Vector3.right;

    /// <summary>
    /// 左方向
    /// </summary>
    public Vector3 Left { get; private set; } = Vector3.up;

    /// <summary>
    /// 右方向
    /// </summary>
    public Vector3 Right => -Left;

    /// <summary>
    /// 後ろ方向
    /// </summary>
    public Vector3 Back => -direction;

    /// <summary>
    /// 速度と方向を基に移動する
    /// </summary>
    public void Move()
    {
        transform.position += direction * SpeedPerFrame;
    }
    
    /// <summary>
    /// ある座標に向く
    /// </summary>
    /// <param name="target">目標座標</param>
    public void LookAt(Vector3 target)
    {
        Direction = target - transform.position;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        // 画面外に出たら無効化する
        switch (other.gameObject.tag)
        {
            case Tags.MovableArea:
                ToUnused();
                break;
        }
    }

    public override void OnInstantiated()
    {
        Renderer = gameObject.GetComponent<SpriteRenderer>();
        InitialColor = Renderer.color;
    }
}
