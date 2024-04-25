
//下载文件的版本校验

using System;using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;

public enum VerifyState
{
    Wait,
    StartVerify,
    Verifying,
    VerifyFinish,
}

/// <summary>
/// 文件校验 需要分帧处理
/// </summary>
public class VerifySystem
{
    private List<DownloadItem> _waitVerifyItems = new List<DownloadItem>();

    private int verifyMaxCount = 2;

    private VerifyState curState ;

    private UnityEngine.Object lockObject = new UnityEngine.Object();
    
    private Task runTask;

    public VerifySystem()
    {
        curState = VerifyState.Wait;
        SimpleGameEvent.Instance.Register<DownloadItem>(EventConstant.OnDownLoadSuc,OnDownLoadSuc);
    }

    void OnDownLoadSuc(EventData<DownloadItem> item)
    {
        lock (lockObject)
        {
            _waitVerifyItems.Add(item.Data);    
        }
    }

    void StartDealVerify()
    {
        curState = VerifyState.StartVerify;
        runTask = Task.Run(OnDealVerify);
    }

    void OnDealVerify()
    {
        lock (lockObject)
        {
            curState = VerifyState.Verifying;
            DownloadItem item = _waitVerifyItems[0];
            string loadHashCode = PatchHelper.InternalComputeHash(item.DownloadBytes);
            if (loadHashCode == item.HashCode)
            {
                item.ChangeState(LoadState.Write);
                _waitVerifyItems.RemoveAt(0);
                curState = VerifyState.VerifyFinish;
                SimpleGameEvent.Instance.Fire(EventConstant.OnVerifyFinish,new EventData<DownloadItem>(item));
            }
        }
    }


    public void Update()
    {
        if (_waitVerifyItems.Count <= 0)
        {
            return;
        }

        //当线程没有启动的时候 或者任务没有完成的时候需要retrun
        if ( curState == VerifyState.Verifying || curState == VerifyState.StartVerify)
        {
            return;
        }
        StartDealVerify();
    }

}
