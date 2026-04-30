
using Code.Games.g_mikspikes.Core;
using Code.Games.g_mikspikes.Data;
using Code.Games.g_mikspikes.Upgrades;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Mikmod
{
    internal static class SpikesHack
    {
        public static bool invincibility_mode = false;
        public static bool freeze_player = false;
        public static bool infinite_spawns = false;
        public static bool spawn_infront_of_the_player = false;
        public static int spawn_iterations = 50;

        public static int ratio_points = 1;
        public static int ratio_shield = 1;
        public static int ratio_life = 1;
        public static int ratio_laser = 1;
    }

    [HarmonyPatch(typeof(Collectibles_Spawner), "DecideIfToSpawnCollectible")]
    public static class Spawner_Patch
    {
        static bool Prefix(Collectibles_Spawner __instance)
        {
            if (!SpikesHack.infinite_spawns)
                return true;

            int iterations = SpikesHack.spawn_iterations;

            for (int i = 0; i < iterations; i++)
            {
                SpawnOnce(__instance);
            }

            return false;
        }

        static void SpawnOnce(Collectibles_Spawner __instance)
        {
            for (int i = 0; i < SpikesHack.ratio_points; i++)
                AccessTools.Method(typeof(Collectibles_Spawner), "SpawnCollectibleItemAt")
                    .Invoke(__instance, new object[] { MikSpikes_Upgrades_Const_Info.ECollectibleType.Points });

            for (int i = 0; i < SpikesHack.ratio_shield; i++)
                AccessTools.Method(typeof(Collectibles_Spawner), "SpawnCollectibleItemAt")
                    .Invoke(__instance, new object[] { MikSpikes_Upgrades_Const_Info.ECollectibleType.Shield });

            for (int i = 0; i < SpikesHack.ratio_life; i++)
                AccessTools.Method(typeof(Collectibles_Spawner), "SpawnCollectibleItemAt")
                    .Invoke(__instance, new object[] { MikSpikes_Upgrades_Const_Info.ECollectibleType.Life });

            for (int i = 0; i < SpikesHack.ratio_laser; i++)
                AccessTools.Method(typeof(Collectibles_Spawner), "SpawnCollectibleItemAt")
                    .Invoke(__instance, new object[] { MikSpikes_Upgrades_Const_Info.ECollectibleType.Laser });
        }
    }

    [HarmonyPatch(typeof(Mikspikes_Controller), "Update")]
    public static class Mikspikes_Update_Patch
    {
        private static readonly FieldInfo canGetHitField =
            AccessTools.Field(typeof(Mikspikes_Controller), "_canGetHit");

        private static readonly MethodInfo freezeMethod =
            AccessTools.Method(typeof(Mikspikes_Controller), "FreezePlayer");

        private static readonly MethodInfo resumeMethod =
            AccessTools.Method(typeof(Mikspikes_Controller), "ResumePlayer");

        private static bool lastFreezeState;

        static void Postfix(Mikspikes_Controller __instance)
        {
            // --- INVINCIBILITY ---
            canGetHitField.SetValue(__instance, !SpikesHack.invincibility_mode);

            // --- FREEZE LOGIC (trigger only on change) ---
            if (SpikesHack.freeze_player != lastFreezeState)
            {
                lastFreezeState = SpikesHack.freeze_player;

                if (SpikesHack.freeze_player)
                {
                    freezeMethod?.Invoke(__instance, null);
                }
                else
                {
                    resumeMethod?.Invoke(__instance, null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Collectibles_Spawner), "SpawnCollectibleItemAt")]
    public static class SpawnCollectible_Patch
    {
        static bool Prefix(
            Collectibles_Spawner __instance,
            MikSpikes_Upgrades_Const_Info.ECollectibleType collectibleType)
        {
            if (!SpikesHack.spawn_infront_of_the_player)
                return true;

            // --- GET PLAYER ---
            var player = UnityEngine.Object.FindObjectOfType<Mikspikes_Controller>();
            if (player == null) return true;

            // --- GET PLAYER POSITION ---
            Vector3 p = player.transform.position;

            // --- GET DIRECTION FIELD ---
            var dirField = AccessTools.Field(typeof(Mikspikes_Controller), "_direction");
            int dir = (int)dirField.GetValue(player);

            // --- CONFIG ---
            float forwardOffset = 3.0f;   // how far in front of player
            float fixedY = p.y;           // lock Y (or set constant lane)

            // --- FORCE SPAWN POSITION ---
            Vector3 pos = new Vector3(
                p.x + (forwardOffset * dir),
                fixedY,
                0f
            );

            // --- GET PREFAB ---
            var prefabField = AccessTools.Field(typeof(Collectibles_Spawner), "_collectibleItem");
            Collectible_Item prefab = (Collectible_Item)prefabField.GetValue(__instance);

            if (prefab == null) return true;

            // --- SPAWN ---
            Collectible_Item item =
                UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);

            item.SetType(collectibleType);
            return false;
        }
    }
}