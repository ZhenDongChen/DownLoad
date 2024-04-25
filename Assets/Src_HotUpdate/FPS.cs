using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 检测当前游戏的帧率 就是求平均值
/// 1)Time.unscaledTime 当前跑一帧所需的时间
/// 1）游戏的帧率 看当前设置的 1/Application.targetFrameRate 当前游戏设置的帧率 
/// 半秒检测一次
/// </summary>
public class FPS : MonoBehaviour
{
    private const float checkFpsIntervalTime = 1f;
    private int frameCount = 0;
    private float frameTimeOffset;
    private float checkFps;

    public Text FpsText;
    
    private void Start()
    {
        checkFps = 0;
        frameTimeOffset = 0;
        frameCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        frameTimeOffset += Time.unscaledDeltaTime;
        frameCount++;
        if (frameTimeOffset >= checkFpsIntervalTime)
        {
            checkFps = frameCount;
            FpsText.text = checkFps.ToString();
            frameCount = 0;
            frameTimeOffset = 0;
        }
    }
}
