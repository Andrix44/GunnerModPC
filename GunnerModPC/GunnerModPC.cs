using GunnerModPC;
using MelonLoader;
using HarmonyLib;
using System.Reflection;
using GHPC.Player;
using GHPC;
using GHPC.UI;

[assembly: MelonInfo(typeof(GMPC), "Gunner, Mod, PC!", "1.0.0", "Andrix")]
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
                harmony.PatchAll(typeof(ReceivedShotStoryPatch));
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
        }
    }

    [HarmonyPatch(typeof(PlayerInput), "ReceivedShotStory")]
    public class ReceivedShotStoryPatch
    {
        public static bool Prefix(ref PlayerInput __instance, Unit sender, GHPC.Weapons.LiveRound.ShotStory story,
                                   ref float ____shotStoryLifeRemaining, ref AarController ____aarController, ref bool ____allowShotStory)
        {
            if (____allowShotStory && sender == __instance.CurrentPlayerUnit)
            {
                ____shotStoryLifeRemaining = 5f;
                ____aarController.ShotStoryTextBox.text = story.FinalString;
            }
            return false;
        }
    }
}
