using BepInEx;
using BepInEx.Configuration;
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
        private static ConfigEntry<bool> localPosition;
        private static ConfigEntry<bool> localPositionLog;
        private static ConfigEntry<bool> loseHealth;
        private static ConfigEntry<bool> powerupTimer;
        private static ConfigEntry<bool> powerupTimerMax;
        private static ConfigEntry<bool> applyDamage;
        private static ConfigEntry<bool> enemy_Bullet_Update;

        public override void Load()
        {
            log = Log;
            // Plugin startup logic
            Log.LogMessage($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            instance?.UnpatchSelf();
            instance =Harmony.CreateAndPatchAll(typeof(Plugin));

            loseHealth = Config.Bind("Gameplay", "loseHealth", true);
            localPosition=Config.Bind("Player_Ball", "localPosition", true);
            localPositionLog = Config.Bind("Player_Ball", "log", true);
            powerupTimer = Config.Bind("Player01", "powerupTimer", true);
            powerupTimerMax = Config.Bind("Player01", "powerupTimerMax", true);
            applyDamage = Config.Bind("BrickTrap", "applyDamage", true);
            enemy_Bullet_Update = Config.Bind("enemy_Bullet", "Update", true);

            loseHealth.SettingChanged += LoseHealth_SettingChanged; ;
            localPosition.SettingChanged += LocalPosition_SettingChanged; ;
            localPositionLog.SettingChanged += LocalPositionLog_SettingChanged;
        }

        private void LocalPosition_SettingChanged(object sender, System.EventArgs e)
        {
            Log.LogMessage($"localPosition {localPosition.Value}");
        }

        private void LoseHealth_SettingChanged(object sender, System.EventArgs e)
        {
            Log.LogMessage($"loseHealth {loseHealth.Value}");
        }

        private void LocalPositionLog_SettingChanged(object sender, System.EventArgs e)
        {
            Log.LogMessage($"localPositionLog {localPositionLog.Value}");
        }

        public override bool Unload()
        {
            Log.LogMessage($"Plugin {PluginInfo.PLUGIN_GUID} is Unload!");

            instance?.UnpatchSelf();

            return false;
        }

        // Gameplay

        /// <summary>
        /// succ
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Gameplay), "LoseHealth")]
        [HarmonyPrefix]
        public static void LoseHealth(Gameplay __instance)
        {
            if (!loseHealth.Value)
                return;
            log.LogInfo($"Gameplay.LoseHealth");
            __instance.AddHealth();
        }        
        
        /// <summary>
        /// succ
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Enemy_Bullet), "Update")]
        [HarmonyPrefix]
        public static void Enemy_Bullet_Update(Enemy_Bullet __instance)
        {
            if (!enemy_Bullet_Update.Value)
                return;
            var v = __instance.transform.localPosition;
            log.LogInfo($"Enemy_Bullet.Update {v}");
            v.y = -4f;
            __instance.transform.localPosition = v;
        }

        //[HarmonyPatch(typeof(BrickTurret), "ApplyDamage")]
        //[HarmonyPatch(typeof(BrickTrap), "ApplyDamage")]
        //[HarmonyPrefix]
        //public static bool ApplyDamage()
        //{
        //    if (!applyDamage.Value)
        //        return true;
        //
        //    log.LogInfo($"BrickTrap.ApplyDamage ");
        //    return false;
        //}

        #region Player_Ball

        /// <summary>
        /// succ??? fail
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(Player_Ball), "showLine", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool showLine(Player_Ball __instance, ref bool __result)
        {
            log.LogInfo($"Player_Ball.showLine {__result}");
            __result = true;
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
            UnhollowerBaseLib.Il2CppReferenceArray<GameObject> g = GameObject.FindGameObjectsWithTag("Player_Ball");
            if (g != null)
            {
                log.LogInfo($"Player_Ball.Start {g.Count}");
                foreach (GameObject item in g)
                {
                    item.transform.localScale *= 2;
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

            if (v.x < -10 || v.x > 10|| v.y > 10)
            {
                v.x = 0;
                v.y = 0;
                __instance.transform.localPosition = v;
            }

            if (localPositionLog.Value)
                log.LogInfo($"Player_Ball.Update {v}");

            if (!localPosition.Value)
                return;

            // -3.3 3.7
            if (v.y <= -3.4)
            {
                v.y = 3.7F;
                __instance.transform.localPosition = v;
            }
        }
        
        [HarmonyPatch(typeof(Player_Ball), "hasOrbiting",MethodType.Setter)]
        [HarmonyPostfix]
        public static void Player_Ball_hasOrbitingSetter(Player_Ball __instance, bool __0)
        {
            log.LogInfo($"Player_Ball.hasOrbiting Setter {__0} , {__instance.maxHits}");
        }

        [HarmonyPatch(typeof(Player_Ball), "hasOrbiting",MethodType.Getter)]
        [HarmonyPostfix]
        public static void Player_Ball_hasOrbitingGetter(Player_Ball __instance, bool __result)
        {
            log.LogInfo($"Player_Ball.hasOrbiting Getter {__result}, {__instance.maxHits}");
        }

        [HarmonyPatch(typeof(Player_Ball), "ClearOrbitingBullets")]
        [HarmonyPostfix]
        public static void Player_Ball_ClearOrbitingBullets(Player_Ball __instance)
        {
            log.LogInfo($"Player_Ball.ClearOrbitingBullets {__instance.maxHits}");
        }

        [HarmonyPatch(typeof(Player_Ball), "SetupOrbitingBullets")]
        [HarmonyPostfix]
        public static void Player_Ball_SetupOrbitingBullets(Player_Ball __instance)
        {
            log.LogInfo($"Player_Ball.SetupOrbitingBullets {__instance.maxHits}");
        }

        [HarmonyPatch(typeof(Player_Ball), "isNotMultiBallClone", MethodType.Setter)]
        [HarmonyPostfix]
        public static void Player_Ball_isNotMultiBallCloneSetter(Player_Ball __instance, bool __0)
        {
            log.LogInfo($"Player_Ball.isNotMultiBallClone Setter {__0} , {__instance.maxHits}");
        }

        [HarmonyPatch(typeof(Player_Ball), "isNotMultiBallClone", MethodType.Getter)]
        [HarmonyPostfix]
        public static void Player_Ball_isNotMultiBallCloneGetter(Player_Ball __instance, bool __result)
        {
            log.LogInfo($"Player_Ball.isNotMultiBallClone Getter {__result}, {__instance.maxHits}");
        }

        #endregion

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

        #region Player01


        [HarmonyPatch(typeof(Player01), "ProccessPickup")]
        [HarmonyPostfix]
        public static void ProccessPickup(Player01 __instance)
        {
            log.LogInfo($"Player01.ProccessPickup {__instance.powerupTimer} , {__instance.powerupTimerMax}");
            if (powerupTimerMax.Value)
                __instance.powerupTimerMax *= 100;
                
        } 
        

        //[HarmonyPatch(typeof(Player01), "powerupTimer", MethodType.Setter)]
        //[HarmonyPrefix]
        public static void powerupTimerPre(Player01 __instance, float __0)
        {
            if (!powerupTimer.Value)
                return;

            log.LogInfo($"Player01.powerupTimer {__0}");            
            __instance.powerupTimer = 0;
        }

        /// <summary>
        /// succ??
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__0"></param>
        //[HarmonyPatch(typeof(Player01), "powerupTimerMax", MethodType.Setter)]
        //[HarmonyPrefix]
        public static void powerupTimerMaxPre(Player01 __instance, float __0)
        {
            if (!powerupTimerMax.Value)
                return;

            log.LogInfo($"Player01.powerupTimerMax {__0}");
            __0 *= 100;
            __instance.powerupTimerMax = __0;            
        }

        #endregion
    }
}
