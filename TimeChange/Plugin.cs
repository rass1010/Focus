using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using Utilla;

namespace TimeChange
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        public bool inAllowedRoom = false;
        public bool isPressingB = false;
        public bool justPressedB = false;
        public bool hauntedModMenuEnabled = false;

        public float timeScale;
        public float deltaTimeScale;
        public float slowMotionSpeed;

        public static ConfigEntry<float> sms;


        void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "slowmotion.cfg"), true);
            sms = customFile.Bind("Configuration", "Slow Motion Speed", 0.5f, "slow motion speed");

            Utilla.Events.GameInitialized += GameInitialized;
        }

        void Update()
        {
            if (inAllowedRoom && hauntedModMenuEnabled)
            {

                List<InputDevice> list = new List<InputDevice>();
                InputDevices.GetDevices(list);

                for (int i = 0; i < list.Count; i++) //Get input
                {
                    if (list[i].characteristics.HasFlag(InputDeviceCharacteristics.Right))
                    {
                        list[i].TryGetFeatureValue(CommonUsages.secondaryButton, out isPressingB);
                    }
                }

                if (isPressingB)
                {
                    if (!justPressedB)
                    {
                        StartSlowMo();
                        justPressedB = true;
                    }
                }
                else
                {
                    if (justPressedB)
                    {
                        StopSlowMo();
                        justPressedB = false;
                    }
                }

            }
            
        }

        [ModdedGamemodeJoin]
        public void RoomJoined(string gamemode)
        {
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        public void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
            StopSlowMo();
        }

        public void StartSlowMo()
        {
            Time.timeScale = timeScale * slowMotionSpeed;
            Time.fixedDeltaTime = deltaTimeScale * slowMotionSpeed;
        }

        public void StopSlowMo()
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = deltaTimeScale;
        }

        void OnEnable()
        {
            hauntedModMenuEnabled = true;
        }

        void OnDisable()
        {
            hauntedModMenuEnabled = false;
            StopSlowMo();
        }

        private void GameInitialized(object sender, EventArgs e)
        {
            // Player instance has been created
            slowMotionSpeed = sms.Value;
            timeScale = Time.timeScale;
            deltaTimeScale = Time.fixedDeltaTime;
        }
    }
}
