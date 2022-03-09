using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveBullet : Bullet
{
    /// <summary>
    /// 種類
    /// </summary>
    public override Player.WeaponKind Kind => Player.WeaponKind.Wave;

    /// <summary>
    /// 振幅
    /// </summary>
    [SerializeField]
    private float amplitude = 1.0f;

    /// <summary>
    /// 周期
    /// </summary>
    [SerializeField]
    private float period = 0.4f;

    /// <summary>
    /// 上下の移動
    /// </summary>
    private SinCurve verticalMove;

    /// <summary>
    /// 基準位置
    /// </summary>
    private Vector3 basePos;
    
    // Start is called before the first frame update
    void Start()
    {
        verticalMove.Interval  =  period;
        verticalMove.range.min = -amplitude;
        verticalMove.range.max =  amplitude;
    }

    // Update is called once per frame
    void Update()
    {
        // 基準位置の更新
        basePos += Direction * SpeedPerFrame;

        // 波運動の更新
        verticalMove.Advance();
        var waveVelocity = Left * verticalMove;

        // 座標の更新
        transform.position = basePos + waveVelocity;
    }

    public override void OnGenerated()
    {
        base.OnGenerated();
        basePos = transform.position;
    }

    public override void OnReused()
    {
        base.OnReused();
        verticalMove.Reset();
    }
}
