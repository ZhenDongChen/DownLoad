
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 上传budnle 管理
/// </summary>
public static class UpLoadBundleMgr
{
    private static int maxUpLoadCount = 50;
    private static Dictionary<string, UpLoadBundleItem> _upLoadBundleDic = new Dictionary<string, UpLoadBundleItem>();
    private static List<PatchItemData> _upLoadDatas = new List<PatchItemData>();
    private static int _upLoadCount = 0;
    private static int _upLoadOffset = 0;
    public static void Upload(List<PatchItemData> datas)
    {
        _upLoadDatas.Clear();
        _upLoadBundleDic.Clear();
        _upLoadCount = 0;
        _upLoadOffset = 0;
        _upLoadDatas = datas;
        _upLoadCount = _upLoadDatas.Count;

        int uploadCount = datas.Count <= maxUpLoadCount ? datas.Count : maxUpLoadCount;

        PatchItemData item;
        for (int i = 0; i < uploadCount; i++)
        {
            item = _upLoadDatas[0];
            UpLoadBundleItem upItem = new UpLoadBundleItem(item);
            _upLoadBundleDic.Add(item.FileName,upItem);
            upItem.UpLoad();
            _upLoadDatas.RemoveAt(0);
        }
    }

    public static void OnSimpleProcess(string completeFileName)
    {
        if (_upLoadBundleDic.ContainsKey(completeFileName))
        {
            _upLoadBundleDic.Remove(completeFileName);    
        }
        else
        {
            Debug.LogError($"delete filename not exit Dic fileName:{completeFileName}");
            return;
        }
        _upLoadOffset--;
        EditorUtility.DisplayProgressBar("上传Bundle资源","上传bundle资源",(float)(_upLoadOffset/_upLoadCount));
        
        if (_upLoadOffset <= 0)
        {
            EditorUtility.ClearProgressBar();
            return;
        }
        if (_upLoadDatas.Count <= 0)
            return;
        CheckRemindData();
    }

    static void CheckRemindData()
    {
        int continueCount = maxUpLoadCount - _upLoadBundleDic.Count;
        if (continueCount <=0)
            return;

        PatchItemData item;
        for (int i = 0; i < continueCount; i++)
        {
            item = _upLoadDatas[0];
            UpLoadBundleItem upItem = new UpLoadBundleItem(item);
            _upLoadBundleDic.Add(item.FileName,upItem);
            upItem.UpLoad();
            _upLoadDatas.RemoveAt(0);
        }
    }
    
}
