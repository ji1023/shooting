using UnityEngine;
using System.Collections;

public class Camera : SingletonMonoBehaviour<Camera>
{
    /// <summary>
    /// 振動
    /// </summary>
    private class Vibration
    {
        /// <summary>
        /// 長さ
        /// </summary>
        public Timer duration;

        /// <summary>
        /// 強さ
        /// </summary>
        public float strength;

        /// <summary>
        /// 方向
        /// </summary>
        public Vector3? direction = Vector3.up;

        /// <summary>
        /// 減衰するか否か
        /// </summary>
        public bool isDecay = true;

        /// <summary>
        /// 基準座標
        /// </summary>
        public Vector3 basePos;

        /// <summary>
        /// 振動後の座標
        /// </summary>
        public Vector3 VibratedPos
        {
            get
            {
                // 座標を求める
                var distance = isDecay ? strength * (1.0f - duration.Ratio) : strength;
                var dir = direction.HasValue ? direction.Value : Degree.Rand.XY;
                var result = basePos + dir * distance;

                // 方向反転
                if (direction.HasValue)
                {
                    direction = -direction;
                }

                // 返却
                return result;
            }
        }
    }

    /// <summary>
    /// 振動
    /// </summary>
    private Vibration vibration = new Vibration();

    /// <summary>
    /// 振動させる
    /// </summary>
    /// <param name="strength">強さ</param>
    /// <param name="duration">秒数</param>
    /// <param name="direction">方向</param>
    /// <param name="isDecay">減衰させるか否か</param>
    public void Vibrate(float strength, float duration, Vector2? direction = null, bool isDecay = true)
    {
        if (!vibration.duration.IsCounting)
        {// 振動中ではない場合
            vibration.basePos = transform.position;
        }
        vibration.strength = strength;
        vibration.duration.Restart(duration);
        direction?.Normalize();
        vibration.direction = direction;
        vibration.isDecay = isDecay;
    }

    /// <summary>
    /// 振動を止める
    /// </summary>
    public void StopVibration()
    {
        vibration.duration.Stop();
    }

    private void FixedUpdate()
    {
        if (vibration.duration.IsCounting)
        {// 振動中の場合
            // 計測
            vibration.duration.Advance();

            // 座標ブレ
            transform.position = vibration.VibratedPos;

            // 終了
            if (vibration.duration.IsTermination)
            {// 終了した場合
                transform.position = vibration.basePos;
            }
        }
    }
}
