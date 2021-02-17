#if UNITY_IOS
using System.Collections;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;

public class ZiOSPostProcessBuild
{
    private static ProjectCapabilityManager pcm { get; set; }

    [PostProcessBuild(101)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        UnityEngine.Debug.Log("OnPostprocessBuild");

        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string targetGuid = proj.GetUnityMainTargetGuid();

#if UNITY_2019_3_OR_NEWER
        pcm = new ProjectCapabilityManager(projPath, "ntsdk.entitlements", null, proj.GetUnityMainTargetGuid());
#else
			pcm = new ProjectCapabilityManager(projPath, "ntsdk.entitlements", PBXProject.GetUnityTargetName());
#endif

#if UNITY_2019_3_OR_NEWER
        string target = proj.GetUnityMainTargetGuid();
#else
			string target = proj.TargetGuidByName("Unity-iPhone");
#endif

        string[] linkerFlagsToAdd = {
				// "-lz" , AppGuard 용 . NTSDK 에서 이미 하므로 생략함. 
				"-lstdc++" // AppGuard 용
			};

        proj.UpdateBuildProperty(target, "OTHER_LDFLAGS", linkerFlagsToAdd, null);
        pcm.WriteToFile();

        string finalFolderName = "iosConfig_Line.IcarusEternal.KR.iOS";
        string configDir = Path.GetDirectoryName(Application.dataPath) + $@"/AppGuard_iOS/{finalFolderName}";

        //////////////////

        proj.ReadFromString(File.ReadAllText(projPath));
        List<string> resources = new List<string>();

        CopyAndReplaceDirectory(configDir, Path.Combine(path, finalFolderName));
        GetDirFileList(configDir, ref resources, finalFolderName);

        foreach (string resource in resources)
        {
            Debug.Log("CopyTo 'Copy Bundle Resource' : " + resource);
            string resourcesBuildPhase = proj.GetResourcesBuildPhaseByTarget(targetGuid);
            string resourcesFilesGuid = proj.AddFile(resource, resource, PBXSourceTree.Source);
            proj.AddFileToBuildSection(targetGuid, resourcesBuildPhase, resourcesFilesGuid);
        }

        File.WriteAllText(projPath, proj.WriteToString());
    }

    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
            Directory.Delete(dstPath);
        if (File.Exists(dstPath))
            File.Delete(dstPath);

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
        {
            //if (enableExts.Contains(System.IO.Path.GetExtension(file)))
            {
                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
            }
        }

        foreach (var dir in Directory.GetDirectories(srcPath))
        {
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }

    public static void GetDirFileList(string dirPath, ref List<string> dirs, string subPathFrom = "")
    {
        foreach (string path in Directory.GetFiles(dirPath))
        {
            //       if (enableExts.Contains(System.IO.Path.GetExtension(path)))
            {
                if (subPathFrom != "")
                {
                    dirs.Add(path.Substring(path.IndexOf(subPathFrom)));
                }
                else
                {
                    dirs.Add(path);
                }
            }
        }

        if (Directory.GetDirectories(dirPath).Length > 0)
        {
            foreach (string path in Directory.GetDirectories(dirPath))
            {
                GetDirFileList(path, ref dirs, subPathFrom);
            }
        }

    }
}
#endif