using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TimeGate.EditorTools
{
    [Serializable] class VersionInfo { public string versionName; public int versionCode; public string packageId; }

    /// Single entry point for CI: unity -batchmode -executeMethod TimeGate.EditorTools.BuildScript.BuildAndroid
    public static class BuildScript
    {
        const string PackageId = "com.timegate.timetravel";

        public static void BuildAndroid()
        {
            var v = JsonUtility.FromJson<VersionInfo>(File.ReadAllText("version.json"));
            if (v.packageId != PackageId) throw new Exception("version.json packageId must stay " + PackageId);
            if (v.versionCode < 1) throw new Exception("versionCode must be >= 1");

            PlayerSettings.productName = "TimeGate";
            PlayerSettings.companyName = "TimeGate";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.bundleVersion = v.versionName;
            PlayerSettings.Android.bundleVersionCode = v.versionCode;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            EditorUserBuildSettings.buildAppBundle = false;

            IncludeShaders(); SetIcon();

            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");

            string outPath = "Build/Android/TimeGate-" + v.versionName + "-" + v.versionCode + ".apk";
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-customBuildPath") outPath = args[i + 1];
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? "Build/Android");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" }, locationPathName = outPath, target = BuildTarget.Android, options = BuildOptions.None
            });
            Debug.Log("[TimeGate] Build result: " + report.summary.result + " size=" + report.summary.totalSize + " -> " + outPath);
            if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        // Shaders used via Shader.Find at runtime would be stripped otherwise.
        static void IncludeShaders()
        {
            try
            {
                var all = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
                if (all == null || all.Length == 0) { Debug.LogWarning("[TimeGate] GraphicsSettings asset not found; shaders not pinned."); return; }
                var so = new SerializedObject(all[0]); var arr = so.FindProperty("m_AlwaysIncludedShaders");
                foreach (var n in new[] { "Standard", "Legacy Shaders/Particles/Additive", "Skybox/Procedural", "Sprites/Default", "Unlit/Color" })
                {
                    var sh = Shader.Find(n); if (sh == null) { Debug.LogWarning("[TimeGate] Shader missing: " + n); continue; }
                    bool has = false; for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) has = true;
                    if (has) continue; arr.arraySize++; arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                }
                so.ApplyModifiedProperties();
            }
            catch (Exception e) { Debug.LogWarning("[TimeGate] Shader pinning failed: " + e.Message); }
        }

        static void SetIcon()
        {
            try
            {
                const string p = "Assets/Icons/app_icon_1024.png";
                AssetDatabase.ImportAsset(p);
                var imp = (TextureImporter)AssetImporter.GetAtPath(p); imp.textureCompression = TextureImporterCompression.Uncompressed; imp.SaveAndReimport();
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { tex }, IconKind.Application);
            }
            catch (Exception e) { Debug.LogWarning("[TimeGate] Icon setup failed: " + e.Message); }
        }
    }
}
