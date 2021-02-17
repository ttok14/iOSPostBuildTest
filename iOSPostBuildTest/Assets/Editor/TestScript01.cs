using System.Collections;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using UnityEditor.iOS.Xcode;

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

        string configDir = Path.GetDirectoryName(Application.dataPath) + @"\TestResource";
        var addFilePath = configDir + @"\fileA.png";

        UnityEngine.Debug.LogError("ConfigDir : " + configDir);
        Debug.LogError("AddFilePath : " + addFilePath);
    }
}