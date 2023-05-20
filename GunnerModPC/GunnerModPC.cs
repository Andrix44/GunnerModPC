using GunnerModPC;
using MelonLoader;
using HarmonyLib;
using System.Reflection;
using GHPC.Player;
using GHPC;
using GHPC.UI;

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
        //private MelonPreferences_Entry<bool> thirdPersonCrosshairPatchEnabled;

        public override void OnInitializeMelon()
        {
            config = MelonPreferences.CreateCategory("GMPCConfig");
            fpsPatchEnabled = config.CreateEntry<bool>("fpsPatchEnabled", true);
            shotStoryPatchEnabled = config.CreateEntry<bool>("shotStoryPatchEnabled", true);
            theaterDropdownPatchEnabled = config.CreateEntry<bool>("theaterDropdownPatchEnabled", true);
            //thirdPersonCrosshairPatchEnabled = config.CreateEntry<bool>("thirdPersonCrosshairPatchEnabled", true);

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
                HUDFPS fpsCounter = UnityEngine.Object.FindObjectOfType<HUDFPS>();
                if (fpsCounter != null )
                {
                    fpsCounter.SetActive(true);
                    LoggerInstance.Msg("FPS counter activated!");
                }
                else
                {
                    LoggerInstance.Warning("HUDFPS object not found, FPS counter could not be activated!");
                }
            }

            if (sceneName == "MainMenu2_Scene" && theaterDropdownPatchEnabled.Value)
            {
                MissionMenuSetup missionMenuSetup = UnityEngine.Object.FindAnyObjectByType<MissionMenuSetup>();
                if (missionMenuSetup != null)
                {
                    FieldInfo _theaterDropdown = typeof(MissionMenuSetup).GetField("_theaterDropdown", BindingFlags.Instance | BindingFlags.NonPublic);
                    TMPro.TMP_Dropdown theaterDropdown = (TMPro.TMP_Dropdown)_theaterDropdown.GetValue(missionMenuSetup);
                    theaterDropdown.template.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Vertical, 10000);
                    LoggerInstance.Msg("Theatre dropdown menu patch activated!");
                }
                else
                {
                    LoggerInstance.Msg("MissionMenuSetup object not found, theatre dropdown menu patch could not be activated!");
                }
            }

            if (shotStoryPatchEnabled.Value)
            {
                ReportShotStoryPatch.playerInput = UnityEngine.Object.FindObjectOfType<PlayerInput>();
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
