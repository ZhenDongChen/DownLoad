using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// UnityWebRequest
/// 只要消耗是在  内核分配内存（读写缓存队列 ）占用了手机的网络的端口号
/// Update 频繁的访问数据 占用主线程的 CPU 耗时
///  
/// </summary>

public class TestDownLoad : MonoBehaviour
{
    public Slider slider;

    public Text DownLoadCount;
    public Text LoadingText;
    
    void Start()
    {
        StartCoroutine(LoadVersion(OnLoadVersionComplete));
    }

    void OnLoadVersionComplete(string version)
    {
        var patchItemDatas = PatchHelper.TryString2PatchItemData(version);
        foreach (var item in patchItemDatas)
        {
            DownloadMgr.Instance.AddDownLoad(item);    
        }
    }

    IEnumerator LoadVersion(Action<string> onLoadComplete)
    {
        yield return new WaitForSeconds(2f);
        string versionUrl = $"{PatchConfig.VersionUrl}/{PatchConfig.VersionName}";
        UnityWebRequest request = new UnityWebRequest(versionUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (string.IsNullOrEmpty(request.error))
        {
            if (!Directory.Exists(PatchConfig.WritePath))
            {
                Directory.CreateDirectory(PatchConfig.WritePath);
            }

            string versionPath = Path.Combine(PatchConfig.WritePath, PatchConfig.VersionName);
            File.WriteAllBytes(versionPath,request.downloadHandler.data);
            Debug.Log($"Down Load finish Content:{request.downloadHandler.text}");
            onLoadComplete?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
    

    private void Update()
    {
        DownLoadCount.text = $"当前下载文件的进度：{DownloadMgr.Instance.GetDownLoadProcess()*100}%  下载速度：{DownloadMgr.Instance.GetDownLoadSpeed()}M/S";
       // LoadingText.text = DownloadMgr.Instance.DisplayLoadProcess();
    }
}
