﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKIT.Extensions;
using ULTRAKIT.Extensions.Classes;
using ULTRAKIT.Loader.Injectors;
using ULTRAKIT.Loader.Loaders;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ULTRAKIT.Loader.Patches
{
    [HarmonyPatch(typeof(SpawnMenu))]
    public static class SpawnMenuPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void AwakePrefix(SpawnMenu __instance, SpawnableObjectsDatabase ___objects)
        {
            // Creates a constant copy of vanilla database for loader use
            // The database persists across loads/scenes, so doing otherwise would keep adding spawnables to a list that already has them
            if (!SpawnablesLoader.init)
            {
                // DELETE
                SpawnablesInjector.Init();

                Registries.spawn_spawnablesDatabase.enemies = ___objects.enemies;
                Registries.spawn_spawnablesDatabase.objects = ___objects.objects;
                Registries.spawn_spawnablesDatabase.sandboxTools = ___objects.sandboxTools;
                SpawnablesLoader.init = true;
            }

            SpawnablesLoader.InjectSpawnables(__instance);
            ___objects.sandboxTools = Registries.spawn_tools;
            ___objects.enemies = Registries.spawn_enemies;
            ___objects.objects = Registries.spawn_objects;
        }
    }

    [HarmonyPatch(typeof(BossHealthBar))]
    public static class BossHealthBarPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(BossHealthBar __instance, EnemyIdentifier ___eid, GameObject ___bossBar)
        {
            var cust = __instance.gameObject.GetComponent<CustomHealthbarPos>();
            if (cust)
            {
                cust.barObj = ___bossBar;
                cust.enemy = ___eid.gameObject;
                cust.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(MinosBoss))]
    public static class MinosPatch
    {
        // Keeps Minos from flying high in the sky when spawned and positions the health bar
        static float minosHeight = 600, minosOffset = 200;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void StartPrefix(MinosBoss __instance)
        {
            CustomHealthbarPos cust = __instance.GetComponentInChildren<CustomHealthbarPos>(true);
            if (cust)
            {
                cust.offset = Vector3.up * minosHeight * 1.5f;
                cust.size = 0.25f;
                var plr = MonoSingleton<NewMovement>.Instance.transform;
                __instance.transform.position = plr.position + Vector3.down * minosHeight;
                __instance.transform.position += plr.forward * minosOffset;
                __instance.transform.forward = Vector3.ProjectOnPlane((plr.position - __instance.transform.position).normalized, Vector3.up);
            }
        }
    }

    [HarmonyPatch(typeof(LeviathanHead))]
    public static class LeviathanPatch
    {
        // Positions the leviathan health bar when spawned
        static float leviHeight = 50;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void StartPrefix(LeviathanHead __instance)
        {
            var cust = __instance.transform.parent.GetComponentInChildren<CustomHealthbarPos>(true);
            if (cust)
            {
                cust.offset = Vector3.up * leviHeight * 1.5f;
                cust.size = 0.25f;
            }

            SkinnedMeshRenderer renderer = __instance.transform.Find("Leviathan_SplineHook_Basic/ArmR").GetComponent<SkinnedMeshRenderer>();
            renderer.material.color = Color.white;
            renderer.material.mainTexture = renderer.material.mainTexture;
        }
    }

    [HarmonyPatch(typeof(LeviathanTail))]
    public static class LeviathanTailPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(LeviathanTail __instance)
        {
            SkinnedMeshRenderer renderer = __instance.transform.Find("Leviathan_SplineHook_Basic/ArmR").GetComponent<SkinnedMeshRenderer>();
            renderer.material.color = Color.white;
            renderer.material.mainTexture = renderer.material.mainTexture;
        }
    }

    [HarmonyPatch(typeof(ComplexSplasher))]
    public static class LeviathanTeasePatch
    {
        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void AwakePostfix(ComplexSplasher __instance)
        {
            Transform armature = __instance.transform.Find("Leviathan_SplineHook_Basic/ArmR");
            if (armature == null) return;
            SkinnedMeshRenderer renderer = armature.GetComponent<SkinnedMeshRenderer>();
            renderer.material.color = Color.white;
            renderer.material.mainTexture = renderer.material.mainTexture;
        }
    }

    [HarmonyPatch(typeof(ObjectActivator))]
    public static class LeviathanTeasePatch2
    {
        [HarmonyPatch("Start"), HarmonyPostfix]
        public static void StartPostfix(ObjectActivator __instance)
        {
            Transform armature = __instance.transform.Find("Leviathan_HeadFix/Leviathan");
            if (armature == null) return;
            SkinnedMeshRenderer renderer = armature.GetComponent<SkinnedMeshRenderer>();
            renderer.material.color = Color.white;
            renderer.material.mainTexture = renderer.material.mainTexture;
        }
    }

    [HarmonyPatch(typeof(Wicked))]
    public static class WickedPatch
    {
        [HarmonyPatch("GetHit")]
        [HarmonyPrefix]
        public static bool GetHitPrefix(Wicked __instance)
        {
            // Keeps Something Wicked from getting mad and breaking when there is nowhere to teleport
            if (__instance.patrolPoints[0] == null)
            {
                var oldAud = __instance.hitSound.GetComponent<AudioSource>();

                var newAudObj = new GameObject();
                newAudObj.transform.position = __instance.transform.position;

                var newAud = newAudObj.AddComponent<AudioSource>();
                newAud.playOnAwake = false;
                newAud.clip = oldAud.clip;
                newAud.volume = oldAud.volume;
                newAud.pitch = oldAud.pitch;
                newAud.spatialBlend = oldAud.spatialBlend;
                newAud.Play();

                GameObject.Destroy(newAudObj, newAud.clip.length);

                GameObject.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}
