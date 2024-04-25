using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class ComputeHash 
{

    
    [MenuItem("Tools/checkAssets")]
    public static void CheckAssets()
    {
        string path = Path.Combine(Application.dataPath,"log.txt");
        string result = File.ReadAllText(path);
        var allstr = result.Split(",");
        string targetPath = @"D:\Log\IL2CPP\Gradle\unityLibrary\src\main\assets";
        int index = 0;
        foreach (var item in allstr)
        {
            if (item.Contains("/"))
            {
                var temp = item.Split("/");
                string findPath = Path.Combine(targetPath, item.Trim());
                if (!File.Exists(findPath))
                {
                    Debug.LogError($"findPath:{findPath}");
                    break;
                }

                index++;
            }
        }

        Debug.LogError($"find:{index}");
    }

    static string GetFileName(string filePath,bool isContainSuffix = true)
    {
        //先替换
        filePath = filePath.Replace(@"\", "/");
        int lastIndex = filePath.LastIndexOf("/");
        string fileName = filePath.Substring(lastIndex + 1, filePath.Length - lastIndex - 1);
        if (fileName.Contains("."))
        {
            if (isContainSuffix)
            {
                return fileName;
            }
            else
            {
                var array = fileName.Split(".");
                return array[0];
            }
        }
        return fileName;
    }

    /// <summary>
    /// 生成Patch 文件
    /// 分为三部分
    /// 1.生成PathItemData
    /// 2.生成PathFile
    /// 3.上传PathchItemFile
    /// </summary>
    [MenuItem("Tools/PatchVersion")]
    public static void PatchVersionInfo()
    {
        List<PatchItemData> patchList = new List<PatchItemData>();
        var allTargetFiles = Directory.GetFiles(PatchConfig.BuildPath, "*.*");
        
        EditorUtility.DisplayProgressBar("生成PathItemData","PatchItem",0);
        float  process = 0.0f;
        foreach (var filePath in allTargetFiles)
        {
            var fileBytes = File.ReadAllBytes(filePath);
            var hashcode =PatchHelper.InternalComputeHash(fileBytes);
            PatchItemData data = new PatchItemData(GetFileName(filePath),fileBytes.Length/1024,hashcode);
            EditorUtility.DisplayProgressBar("生成PathItemData","PatchItem",(float)process/allTargetFiles.Length);
            patchList.Add(data);
        }

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var data in patchList)
        {
            stringBuilder.AppendLine(data.ToString());
        }
        File.WriteAllText(PatchConfig.PathGenPath,stringBuilder.ToString());
        EditorUtility.ClearProgressBar();
        //UpdateData(patchList);
    }

    public static void UpdateData(List<PatchItemData> datas)
    {
        List<PatchItemData> items = new List<PatchItemData>(1);
        items.Add(datas[0]);
        TestUpload(datas[0]);
        // UpLoadBundleMgr.Upload(items);
    }
    static string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    static void TestUpload(PatchItemData data)
    {
        string authorization = authenticate("czd", "123456");
        string targetBundlePath = Path.Combine(PatchConfig.BuildPath, data.FileName);
        if (!File.Exists(targetBundlePath))
        {
            Debug.LogError($"not find target bundle path:{targetBundlePath}");
             return;
        }

        var body = File.ReadAllBytes(targetBundlePath);
        UnityWebRequest unityWebRequest = new  UnityWebRequest(data.Url,UnityWebRequest.kHttpVerbPUT);
        unityWebRequest.SetRequestHeader("Content-Type","application/octet-stream;charset=utf-8");
        unityWebRequest.SetRequestHeader("AUTHORIZATION",authorization);
        unityWebRequest.uploadHandler = new UploadHandlerRaw(body);
        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
        var async = unityWebRequest.SendWebRequest();
         async.completed += (async1) =>
         {
             if (unityWebRequest.error != null)
             {
                 Debug.LogError($"{unityWebRequest.error}");
             }
         };
        
    }

}
