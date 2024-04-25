
using System.IO;
using UnityEngine;

public static class PatchConfig
{

    public const string DownloadUrl = "http://192.168.3.57/Bundles/";
    public const string VersionUrl = "http://192.168.3.57/Version/";
    public const string VersionName = "version.txt";
    public static string PatchSourcePath = Path.Combine(Application.dataPath, "Content/Art");
    public static string PathGenPath = Path.Combine(Application.dataPath, "../Build/version.txt");
    public static string BuildPath = Path.Combine(Application.dataPath, "../Build/Content");
    public static string WritePath = Path.Combine(Application.persistentDataPath, "DownLoad");
    
}
