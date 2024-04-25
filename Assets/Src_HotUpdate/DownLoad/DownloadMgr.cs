using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class DownloadMgr : MonoBehaviour
{
    private static DownloadMgr instance;
    public static DownloadMgr Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject insObj = new GameObject("DownloadMgr");
                instance = insObj.AddComponent<DownloadMgr>();
                instance.InitDownLoadMgr();
            }
            return instance;
        }
    }
    
    private bool isStop = true;
    
    private List<DownloadItem> _waitLoadItems = new List<DownloadItem>();
    private Dictionary<string, DownloadItem> _loadingFinishItems = new Dictionary<string, DownloadItem>();

    private FileInputSystem fileInputSystem;
    private VerifySystem verifySystem;
    
    private int _downLoadTotalIndex = 0;
    private int _downLoadOffset = 0;

    private int maxDownLoadCount = 5;
    private float totalDownLength;
    private float timeSecond = 0;
    private float startTime = 0;
    private float endTime = 0;

    public void InitDownLoadMgr()
    {
        fileInputSystem = new FileInputSystem();
        verifySystem = new VerifySystem();
    }



    public void SetStop(bool enable)
    {
        this.isStop = enable;
    }
    
    public void AddDownLoad(PatchItemData itemData,DownloadPriority priority =  DownloadPriority.Lower)
    {
        DownloadItem item = new DownloadItem();
        item.Init(itemData,priority);
        _waitLoadItems.Add(item);
        _downLoadTotalIndex++;
        _downLoadOffset++;
        _waitLoadItems.Sort((x, y) =>
        {
            return x.priority.CompareTo(y.priority);
        });
        isStop = false;
    }
    
    
    public float GetDownLoadSpeed()
    {
        float loadTime = timeSecond - startTime;
        if ( loadTime <= 1)
        {
            loadTime = 1;
        }

        return  (int)totalDownLength / loadTime;
    }

    public float GetDownLoadProcess()
    {
        return (float)_downLoadOffset / _downLoadTotalIndex;
    }

    private List<DownloadItem> removeItems = new List<DownloadItem>();
    private void Update()
    {
        if (isStop)
            return;
        
        fileInputSystem.Update();
        verifySystem.Update();
        
        try
        {
            if (_loadingFinishItems.Count < maxDownLoadCount && _waitLoadItems.Count > 0)
            {
                int changeIndex = maxDownLoadCount - _loadingFinishItems.Count;
                if (changeIndex > _waitLoadItems.Count)
                {
                    changeIndex = _waitLoadItems.Count;
                }
            
                for (int i = 0; i < changeIndex; i++)
                {
                    DownloadItem waitStartItem = _waitLoadItems[0];
                    _loadingFinishItems.Add(waitStartItem.FileName,waitStartItem);
                    waitStartItem.Start();
                    _waitLoadItems.RemoveAt(0);
                }
            }
        }
        catch (Exception e)
        {
           Debug.LogException(e);
        }
        
        removeItems.Clear();
        foreach (var loadItem in _loadingFinishItems)
        {
            loadItem.Value.Update();
            if (loadItem.Value.GetState() == LoadState.Done)
            {
                removeItems.Add(loadItem.Value);
            }
            timeSecond += Time.deltaTime;
           // Debug.Log($"Update _loadingFinishItems Count:{loadItem.Value.GetState() } fileName:{loadItem.Value.FileName} ");
        }
        
        if (removeItems.Count > 0)
        {
            foreach (var item in removeItems)
            {
                _loadingFinishItems.Remove(item.FileName);
               
                totalDownLength += item.GetDownLoadLength();
                //开始下载的 时间
                if (_downLoadOffset == _downLoadTotalIndex)
                    startTime = timeSecond;
                _downLoadOffset--;
                item.Dispose();
                //下载完成
                if (_waitLoadItems.Count <= 0 && _loadingFinishItems.Count <= 0)
                {
                    _downLoadOffset = 0;
                    endTime = timeSecond;
                    isStop = true;
                    Debug.Log($"download all item finish");
                }
            }
        }
    }
}
