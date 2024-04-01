﻿using GunnerModPC;
using MelonLoader;
using HarmonyLib;
using System.Reflection;
using GHPC.Player;
using GHPC;
using GHPC.UI;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using GHPC.Vehicle;
using System.Xml.Linq;

[assembly: MelonInfo(typeof(GMPC), "Gunner, Mod, PC!", "1.6.0", "Andrix")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace GunnerModPC
{
    public class GMPC : MelonMod
    {
        public static MelonPreferences_Category config;
        public static MelonPreferences_Entry<bool> fpsCounter;
        public static MelonPreferences_Entry<bool> shotStory;
        public static MelonPreferences_Entry<bool> theaterDropdown;
        public static MelonPreferences_Entry<bool> customThirdPersonCrosshairColor;
        public static MelonPreferences_Entry<bool> removeThirdPersonCrosshair;
        public static MelonPreferences_Entry<bool> grafenwoehrExtraVehicles;

        public static MelonPreferences_Category cchconfig;
        public static MelonPreferences_Entry<float> crosshairRed;
        public static MelonPreferences_Entry<float> crosshairGreen;
        public static MelonPreferences_Entry<float> crosshairBlue;
        public static MelonPreferences_Entry<float> crosshairAlpha;
        public static Sprite crosshairSprite;
        public static Color crosshairColor;

        public override void OnInitializeMelon()
        {
            config = MelonPreferences.CreateCategory("GMPCConfig");
            grafenwoehrExtraVehicles = config.CreateEntry<bool>("grafenwoehrExtraVehicles", true);
            grafenwoehrExtraVehicles.Description = "Enable/Disable the patch that adds extra vehicles to the Grafenwoehr tank range.";
            shotStory = config.CreateEntry<bool>("shotStory", true);
            shotStory.Description = "Enable/Disable the patch that brings back the live damage report.";
            fpsCounter = config.CreateEntry<bool>("fpsCounter", true);
            fpsCounter.Description = "Enable/Disable the patch that brings back the FPS counter in the bottom left corner.";
            customThirdPersonCrosshairColor = config.CreateEntry<bool>("customCrosshairColor", true);
            customThirdPersonCrosshairColor.Description = "Enable/Disable the patch that sets a custom third person crosshair color. You can change the colors under [GMPCCustomCrosshairColorConfig].";
            theaterDropdown = config.CreateEntry<bool>("theaterDropdown", true);
            theaterDropdown.Description = "Enable/Disable the patch that fixes the theater dropdown menu to not use a scrollbar.";
            removeThirdPersonCrosshair = config.CreateEntry<bool>("removeThirdPersonCrosshair", false);
            removeThirdPersonCrosshair.Description = "Enable/Disable the patch that removes the third person crosshair";

            cchconfig = MelonPreferences.CreateCategory("GMPCCustomCrosshairColorConfig");
            crosshairRed = cchconfig.CreateEntry<float>("red", 1.0f);
            crosshairRed.Description = "Red channel intensity. A float value between 0.0f and 1.0. 1.0 is equal to 255 when using an integer scale.";
            crosshairGreen = cchconfig.CreateEntry<float>("green", 0.25f);
            crosshairGreen.Description = "Green channel intensity. A float value between 0.0f and 1.0. 1.0 is equal to 255 when using an integer scale.";
            crosshairBlue = cchconfig.CreateEntry<float>("blue", 0.0f);
            crosshairBlue.Description = "Blue channel intensity. A float value between 0.0f and 1.0. 1.0 is equal to 255 when using an integer scale.";
            crosshairAlpha = cchconfig.CreateEntry<float>("alpha", 1.0f);
            crosshairAlpha.Description = "Alpha channel intensity. Can be used to make the crosshair transparent. A float value between 0.0f and 1.0. 1.0 is equal to 255 when using an integer scale.";

            HarmonyLib.Harmony harmony = this.HarmonyInstance;

            if (shotStory.Value)
            {
                harmony.PatchAll(typeof(ReportShotStoryPatch));
                LoggerInstance.Msg("Shot story patch activated!");
            }

            if (customThirdPersonCrosshairColor.Value)
            {
                Texture2D texture = new Texture2D(256, 256);
                // This only works if the file extension isn't .png, otherwise it will be converted to a bitmap
                // That is why I changed the extension to .png_ and added it as a binary resource
                ImageConversion.LoadImage(texture, resources.custom_crosshair);
                crosshairSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0f, 0f));
                var components = new List<MelonPreferences_Entry<float>>() { crosshairRed, crosshairGreen, crosshairBlue, crosshairAlpha };
                foreach (var c in components)
                {
                    if (c.Value < 0.0f || c.Value > 1.0f)
                    {
                        LoggerInstance.Warning($"Crosshair color component {c} has an invalid value! Valid values are 0.0-1.0. Defaulting to 1.0.");
                        c.Value = 1.0f;
                    }
                }
                crosshairColor = new Color(crosshairRed.Value, crosshairGreen.Value, crosshairBlue.Value, crosshairAlpha.Value);
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Loaded scene {sceneName}, trying to patch game...");

            if (fpsCounter.Value)
            {
                HUDFPS fpsCounter = GameObject.FindObjectOfType<HUDFPS>();
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

            if (theaterDropdown.Value && (sceneName == "MainMenu2_Scene" || sceneName == "t64_menu" || sceneName == "MainMenu2-1_Scene"))
            {
                MissionMenuSetup missionMenuSetup = GameObject.FindAnyObjectByType<MissionMenuSetup>();
                if (missionMenuSetup != null)
                {
                    FieldInfo _theaterDropdown = typeof(MissionMenuSetup).GetField("_theaterDropdown", BindingFlags.Instance | BindingFlags.NonPublic);
                    TMPro.TMP_Dropdown theaterDropdown = (TMPro.TMP_Dropdown)_theaterDropdown.GetValue(missionMenuSetup);
                    theaterDropdown.template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10000);
                    LoggerInstance.Msg("Theatre dropdown menu patch activated!");
                }
                else
                {
                    LoggerInstance.Error("MissionMenuSetup object not found, theatre dropdown menu patch could not be activated!");
                }
            }

            if (shotStory.Value)
            {
                ReportShotStoryPatch.playerInput = GameObject.FindObjectOfType<PlayerInput>();
            }

            if (removeThirdPersonCrosshair.Value)
            {
                var aimReticle = GameObject.Find("3P aim reticle");
                if (aimReticle != null)
                {
                    aimReticle.SetActive(false);
                    LoggerInstance.Msg("3rd person crosshair removed!");
                }
                else
                {
                    LoggerInstance.Msg("3rd person crosshair object not found, 3rd person crosshair removal patch could not be activated!");
                }
            }

            if (grafenwoehrExtraVehicles.Value && sceneName == "TR01_showcase")
            {
                (string, Vector3)[] grafenwoehrExtraVehicles = { ("T-34-85", new Vector3(1179f, 22f, 1617.5f)),
                                                                 ("T54A", new Vector3(1181f, 23f, 1591f)) };

                foreach (var (name, position)  in grafenwoehrExtraVehicles)
                {
                    // Have to do this because of the HideAndDontSave flag
                    var vehicle = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(o => o.name == name);
                    if (vehicle.Any())
                    {
                        SpawnNeutralVehicle(vehicle.First() as GameObject, position, new Quaternion(0f, 0.8f, 0f, -0.8f));
                    }
                    else
                    {
                        LoggerInstance.Error($"{name} object not found, could not add it to the Grafenwoehr tank range!");
                    }
                }
            }

            if (customThirdPersonCrosshairColor.Value)
            {
                var aimReticle = GameObject.Find("3P aim reticle");
                if (aimReticle != null)
                {
                    UnityEngine.UI.Image image = aimReticle.GetComponent<UnityEngine.UI.Image>();
                    image.sprite = crosshairSprite;
                    image.color = crosshairColor;
                    LoggerInstance.Msg("Custom crosshair color applied!");
                }
                else
                {
                    LoggerInstance.Msg("3rd person crosshair object not found, custom crosshair color patch could not be activated!");
                }
            }
        }

        void SpawnNeutralVehicle(GameObject vehicle, Vector3 position, Quaternion rotation)
        {
            var vehicleComp = vehicle.GetComponent<GHPC.Vehicle.Vehicle>();
            if (vehicle != null)
            {
                vehicleComp.Allegiance = Faction.Neutral;
                GameObject.Instantiate(vehicle, position, rotation);
                LoggerInstance.Msg($"{vehicle.name} successfully spawned at {vehicle.transform.position}!");
            }
            else
            {
                LoggerInstance.Error($"Could not find Vehicle component in {vehicle.name} GameObject!");
            }
        }
    }

    class ReportShotStoryPatch
    {
        static public PlayerInput playerInput;
        static readonly FieldInfo _shotStoryLifeRemaining;
        static readonly FieldInfo _aarController;

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
