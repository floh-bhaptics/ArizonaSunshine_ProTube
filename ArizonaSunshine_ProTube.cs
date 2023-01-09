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

[assembly: MelonInfo(typeof(ArizonaSunshine_ProTube.ArizonaSunshine_ProTube), "ArizonaSunshine_ProTube", "1.0.0", "Florian Fahrenberger")]
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
            //MelonLogger.Msg("Channels: " + ForceTubeVRInterface.ListChannels());
            //JsonDocument doc = JsonDocument.Parse(ForceTubeVRInterface.ListChannels());
            //JsonElement pistol1 = doc.RootElement.GetProperty("channels").GetProperty("pistol1");
            //JsonElement pistol2 = doc.RootElement.GetProperty("channels").GetProperty("pistol2");
            ForceTubeVRInterface.FTChannelFile myChannels = JsonConvert.DeserializeObject<ForceTubeVRInterface.FTChannelFile>(ForceTubeVRInterface.ListChannels());
            var pistol1 = myChannels.channels.pistol1;
            var pistol2 = myChannels.channels.pistol2;
            if ((pistol1.Count > 0) && (pistol2.Count > 0))
            {
                dualWield = true;
                MelonLogger.Msg("Two ProTube devices detected, player is dual wielding.");
                if ((readChannel("pistol1") == "") || (readChannel("pistol2") == ""))
                {
                    MelonLogger.Msg("No configuration files found, saving current right and left hand pistols.");
                    saveChannel("pistol1", pistol1[0].name);
                    saveChannel("pistol2", pistol2[0].name);
                }
                else
                {
                    string rightHand = readChannel("pistol1");
                    string leftHand = readChannel("pistol2");
                    MelonLogger.Msg("Found and loaded configuration. Right hand: " + rightHand + ", Left hand: " + leftHand);
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
                MelonLogger.Msg("Recoil: " + __instance.CurrentRecoil.ToString());
                byte kickPower = 220;
                ForceTubeVRChannel myChannel = ForceTubeVRChannel.pistol1;
                if (dualWield)
                    if (!(__instance.EquipmentSlot.SlotID == E_EQUIPMENT_SLOT_ID.RIGHT_HAND))
                        myChannel = ForceTubeVRChannel.pistol2;
                ForceTubeVRInterface.Kick(kickPower, myChannel);
            }
        }
    }
}
