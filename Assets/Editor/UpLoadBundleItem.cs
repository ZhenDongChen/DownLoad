
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 上传bundle 版本信息
/// </summary>

public class UpLoadBundleItem
{
    public string FileName;
    public string HashCode;

    private UnityWebRequest _request;
    private UnityWebRequestAsyncOperation _asyncOperation;
    private PatchItemData _data;
    private int tryTime = 3;
    private int tryUpLoadIndex = 0;
    public UpLoadBundleItem(PatchItemData data)
    {
        _data = data;
        FileName = data.FileName;
        HashCode = data.FileHashCode;
    }

    void OnComplete(AsyncOperation async)
    {
        if (_request.error != null)
        {
            Debug.LogError(_request.error);
            return;
        }

        string upLoadHashCode = PatchHelper.InternalComputeHash(_request.uploadHandler.data);
        if (!HashCodeEqual(upLoadHashCode))
        {
            if (tryUpLoadIndex <= tryTime)
            {
                //重新开始上传
                UpLoad();
                tryUpLoadIndex++;
                Debug.LogError("上传 文件hash code 值不一致");
            }
            else
            {
                Debug.LogError($"上传失败 {_data.FileName}");   
            }
        }
        else
        {
            UpLoadBundleMgr.OnSimpleProcess(_data.FileName);
            Debug.Log($"上传完成 ：{_data.FileName}");
        }
       
    }

    public void UpLoad()
    {
        string targetBundlePath = Path.Combine(PatchConfig.BuildPath, FileName);
        if (!File.Exists(targetBundlePath))
        {
            Debug.LogError($"not find target bundle path:{targetBundlePath}");
            return;
        }
        _request = UnityWebRequest.Put(_data.Url,File.ReadAllBytes(targetBundlePath));
        _asyncOperation =  _request.SendWebRequest();
        _asyncOperation.completed += OnComplete;
    }

    public bool HashCodeEqual(string hashCode)
    {
        return HashCode == hashCode;
    }
    

}
