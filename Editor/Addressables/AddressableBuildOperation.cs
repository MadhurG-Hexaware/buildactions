using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperUnityBuild.BuildTool;
using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;

public class AddressableBuildOperation : BuildAction, IPreBuildPerPlatformAction
{
    public static string build_script
            = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";

    public static string settings_asset
        = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

    public string profile_name = "Default";
    private static AddressableAssetSettings settings;

    public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime, ref BuildOptions options, string configKey, string buildPath)
    {
        base.PerBuildExecute(releaseType, platform, architecture, scriptingBackend, distribution, buildTime, ref options, configKey, buildPath);

        if (!platform.enabled || !architecture.enabled)
                return;

         // Build addressable assets
        #if UNITY_5_6_OR_NEWER
            EditorUserBuildSettings.SwitchActiveBuildTarget(platform.targetGroup, architecture.target);
        #else
            EditorUserBuildSettings.SwitchActiveBuildTarget(arch.target);
        #endif

        BuildNow();
    }

    protected override void DrawProperties(SerializedObject obj)
    {
        //base.DrawProperties(obj);
        EditorGUILayout.PropertyField(obj.FindProperty("profile_name"));
        if (GUILayout.Button("Run Now", GUILayout.ExpandWidth(true)))
             BuildNow();
    }

    private void BuildNow()
    {
        getSettingsObject(settings_asset);
        setProfile(profile_name);
        IDataBuilder builderScript
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

        if (builderScript == null)
        {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return;
        }

        setBuilder(builderScript);

         AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }


    }
    #region internal
    static void getSettingsObject(string settingsAsset)
        {
            // This step is optional, you can also use the default settings:
            //settings = AddressableAssetSettingsDefaultObject.Settings;

            settings
                = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                    as AddressableAssetSettings;

            if (settings == null)
                Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                               $"a settings object.");
        }



        static void setProfile(string profile)
        {
            string profileId = settings.profileSettings.GetProfileId(profile);
            if (String.IsNullOrEmpty(profileId))
                Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                                 $"using current profile instead.");
            else
                settings.activeProfileId = profileId;
        }



        static void setBuilder(IDataBuilder builder)
        {
            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
                settings.ActivePlayerDataBuilderIndex = index;
            else
                Debug.LogWarning($"{builder} must be added to the " +
                                 $"DataBuilders list before it can be made " +
                                 $"active. Using last run builder instead.");
        }
        #endregion
}