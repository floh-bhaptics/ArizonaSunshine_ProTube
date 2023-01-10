using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


using MelonLoader;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Threading;

[assembly: MelonInfo(typeof(ArizonaSunshine_ProTube.ArizonaSunshine_ProTube), "ArizonaSunshine_ProTube", "1.0.1", "Florian Fahrenberger")]
[assembly: MelonGame("Vertigo Games", "ArizonaSunshine")]

namespace ArizonaSunshine_ProTube
{
    public class ArizonaSunshine_ProTube : MelonMod
    {
        public static string configPath = Directory.GetCurrentDirectory() + "\\UserData\\";
        public static bool dualWield = false;

        public override void OnInitializeMelon()
        {
            InitializeProTube();
        }

        public static void saveChannel(string channelName, string proTubeName)
        {
            string fileName = configPath + channelName + ".pro";
            File.WriteAllText(fileName, proTubeName, Encoding.UTF8);
        }

        public static string readChannel(string channelName)
        {
            string fileName = configPath + channelName + ".pro";
            if (!File.Exists(fileName)) return "";
            return File.ReadAllText(fileName, Encoding.UTF8);
        }

        public static void dualWieldSort()
        {
            ForceTubeVRInterface.FTChannelFile myChannels = JsonConvert.DeserializeObject<ForceTubeVRInterface.FTChannelFile>(ForceTubeVRInterface.ListChannels());
            var pistol1 = myChannels.channels.pistol1;
            var pistol2 = myChannels.channels.pistol2;
            if ((pistol1.Count > 0) && (pistol2.Count > 0))
            {
                dualWield = true;
                MelonLogger.Msg("Two ProTube devices detected, player is dual wielding.");
                if ((readChannel("rightHand") == "") || (readChannel("leftHand") == ""))
                {
                    MelonLogger.Msg("No configuration files found, saving current right and left hand pistols.");
                    saveChannel("rightHand", pistol1[0].name);
                    saveChannel("leftHand", pistol2[0].name);
                }
                else
                {
                    string rightHand = readChannel("rightHand");
                    string leftHand = readChannel("leftHand");
                    MelonLogger.Msg("Found and loaded configuration. Right hand: " + rightHand + ", Left hand: " + leftHand);
                    // Channels 4 and 5 are ForceTubeVRChannel.pistol1 and pistol2
                    ForceTubeVRInterface.ClearChannel(4);
                    ForceTubeVRInterface.ClearChannel(5);
                    ForceTubeVRInterface.AddToChannel(4, rightHand);
                    ForceTubeVRInterface.AddToChannel(5, leftHand);
                }
            }
        }

        private static void InitializeProTube()
        {
            MelonLogger.Msg("Initializing ProTube gear...");
            ForceTubeVRInterface.InitAsync(true);
            Thread.Sleep(10000);
            dualWieldSort();
        }


        [HarmonyPatch(typeof(Gun), "ShootBullet", new Type[] { })]
        public class bhaptics_ShootHaptics
        {
            [HarmonyPostfix]
            public static void Postfix(Gun __instance, bool __result)
            {
                if (!__result) return;
                byte kickPower = 200;
                switch (__instance.AmmoType)
                {
                    case E_AMMO_TYPE.AMMO_BULLET:
                        kickPower = 200;
                        break;
                    case E_AMMO_TYPE.AMMO_MACHINE:
                        kickPower = 200;
                        break;
                    case E_AMMO_TYPE.AMMO_SHELL:
                        kickPower = 230;
                        break;
                    case E_AMMO_TYPE.AMMO_GRENADE:
                        kickPower = 170;
                        break;
                    case E_AMMO_TYPE.AMMO_SNIPER:
                        kickPower = 255;
                        break;
                }
                ForceTubeVRChannel myChannel = ForceTubeVRChannel.pistol1;
                if (!(__instance.EquipmentSlot.SlotID == E_EQUIPMENT_SLOT_ID.RIGHT_HAND))
                    myChannel = ForceTubeVRChannel.pistol2;
                ForceTubeVRInterface.Kick(kickPower, myChannel);
            }
        }
    }
}
