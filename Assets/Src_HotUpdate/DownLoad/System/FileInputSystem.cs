

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


public enum InputState
{
    Wait,
    StartWrite,
    Writing,
    WritFinish,
}

/// <summary>
/// 文件 写入 需要做分帧处理
/// </summary>
public class FileInputSystem
{
    private List<DownloadItem> _waitInputItems = new List<DownloadItem>();

    private int verifyMaxCount = 2;

    private InputState curState ;
    private UnityEngine.Object lockObject = new UnityEngine.Object();

    private Task runTask;
    public FileInputSystem()
    {
        curState = InputState.Wait;
        SimpleGameEvent.Instance.Register<DownloadItem>(EventConstant.OnVerifyFinish,OnVerifyFinish);
    }

    void OnVerifyFinish(EventData<DownloadItem> item)
    {
        lock (lockObject)
        {
            _waitInputItems.Add(item.Data);    
        }
    }

    
    void StartDealWrite()
    {
        curState = InputState.StartWrite;
        runTask = Task.Run(
            OnDealWrite
        );
    }

    void OnDealWrite()
    {
        lock (lockObject)
        {
            curState = InputState.Writing;
            DownloadItem item = _waitInputItems[0];
            string target = Path.Combine(PatchConfig.WritePath, item.FileName);
            if (!Directory.Exists(PatchConfig.WritePath))
            {
                Directory.CreateDirectory(PatchConfig.WritePath);
            }
            File.WriteAllBytes(target,item.DownloadBytes);
            SimpleGameEvent.Instance.Fire(EventConstant.OnWriteFileFinish,new EventData<DownloadItem>(item));
            _waitInputItems.RemoveAt(0);
            curState = InputState.WritFinish;
            item.ChangeState(LoadState.Done);
        }
    }
    
    public void Update()
    {
        if (_waitInputItems.Count <= 0)
        {
            return;
        }

        //当线程没有启动的时候 或者任务没有完成的时候需要retrun
        if (curState == InputState.Writing || curState == InputState.StartWrite)
        {
            return;
        }
        StartDealWrite();
    }
}
