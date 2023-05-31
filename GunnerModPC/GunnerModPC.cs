using GunnerModPC;
using MelonLoader;
using HarmonyLib;
using System.Reflection;
using GHPC.Player;
using GHPC;
using GHPC.UI;
using UnityEngine;
using System.Linq;

[assembly: MelonInfo(typeof(GMPC), "Gunner, Mod, PC!", "1.1.0", "Andrix")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace GunnerModPC
{
    public class GMPC : MelonMod
    {
        public static MelonPreferences_Category config;
        public static MelonPreferences_Entry<bool> fpsPatchEnabled;
        public static MelonPreferences_Entry<bool> shotStoryPatchEnabled;
        public static MelonPreferences_Entry<bool> theaterDropdownPatchEnabled;
        public static MelonPreferences_Entry<bool> thirdPersonCrosshairPatchEnabled;
        public static MelonPreferences_Entry<bool> t3485GrafenwoehrPatchEnabled;

        public override void OnInitializeMelon()
        {
            config = MelonPreferences.CreateCategory("GMPCConfig");
            fpsPatchEnabled = config.CreateEntry<bool>("fpsPatchEnabled", true);
            shotStoryPatchEnabled = config.CreateEntry<bool>("shotStoryPatchEnabled", true);
            theaterDropdownPatchEnabled = config.CreateEntry<bool>("theaterDropdownPatchEnabled", true);
            thirdPersonCrosshairPatchEnabled = config.CreateEntry<bool>("thirdPersonCrosshairPatchEnabled", true);
            t3485GrafenwoehrPatchEnabled = config.CreateEntry<bool>("t3485GrafenwoehrPatchEnabled", true);

            HarmonyLib.Harmony harmony = this.HarmonyInstance;

            if (shotStoryPatchEnabled.Value)
            {
                harmony.PatchAll(typeof(ReportShotStoryPatch));
                LoggerInstance.Msg("Shot story patch activated!");
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Loaded scene {sceneName}, trying to patch game...");

            if (fpsPatchEnabled.Value)
            {
                HUDFPS fpsCounter = Object.FindObjectOfType<HUDFPS>();
                if (fpsCounter != null )
                {
                    fpsCounter.SetActive(true);
                    LoggerInstance.Msg("FPS counter activated!");
                }
                else
                {
                    LoggerInstance.Msg("HUDFPS object not found, FPS counter could not be activated!");
                }
            }

            if (sceneName == "MainMenu2_Scene" && theaterDropdownPatchEnabled.Value)
            {
                MissionMenuSetup missionMenuSetup = Object.FindAnyObjectByType<MissionMenuSetup>();
                if (missionMenuSetup != null)
                {
                    FieldInfo _theaterDropdown = typeof(MissionMenuSetup).GetField("_theaterDropdown", BindingFlags.Instance | BindingFlags.NonPublic);
                    TMPro.TMP_Dropdown theaterDropdown = (TMPro.TMP_Dropdown)_theaterDropdown.GetValue(missionMenuSetup);
                    theaterDropdown.template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10000);
                    LoggerInstance.Msg("Theatre dropdown menu patch activated!");
                }
                else
                {
                    LoggerInstance.Msg("MissionMenuSetup object not found, theatre dropdown menu patch could not be activated!");
                }
            }

            if (shotStoryPatchEnabled.Value)
            {
                ReportShotStoryPatch.playerInput = Object.FindObjectOfType<PlayerInput>();
            }

            if (thirdPersonCrosshairPatchEnabled.Value)
            {
                var aimReticle = GameObject.Find("3P aim reticle");
                if (aimReticle != null)
                {
                    aimReticle.SetActive(false);
                    LoggerInstance.Msg("3rd person crosshair removed!");
                }
                else
                {
                    LoggerInstance.Msg("3rd person crosshair object not found, 3rd person crosshair patch could not be activated!");
                }
            }

            if (sceneName == "TR01_showcase" && t3485GrafenwoehrPatchEnabled.Value)
            {
                // Have to do this because of the HideAndDontSave flag
                var t3485 = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == "T-34-85").First() as GameObject;
                if (t3485 != null)
                {
                    var vehicle = t3485.GetComponent<GHPC.Vehicle.Vehicle>();
                    if (vehicle != null)
                    {
                        vehicle.Allegiance = Faction.Neutral;
                        GameObject.Instantiate(t3485, new Vector3(1179f, 22f, 1614f), new Quaternion(0f, 0.8f, 0f, -0.8f));
                        LoggerInstance.Msg("T-34-85 successfully spawned!");
                    }
                    else
                    {
                        LoggerInstance.Error("Could not find Vehicle component in T-34-85 GameObject!");
                    }
                }
                else
                {
                    LoggerInstance.Error("T-34-85 object not found, T-34-85 Grafenwoehr patch could not be activated!");
                }
            }
        }
    }

    class ReportShotStoryPatch
    {
        static public PlayerInput playerInput;
        static FieldInfo _shotStoryLifeRemaining;
        static FieldInfo _aarController;

        static ReportShotStoryPatch()
        {
            // playerInput is reloaded on every scene load
            _shotStoryLifeRemaining = typeof(PlayerInput).GetField("_shotStoryLifeRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
            _aarController = typeof(PlayerInput).GetField("_aarController", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "ReportShotStory")]
        [HarmonyPrefix]
        static bool ReportShotStoryPrefix(GHPC.Weapons.LiveRound __instance)
        {
            if (__instance.Story != null && __instance.ShotInfo.IsPlayerShot &&
                __instance.IsSpall == false && __instance.ParentRound == null &&
                __instance.Story.ShotNumber > 0)
            {
                _shotStoryLifeRemaining.SetValue(playerInput, 5);
                AarController aac = (AarController)_aarController.GetValue(playerInput);
                aac.ShotStoryTextBox.text = __instance.Story.FinalString;
            }

            return false;
        }

        [HarmonyPatch(typeof(AarController), "Awake")]
        [HarmonyPostfix]
        static void AarControllerAwakePostfix(AarController __instance)
        {
            __instance.ShotStoryTextBox?.gameObject.SetActive(true);

        }
    }
}
