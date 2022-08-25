using ABI.CCK.Components;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Player;
using EventLogger;
using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using UnityEngine;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;
using Main = EventLogger.Main;

[assembly: MelonInfo(typeof(Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace EventLogger;

public static class Guh {
    public const string Name = "EventLogger";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}

public static class Patches {
    public static void Patch(HarmonyLib.Harmony harmonyInstance) {
        harmonyInstance.Patch(typeof(CVRVideoPlayer).GetMethod("SetVideoUrl"), prefix: new HarmonyMethod(typeof(Patches).GetMethod("OnSetVideoUrl", BindingFlags.Static | BindingFlags.NonPublic)));
        harmonyInstance.Patch(typeof(ABI_RC.Core.Networking.IO.Instancing.Instances).GetMethod("SetJoinTarget"), postfix: new HarmonyMethod(typeof(Patches).GetMethod("SetJoinTarget")));
        MelonLogger.Msg("Harmony patches completed!");
    }
    private static bool OnSetVideoUrl(string url, bool broadcast = true, string objPath = "", string username = null, bool isPaused = false) {
        if ((bool)Main.LogVideoPlayerSetting.BoxedValue) MelonLogger.Msg($"VideoPlayer URL changed: {url}");
        return true;
    }
    public static void SetJoinTarget(string instanceId, string worldId) {
        if ((bool)Main.LogWorldsSetting.BoxedValue) MelonLogger.Msg("Joining Instance {0}:{1}", worldId, instanceId);
    }
}

public class Main : MelonMod {
    public bool fully_loaded = false;
    public MelonPreferences_Entry LogJoinLeavesSetting, LogAvatarChangesSetting, LogPreferencesSetting;
    public static MelonPreferences_Entry LogWorldsSetting, LogVideoPlayerSetting;


    public override void OnPreferencesLoaded(string filepath) {
        if ((bool)LogPreferencesSetting.BoxedValue) {
            LoggerInstance.Msg("OnPreferencesLoaded: {0}", filepath);
        }
    }
    public override void OnPreferencesSaved(string filepath) {
        if ((bool)LogPreferencesSetting.BoxedValue) {
            LoggerInstance.Msg("OnPreferencesSaved: {0}", filepath);
        }
    }

    public override void OnPreSupportModule() {
        LoggerInstance.Msg("OnPreSupportModule");
        LoggerInstance.Msg(Environment.CommandLine);
    }
    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        LogWorldsSetting = cat.CreateEntry<bool>("LogWorlds", true, "Log World Joins/Leaves");
        LogJoinLeavesSetting = cat.CreateEntry<bool>("LogJoinLeaves", true, "Log Player Joins/Leaves");
        LogAvatarChangesSetting = cat.CreateEntry<bool>("LogAvatarChanges", true, "Log Avatar switching");
        LogPreferencesSetting = cat.CreateEntry<bool>("LogPreferences", false, "Log Saving/Loading of MelonPrefs");
        LogVideoPlayerSetting = cat.CreateEntry<bool>("LogVideoPlayer", false, "Log Video Player Events");

        ButtonAPI.OnInit += ButtonAPI_OnInit;
        ButtonAPI.OnPlayerJoin += OnPlayerJoin;
        ButtonAPI.OnPlayerLeave += OnPlayerLeave;
        ButtonAPI.OnAvatarInstantiated_Pre_E += OnAvatarInstantiated_Pre_E;
        ButtonAPI.OnAvatarInstantiated_Post_E += OnAvatarInstantiated_Post_E;

        Patches.Patch(HarmonyInstance);
    }

    private void GameNetwork_Disconnected(object sender, DarkRift.Client.DisconnectedEventArgs e) => OnDisconnected("GameNetwork", sender, e);
    private void CallsNetwork_Disconnected(object sender, DarkRift.Client.DisconnectedEventArgs e) => OnDisconnected("CallsNetwork", sender, e);
    private void Api_Disconnected(object sender, DarkRift.Client.DisconnectedEventArgs e) => OnDisconnected("API", sender, e);
    private void OnDisconnected(string network, object sender, DarkRift.Client.DisconnectedEventArgs e) {
        if (e == null) return;
        LoggerInstance.Msg("{0} {1} Disconnected: {1}", (e.LocalDisconnect ? "[LOCAL]" : ""), network, e.Exception.Message);
    }
    private void OnMessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e) {
    }

    public override void OnApplicationLateStart() {
        LoggerInstance.Msg("OnApplicationLateStart");
    }

    private void ButtonAPI_OnInit() {
        LoggerInstance.Msg("ButtonAPI_OnInit");
    }

    private void OnPlayerJoin(PlayerDescriptor player) {
        if ((bool)LogJoinLeavesSetting.BoxedValue) {
            LoggerInstance.Msg(!string.IsNullOrEmpty(player.userName) ? $"\"{player.userName}\" ({player.ownerId}) joined" : "You joined");
        }
    }
    private bool OnAvatarInstantiated_Pre_E(PuppetMaster arg1, GameObject arg2) {
        if ((bool)LogAvatarChangesSetting.BoxedValue) {
            LoggerInstance.Msg($"OnAvatarInstantiated_Pre_E ({arg1.name}) ({arg2.name})");
        }

        return true;
    }
    private void OnAvatarInstantiated_Post_E(PuppetMaster arg1, GameObject arg2) {
        if ((bool)LogAvatarChangesSetting.BoxedValue) {
            LoggerInstance.Msg($"OnAvatarInstantiated_Post_E ({arg1.name}) ({arg2.name})");
        }
    }
    private void OnPlayerLeave(PlayerDescriptor player) {
        if ((bool)LogJoinLeavesSetting.BoxedValue) {
            LoggerInstance.Msg(!string.IsNullOrEmpty(player.userName) ? $"\"{player.userName}\" ({player.ownerId}) left" : "You left");
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue) {
            LoggerInstance.Msg("OnSceneWasLoaded: \"{0}\" ({1})", sceneName, buildIndex);
        }
    }
    public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue) {
            LoggerInstance.Msg("OnSceneWasInitialized: \"{0}\" ({1})", sceneName, buildIndex);
        }
        if (!fully_loaded && sceneName == "Init") {
            fully_loaded = true;
            OnGameFullyLoaded();
        }
    }
    public void OnGameFullyLoaded() {
        LoggerInstance.Msg("OnGameFullyLoaded");
        try {
            NetworkManager.Instance.GameNetwork.Disconnected += GameNetwork_Disconnected;
            // NetworkManager.Instance.GameNetwork.MessageReceived += OnMessageReceived;
            NetworkManager.Instance.Api.Disconnected += Api_Disconnected;
            NetworkManager.Instance.CallsNetwork.Disconnected += CallsNetwork_Disconnected;
        } catch (Exception ex) {
            LoggerInstance.Warning(ex);
        }
    }
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
        if ((bool)LogWorldsSetting.BoxedValue) {
            LoggerInstance.Msg("OnSceneWasUnloaded: \"{0}\" (1)", sceneName, buildIndex);
        }
    }

    public override void OnApplicationQuit() {
        LoggerInstance.Msg("OnApplicationQuit");
    }
}