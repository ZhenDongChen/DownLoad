
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

//生成对应的version 文件 版本的url|文件名字|文件大小|hashcode

public class PatchItemData
{
    public string Url;
    public string FileName;
    public float FileLength;
    public string FileHashCode;
    public DownloadPriority Priority =  DownloadPriority.Lower;
    
    public const int SplitStrLength = 4;
    public const int FileNameIndex = 0;
    public const int UrlIndex = 1;
    public const int FileLengthIndex = 2;
    public const int FileHashCodeIndex = 3;

    public PatchItemData()
    {
        
    }

    public PatchItemData(string fileName,float fileLength,string fileHashCode)
    {
        Url = $"{PatchConfig.DownloadUrl}{fileName}";
        FileLength = fileLength;
        FileHashCode = fileHashCode;
        FileName = fileName;
    }
    public string ToString()
    {
        return $"{FileName}|{Url}|{FileLength}|{FileHashCode}";
    }

    public void  TryParse(string[] contents)
    {
        FileName = contents[FileNameIndex];
        Url = $"{PatchConfig.DownloadUrl}/{FileName}";
        float length = 0;
        if (float.TryParse(contents[FileLengthIndex], out length))
        {
            FileLength = length;
        }
        FileHashCode = contents[FileHashCodeIndex];
       
    }
}
public class PatchHelper
{
    static MD5 create = MD5.Create();
    static StringBuilder sb = new StringBuilder();
    public static string InternalComputeHash(byte[] buffer)
    {
        byte[] hash = create.ComputeHash(buffer);
        sb.Clear();
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
    public static List<PatchItemData> TryString2PatchItemData(string versionContent)
    {
        var splitArray = versionContent.Split("\n");
        if (splitArray.Length <= 0)
        {
            return null;
        }

        List<PatchItemData> datas = new List<PatchItemData>();
        foreach (var line in splitArray)
        {
            if (string.IsNullOrEmpty(line.Trim()))
            {
                continue;
            }

            var contentArray = line.Trim().Split("|");
            if (contentArray.Length != PatchItemData.SplitStrLength)
            {
                Debug.LogError($"format is illegal lineContent:{line}");
                continue;
            }

            PatchItemData itemData = new PatchItemData();
            itemData.TryParse(contentArray);
            datas.Add(itemData);
        }
        return datas;
    }
}
