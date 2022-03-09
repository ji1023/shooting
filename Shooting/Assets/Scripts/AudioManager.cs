using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    /// <summary>
    /// BGMの音量
    /// </summary>
    [SerializeField]
    public Ratio bgmVolume = 1.0f;

    /// <summary>
    /// SEの音量
    /// </summary>
    [SerializeField]
    public Ratio seVolume = 1.0f;

    /// <summary>
    /// BGMのオーディオソース
    /// </summary>
    [SerializeField]
    private AudioSource bgmSource = null;

    /// <summary>
    /// SEのオーディオソース
    /// </summary>
    [SerializeField]
    private AudioSource seSource = null;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        // 音量の反映
        bgmSource.volume = bgmVolume;
        seSource .volume = seVolume;
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="clip">再生するBGM</param>
    public void PlayBGM(AudioClip clip, bool isLooping = true)
    {
        // 今再生しているBGMを停止
        bgmSource.Stop();

        // クリップ反映
        bgmSource.clip = clip;

        // ループ設定
        bgmSource.loop = isLooping;

        // 再生
        bgmSource.time = 0.0f;
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// BGMの再生を再開する
    /// </summary>
    /// <param name="isFromBeginning">初めからにするか否か</param>
    public void RestartBGM(bool isFromBeginning = true)
    {
        // BGMがなければ無視
        if (bgmSource.clip == null) { return; }

        // 初めに戻す
        if (isFromBeginning)
        {
            bgmSource.time = 0.0f;
        }

        // 再生
        bgmSource.Play();
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="clip">再生するSE</param>
    public void PlaySE(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }
}
