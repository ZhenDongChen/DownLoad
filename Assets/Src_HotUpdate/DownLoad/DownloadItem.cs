using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public enum DownloadPriority
{
    Lower,
    Normal,
    Higher
}

public enum LoadState
{
    None,
    Wait,//等待加载
    Loading,//正在加载
    Write,//正在写入
    Fail,//校验失败
    Done,//完成
    Abort,//终止
}

public class DownloadItem 
{
    public string DownLoadUrl;
    public DownloadPriority priority = DownloadPriority.Lower;
    public string FileName;
    public string HashCode;
    public byte[] DownloadBytes;
    
    private PatchItemData patchItemData;
    private UnityWebRequest downloadRequest;
    private UnityWebRequestAsyncOperation asyncRequest;
    
    private int tryDownLoadCount = 0;

    private LoadState curState;
    
    public void Init(PatchItemData itemData,DownloadPriority pPriority = DownloadPriority.Lower)
    {
        this.priority = pPriority;
        patchItemData = itemData;
        DownLoadUrl = patchItemData.Url;
        FileName = patchItemData.FileName;
        HashCode = patchItemData.FileHashCode;
        downloadRequest = UnityWebRequestAssetBundle.GetAssetBundle(DownLoadUrl);
        downloadRequest.downloadHandler = new DownloadHandlerBuffer();
        curState = LoadState.Wait;
    }

    void OnDownLoadComplete(AsyncOperation async)
    {
        DownloadBytes = downloadRequest.downloadHandler.data;
        SimpleGameEvent.Instance.Fire(EventConstant.OnDownLoadSuc,new EventData<DownloadItem>(this));
    }

    public void ChangeState(LoadState state)
    {
        curState = state;
        switch (state)
        {
            case LoadState.Abort:
                break;
            case LoadState.Fail:
                break;
        }
    }

    public void Start()
    {
        this.FileName = patchItemData.FileName;
        this.HashCode = patchItemData.FileHashCode;
        
        asyncRequest = downloadRequest.SendWebRequest();
        asyncRequest.completed += OnDownLoadComplete;
        ChangeState(LoadState.Loading);
    }

    void ReStart()
    {
        Abort();
        Start();
    }

    public void Abort()
    {
        downloadRequest.Abort();
        ChangeState(LoadState.Abort);
    }
    

    /// <summary>
    /// 获取下载文件的字节数
    /// </summary>
    /// <returns></returns>
    public float GetDownLoadLength()
    {
        if (DownloadBytes == null)
        {
            return 0;
        }
        
        return DownloadBytes.Length /1024/1024;
    }

    public LoadState GetState()
    {
        return curState;
    }

    public float GetProcess()
    {
        return asyncRequest.progress;
    }

    public bool IsDone()
    {
        return asyncRequest.isDone;
    }

    public void Update()
    {
        
    }

    public  void Dispose()
    {
        downloadRequest.Dispose();
        patchItemData = null;
        DownLoadUrl = null;
        FileName = null;
        HashCode = null;
        DownloadBytes = null;
    }
}
