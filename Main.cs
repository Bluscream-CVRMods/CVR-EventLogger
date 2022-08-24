using MelonLoader;
using EventLogger;
using UnityEngine;
using Main = EventLogger.Main;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;
using ABI_RC.Core.Player;

[assembly: MelonInfo(typeof(Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace EventLogger;

public static class Guh
{
    public const string Name = "Event Logger";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}

public class Main : MelonMod
{
    public MelonPreferences_Entry LogJoinLeavesSetting;
    public MelonPreferences_Entry LogWorldsSetting;
    public MelonPreferences_Entry LogAvatarChangesSetting;

    public override void OnPreferencesLoaded(string filepath) {
        MelonLogger.Msg("OnPreferencesLoaded: {0}", filepath);
    }
    public override void OnPreferencesSaved(string filepath) {
        MelonLogger.Msg("OnPreferencesSaved: {0}", filepath);
    }

    public override void OnPreSupportModule() {
        MelonLogger.Msg("OnPreSupportModule");
    }
    public override void OnApplicationStart() {
        var cat = MelonPreferences.CreateCategory(Guh.Name);
        LogWorldsSetting = cat.CreateEntry<bool>("LogWorlds", true, "Log World Joins/Leaves");
        LogJoinLeavesSetting = cat.CreateEntry<bool>("LogJoinLeaves", true, "Log Player Joins/Leaves");
        LogAvatarChangesSetting = cat.CreateEntry<bool>("LogAvatarChanges", true, "Log Avatar switching");

        ButtonAPI.OnInit += ButtonAPI_OnInit;
        ButtonAPI.OnPlayerJoin += OnPlayerJoin;
        ButtonAPI.OnPlayerLeave += OnPlayerLeave;
        ButtonAPI.OnAvatarInstantiated_Pre_E += OnAvatarInstantiated_Pre_E;
        ButtonAPI.OnAvatarInstantiated_Post_E += OnAvatarInstantiated_Post_E;
    }
    public override void OnApplicationLateStart() {
        MelonLogger.Msg("OnApplicationLateStart");
    }

    private void ButtonAPI_OnInit() {
        MelonLogger.Msg("ButtonAPI_OnInit");
    }

    private void OnPlayerJoin(PlayerDescriptor player) {
        if ((bool)LogJoinLeavesSetting.BoxedValue)
            MelonLogger.Msg(!string.IsNullOrEmpty(player.userName) ? $"\"{player.userName}\" ({player.ownerId}) joined" : "Local Player Init");
    }
    private void OnAvatarInstantiated_Post_E(PuppetMaster arg1, GameObject arg2) {
        if ((bool)LogAvatarChangesSetting.BoxedValue)
            MelonLogger.Msg($"OnAvatarInstantiated_Post_E {arg1.name}");
    }
    private bool OnAvatarInstantiated_Pre_E(PuppetMaster arg1, GameObject arg2) {
        if ((bool)LogAvatarChangesSetting.BoxedValue)
            MelonLogger.Msg("OnAvatarInstantiated_Pre_E");
        return true;
    }
    private void OnPlayerLeave(PlayerDescriptor player) {
        if ((bool)LogJoinLeavesSetting.BoxedValue)
            MelonLogger.Msg(!string.IsNullOrEmpty(player.userName) ? $"\"{player.userName}\" ({player.ownerId}) lft" : "Local Player Leave");
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue)
            MelonLogger.Msg("OnSceneWasInitialized: \"{0}\" (1)", sceneName, buildIndex);
    }
    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue)
            MelonLogger.Msg("OnSceneWasLoaded: \"{0}\" (1)", sceneName, buildIndex);
    }
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue)
            MelonLogger.Msg("OnSceneWasUnloaded: \"{0}\" (1)", sceneName, buildIndex);
    }

    public override void OnApplicationQuit() {
        MelonLogger.Msg("OnApplicationQuit");
    }
}