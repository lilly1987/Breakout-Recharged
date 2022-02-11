using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace MyFirstPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        Harmony instance;
        private static ManualLogSource log;

        public override void Load()
        {
            log = Log;
            // Plugin startup logic
            Log.LogMessage($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            instance?.UnpatchSelf();
            instance =Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public override bool Unload()
        {
            Log.LogMessage($"Plugin {PluginInfo.PLUGIN_GUID} is Unload!");

            instance?.UnpatchSelf();

            return false;
        }

        /// <summary>
        /// succ
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Gameplay), "LoseHealth")]
        [HarmonyPrefix]
        public static void LoseHealth(Gameplay __instance)
        {
            log.LogInfo($"Gameplay.LoseHealth");
            __instance.AddHealth();
        }
        
        /// <summary>
        /// succ???
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(Player_Ball), "showLine",MethodType.Getter)]
        [HarmonyPrefix]
        public static bool showLine(Player_Ball __instance,ref bool __result)
        {
            log.LogInfo($"Player_Ball.showLine {__result}");
            __result=true;
            __instance.showLine = true;
            return false;
        }

        /// <summary>
        /// fail
        /// </summary>
        /// <param name="__instance"></param>
        //[HarmonyPatch(typeof(Player_Ball), "Start")]
        //[HarmonyPostfix]
        public static void Player_Ball_Start(Player_Ball __instance)
        {
            log.LogInfo($"Player_Ball.Start");
            __instance.transform.localScale.Set(2, 2, 2);// not run
            UnhollowerBaseLib.Il2CppReferenceArray<GameObject> g =GameObject.FindGameObjectsWithTag("Player_Ball");
            if (g != null)
            {
                log.LogInfo($"Player_Ball.Start {g.Count}");
                foreach (GameObject item in g)
                {
                    item.transform.localScale.Set(2, 2, 2);
                }
            }
        }

        /// <summary>
        /// succ
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Player_Ball), "Update")]
        [HarmonyPostfix]
        public static void Player_Ball_Update(Player_Ball __instance)
        {
            var v = __instance.transform.localPosition;
            // -3.3 3.7
            //log.LogInfo($"Player_Ball.Update {v}");
            if (v.y<=-3.4)
            {
                v.y = 3.7F;
                __instance.transform.localPosition=v;
            }
        }

        /// <summary>
        /// fail
        /// </summary>
        /// <param name="__instance"></param>
        //[HarmonyPatch(typeof(PoolManager), "PickupsRoot", MethodType.Setter)]
        //[HarmonyPostfix]
        public static void PoolManager_PickupsRoot(PoolManager __instance)
        {            
            Transform t =__instance.PickupsRoot;
            if (t != null)
            {
                log.LogInfo($"PoolManager.PickupsRoot {t.childCount}");
                for (int i = 0; i < t.childCount; i++)
                {
                    var g=t.GetChild(i).gameObject ;
                    if (g.activeSelf)
                    {
                        g.transform.localScale.Set(10,10,10);
                    }
                }
            }
        }
    }
}
