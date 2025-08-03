using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FasterSaveDeletion;

[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{

    internal static new ManualLogSource Logger;

    public void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.GUID} is loaded!");
        Harmony harmony = new(PluginInfo.GUID);
        harmony.Patch(
            original: typeof(SaveProfiles).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
            postfix: new HarmonyMethod(typeof(Save_Profiles_Patch), nameof(Save_Profiles_Patch.Faster_Deletion))
        );
    }

    public class Save_Profiles_Patch
    {
        public static void Faster_Deletion(SaveProfiles __instance)
        {
            // Harmony reflection logic
            var type = __instance.GetType();
            var clearTimeField = AccessTools.Field(type, "clearTime");
            if (clearTimeField == null) return;

            float clearTime = (float)clearTimeField.GetValue(__instance);
            if (Game.inputs.h < -0.5f)
            {
                // Speed up the save deletion, was originally Time.unscaledDeltaTime * 0.3333
                float newClearTime = Mathf.MoveTowards(clearTime, 1f, Time.unscaledDeltaTime);
                clearTimeField.SetValue(__instance, newClearTime);
            }
        }
    }
}