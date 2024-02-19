using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Paradox.ParadoxMod), "The Paradox", "0.3.0", "Amitai")]
[assembly: MelonGame("Dodge Roll", "Exit the Gungeon")]
namespace Paradox
{
    public class ParadoxMod : MelonMod
    {
        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.GetNextRoute))]
        static class PatchGetNextRoute
        {
            public static bool Prefix(ref NPCRoute_Selector __instance, ref PlayerController.PlayableCharacters playerIdentity)
            {
                Route currentRoute = ParadoxSaveData.GetLastRouteForCharacter(playerIdentity);
                Route nextRoute = ParadoxNPCRoute_Selector.GetNextRoute(playerIdentity, currentRoute, ref __instance);
                ParadoxSaveData.SetLastRouteForCharacter(playerIdentity, nextRoute);
                return true;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), "OnInitialize")]
        static class PatchOnInitialize
        {
            public static bool Prefix(NPCRoute_Selector __instance)
            {
                tk2dSpriteCollectionData collection = ParadoxResourceManager.CreateRouteSprites();
                ParadoxNPCRoute_Selector.AddSprite(__instance, collection, 0, "Paradox");
                ParadoxNPCRoute_Selector.AddSprite(__instance, collection, 1, "BossRush");
                ParadoxNPCRoute_Selector.FixSprites(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), "FixSprites")]
        static class PatchFixSprites
        {
            public static bool Prefix(ref NPCRoute_Selector __instance)
            {
                ParadoxNPCRoute_Selector.FixSprites(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.SetRouteSprite))]
        static class PatchSetRouteSprite
        {
            public static bool Prefix(ref NPCRoute_Selector __instance, ref PlayerController.PlayableCharacters targetRoute)
            {
                Route nextRoute = ParadoxSaveData.GetLastRouteForCharacter(CharacterManager.Instance.GetPrimaryPlayerController().Identity);
                ParadoxNPCRoute_Selector.SetRouteSprite(__instance, nextRoute);
                return false;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.AnyOtherRouteUnlocked))]
        static class PatchAnyOtherRouteUnlocked
        {
            public static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveData), nameof(SaveData.SetLastRouteForCharacter))]
        static class PatchSetLastRouteForCharacter
        {
            public static bool Prefix(ref PlayerController.PlayableCharacters identity, ref PlayerController.PlayableCharacters route)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "get_RouteData")]
        static class PatchGetRouteData
        {
            public static bool Prefix(ref PlayerController __instance, ref RouteData __result)
            {
                FieldInfo cache = AccessTools.Field(typeof(PlayerController), "m_cachedRouteData");
                if (cache.GetValue(__instance) == null)
                {
                    MelonLogger.Msg("getting new route data");
                    cache.SetValue(__instance, ParadoxRouteData.GetRouteData(ParadoxSaveData.GetLastRouteForCharacter(__instance.Identity)));
                }
                __result = (RouteData)cache.GetValue(__instance);
                return false;
            }

        }

        [HarmonyPatch(typeof(NPCSST_JETSKI), "Start")]
        static class PatchStart
        {
            public static bool Prefix(ref NPCSST_JETSKI __instance)
            {
                if (GameManager.RunData.HasAttemptedGlocktopus)
                {
                    AccessTools.Field(typeof(NPCSST_JETSKI), "m_hasBait").SetValue(__instance, true);
                }
                return true;
            }

            public static void Postfix(ref NPCSST_JETSKI __instance)
            {
                if (!GameManager.RunData.HasAttemptedGlocktopus && ParadoxSaveData.GetCurrentRoute() == Route.BOSSRUSH)
                {
                    __instance.gameObject.SetActive(true);
                    UnityEngine.GameObject wall = __instance.ThingsToTurnOffIfHasBait[0];
                    wall.transform.SetX(-12.3f);
                }
            }
        }

        [HarmonyPatch(typeof(MultifightDoor), "Awake")]
        static class PatchAwake
        {
            public static bool Prefix(ref MultifightDoor __instance)
            {
                if (ParadoxSaveData.GetCurrentRoute() == Route.BOSSRUSH)
                {
                    __instance.Close();
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameManager), "Coroutine_Area")]
        static class PatchCoroutine_Area
        {
            public static bool Prefix()
            {
                BraveTime.ClearTimeScaleModifier("powerup");
                return true;
            }
        }

        static IEnumerable<CodeInstruction> TranspilerCoroutine_Area(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_I4_6 && code[i + 1].opcode == OpCodes.Bge)
                {
                    code[i].opcode = OpCodes.Ldc_I4;
                    code[i].operand = 14;
                }
            }
            return code;
        }

        static float GetParachuteX()
        {
            return ParadoxSaveData.GetCurrentRoute() == Route.BOSSRUSH ? -7.1f : -1f;
        }

        static void ReleaseCamera()
        {
            if (ParadoxSaveData.GetCurrentRoute() == Route.BOSSRUSH)
            {
                GameManager.Instance.CameraOverridePosition = null;
            }
        }

        static IEnumerable<CodeInstruction> TranspilerHandleOutroRoomCR(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            int index = -1;
            List<CodeInstruction> releaseCamera = new List<CodeInstruction>{ };
            for (int i = 1; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == -1f)
                {
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = AccessTools.Method(typeof(ParadoxMod), nameof(ParadoxMod.GetParachuteX));
                }
                else if (code[i].opcode == OpCodes.Callvirt && code[i].operand.ToString().Contains("PrimaryPlayer") && code[i+1].opcode == OpCodes.Stfld)
                {
                    
                    index = i - 2;
                }
            }
            code.Insert(index, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ParadoxMod), nameof(ParadoxMod.ReleaseCamera))));
            return code;
        }

        public override void OnInitializeMelon()
        {
            var areaMethod = AccessTools.Method(typeof(GameManager), "Coroutine_Area");
            var areaMoveNext = AccessTools.EnumeratorMoveNext(areaMethod);
            var areaTranspiler = new HarmonyMethod(AccessTools.Method(typeof(ParadoxMod), nameof(ParadoxMod.TranspilerCoroutine_Area)));
            HarmonyInstance.Patch(areaMoveNext, transpiler: areaTranspiler);

            var outroMethod = AccessTools.Method(typeof(Room), "HandleEnterOutroRoomCR");
            var outroMoveNext = AccessTools.EnumeratorMoveNext(outroMethod);
            var outroTranspiler = new HarmonyMethod(AccessTools.Method(typeof(ParadoxMod), nameof(ParadoxMod.TranspilerHandleOutroRoomCR)));
            HarmonyInstance.Patch(outroMoveNext, transpiler: outroTranspiler);

            if (ParadoxResourceManager.LoadBundle())
            {
                MelonLogger.Msg("bundle loaded");
            }
            else
            {
                MelonLogger.Msg("bundle failed");
            }
        }
    }
}
