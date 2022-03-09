using UnityEngine;
using System.Collections;

public class Scroll : MonoBehaviour
{
    private new SpriteRenderer renderer = null;

    /// <summary>
    /// スクロール速度
    /// </summary>
    [SerializeField]
    private float speed = 0.5f;

    // Use this for initialization
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float u = Mathf.Repeat(Time.time * speed, 1.0f);
        var offset = new Vector2(u, 0.0f);
        renderer.material.SetTextureOffset("_MainTex", offset);
    }
}
