using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;

namespace LethalExperience
{
    [BepInPlugin("Stoneman.LethalExperience", "Lethal Experience", "1.1.0")]
    public class LethalXP : BaseUnityPlugin
    {
        private const string modGUID = "Stoneman.LethalExperience";
        private const string modName = "Lethal Experience";
        private const string modVersion = "1.1.0";
        private const string modAuthor = "Stoneman";
        internal static ConfigEntry<int> configXP;
        internal static ConfigEntry<int> configLevel;
        internal static ConfigEntry<int> configProfit;
        internal static ConfigEntry<int> configSkillpoints;
        internal static ManualLogSource Log;
        private void Awake()
        {
            var harmony = new Harmony(modGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log = Logger;

            Log.LogInfo("Lethal Experience loaded!");

            configXP = Config.Bind("General",
            "XP",
            0,
            "How much XP you've gained while playing with this mod. Best not to touch this :)");

            configLevel = Config.Bind("General",
            "Level",
            0,
            "What level you are. Don't cheat now.");

            configProfit = Config.Bind("General",
            "Profit",
            0,
            "How much profit did you make the company? Best not to touch this :)");

            configSkillpoints = Config.Bind("General",
            "Skillpoints",
            0,
            "How many skillpoints you have. Don't cheat now.");
        }

        public static void AddXP(int xp)
        {
            int oldXP = configXP.Value;

            // Add to config.
            configXP.Value += xp;
            configProfit.Value += xp;

            int newXP = configXP.Value;

            // If we have enough XP to level up, level up.
            if (configXP.Value >= GetXPRequirement())
            {
                configXP.Value -= GetXPRequirement();
                configLevel.Value++;

                // Level up update!
                LethalExperience.Patches.HUDManagerPatch.ShowLevelUp();

                // Give a skillpoint every 5 levels.
                if (configLevel.Value % 5 == 0)
                {
                    configSkillpoints.Value++;
                }
            }

            // Show XP Bar!
            LethalExperience.Patches.HUDManagerPatch.ShowXPUpdate(oldXP, newXP, xp);
        }

        public static int GetXP()
        {
            return configXP.Value;
        }

        public static int GetLevel()
        {
            return configLevel.Value;
        }

        public static int GetXPRequirement()
        {
            return 100 + GetLevel() * 10;
        }

        public static int GetSkillpoints()
        {
            return configSkillpoints.Value;
        }

        public static void SetSkillpoints(int skillpoints)
        {
            configSkillpoints.Value = skillpoints;
        }
    }
}