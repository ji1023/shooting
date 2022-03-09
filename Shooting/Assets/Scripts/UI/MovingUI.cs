using UnityEngine;
using System.Collections;

public class MovingUI : MonoBehaviour
{
    /// <summary>
    /// 登場する位置
    /// </summary>
    [SerializeField]
    private Vector3 startPos = Vector3.zero;

    /// <summary>
    /// 登場後とどまる位置
    /// </summary>
    [SerializeField]
    private Vector3 endPos = Vector3.zero;

    /// <summary>
    /// 挙動用タイマー
    /// </summary>
    private Timer behaviourTimer;
    
    /// <summary>
    /// 挙動
    /// </summary>
    private System.Action behave = null;

    private void Start()
    {
        behaviourTimer.isUnscaled = true;
    }

    // Update is called once per frame
    void Update()
    {
        behave?.Invoke();
    }

    /// <summary>
    /// 始点から終点へ移動する
    /// </summary>
    /// <param name="interval">移動に要する秒数</param>
    public void StartToEnd(float interval)
    {
        LerpMove(startPos, endPos, interval);
    }

    /// <summary>
    /// 終点から始点へ移動する
    /// </summary>
    /// <param name="interval">移動に要する秒数</param>
    public void EndToStart(float interval)
    {
        LerpMove(endPos, startPos, interval);
    }

    private void LerpMove(Vector3 start, Vector3 end, float interval)
    {
        behaviourTimer.Restart(interval);
        transform.position = start;
        behave = () =>
        {
            behaviourTimer.Advance();
            transform.position = Vector3.Lerp(start, end, Easing.Quad.Out(behaviourTimer.Ratio));
            if (behaviourTimer.IsTermination)
            {
                transform.position = end;
                behave = null;
            }
        };
    }
}
