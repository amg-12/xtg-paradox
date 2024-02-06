using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Paradox
{
    public enum Route
    {
        PILOT,
        CONVICT,
        MARINE,
        HUNTER,
        ROBOT,
        BULLET,
        CULTIST,
        PARADOX
    }

    public class ParadoxMod : MelonMod
    {
        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.GetNextRoute))]
        static class PatchGNR
        {
            public static bool Prefix(ref PlayerController.PlayableCharacters playerIdentity, ref NPCRoute_Selector __instance)
            {
                Route currentRoute = GetLastRouteForCharacter(playerIdentity);
                Route nextRoute = GetNextRoute(playerIdentity, currentRoute, ref __instance);
                SetLastRouteForCharacter(playerIdentity, nextRoute);
                return true;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.SetRouteSprite))]
        static class PatchSRS
        {
            public static bool Prefix(ref NPCRoute_Selector __instance, ref PlayerController.PlayableCharacters targetRoute)
            {
                Route nextRoute = GetLastRouteForCharacter(CharacterManager.Instance.GetPrimaryPlayerController().Identity);
                targetRoute = (PlayerController.PlayableCharacters)nextRoute;
                return true;
            }
        }

        [HarmonyPatch(typeof(NPCRoute_Selector), nameof(NPCRoute_Selector.AnyOtherRouteUnlocked))]
        static class PatchAORU
        {
            public static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveData), nameof(SaveData.SetLastRouteForCharacter))]
        static class PatchSLRFC
        {
            public static bool Prefix(ref PlayerController.PlayableCharacters identity, ref PlayerController.PlayableCharacters route)
            {
                MelonLogger.Msg("blocked writing " + route + " for " + identity);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "get_RouteData")]
        static class PatchGDR
        {
            public static bool Prefix(ref PlayerController __instance, ref RouteData __result)
            {
                FieldInfo cache = AccessTools.Field(typeof(PlayerController), "m_cachedRouteData");
                if (cache.GetValue(__instance) == null)
                {
                    MelonLogger.Msg("getting new route data");
                    cache.SetValue(__instance, GetRouteData(GetLastRouteForCharacter(__instance.Identity)));
                }
                __result = (RouteData)cache.GetValue(__instance);
                return false;
            }

        }

        public static Route GetNextRoute(PlayerController.PlayableCharacters playerIdentity, Route currentRoute, ref NPCRoute_Selector __instance)
        {
            Route route = (Route)playerIdentity;
            switch (currentRoute)
            {
                case Route.PILOT:
                    route = Route.CONVICT;
                    break;
                case Route.CONVICT:
                    route = Route.MARINE;
                    break;
                case Route.MARINE:
                    route = Route.HUNTER;
                    break;
                case Route.HUNTER:
                    route = Route.ROBOT;
                    break;
                case Route.ROBOT:
                    route = Route.BULLET;
                    break;
                case Route.BULLET:
                    route = Route.CULTIST;
                    break;
                case Route.CULTIST:
                    route = Route.PARADOX;
                    break;
                case Route.PARADOX:
                    route = Route.PILOT;
                    break;
            }
            PlayerController.PlayableCharacters id = (PlayerController.PlayableCharacters)route;
            if (__instance.RouteIsUnlocked(id) || playerIdentity == id || route == Route.PARADOX)
            {
                return route;
            }
            return GetNextRoute(playerIdentity, route, ref __instance);
        }

        public static FieldInfo LastRouteField(PlayerController.PlayableCharacters identity)
        {
            string name = null;
            switch (identity)
            {
                case PlayerController.PlayableCharacters.PILOT:
                    name = "m_lastRoutePilot";
                    break;
                case PlayerController.PlayableCharacters.CONVICT:
                    name = "m_lastRouteConvict";
                    break;
                case PlayerController.PlayableCharacters.MARINE:
                    name = "m_lastRouteSoldier";
                    break;
                case PlayerController.PlayableCharacters.HUNTER:
                    name = "m_lastRouteHunter";
                    break;
                case PlayerController.PlayableCharacters.ROBOT:
                    name = "m_lastRouteRobot";
                    break;
                case PlayerController.PlayableCharacters.BULLET:
                    name = "m_lastRouteBullet";
                    break;
                case PlayerController.PlayableCharacters.CULTIST:
                    name = "m_lastRouteCultist";
                    break;
                default:
                    break;
            }
            return AccessTools.Field(typeof(SaveData), name);
        }

        public static void SetLastRouteForCharacter(PlayerController.PlayableCharacters identity, Route route)
        {
            FieldInfo field = LastRouteField(identity);
            field.SetValue(SaveData.Current, (int)(route + 1));
            MelonLogger.Msg("wrote " + route.ToString() + " for " + identity.ToString());
        }

        public static Route GetLastRouteForCharacter(PlayerController.PlayableCharacters identity)
        {
            SaveData save = SaveData.Current;
            if (!save.TestFlag(SaveFlags.ROUTE_SELECTOR_AVAILABLE))
            {
                save.ResetRoutesToDefault();
                MelonLogger.Msg("reset routes to default");
            }
            FieldInfo field = LastRouteField(identity);
            int value = (int)field.GetValue(save);
            if (value <= 0)
            {
                MelonLogger.Msg("got for " + value + " for " + identity.ToString() + "; restoring default");
                return (Route)identity;
            }
            return (Route)(value - 1);
        }

        public static RouteData GetRouteData(Route id)
        {
            if (id == Route.PARADOX)
            {
                return GetRouteDataRandom();
            }
            else
            {
                return GetRouteDataStatic(id);
            }
        }

        public static RouteData GetRouteDataStatic(Route id)
        {
            RouteData routeData = null;
            switch (id)
            {
                case Route.PILOT:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Pilot");
                    break;
                case Route.CONVICT:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Convict");
                    break;
                case Route.MARINE:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Marine");
                    break;
                case Route.HUNTER:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Hunter");
                    break;
                case Route.ROBOT:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Robot");
                    break;
                case Route.BULLET:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Bullet");
                    break;
                case Route.CULTIST:
                    routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Cultist");
                    break;
            }
            if (routeData == null)
            {
                routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Default");
            }
            return routeData;
        }

        public static RouteData GetRouteDataRandom()
        {
            RouteData routeData = UnityEngine.Object.Instantiate<RouteData>(ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Hunter"));
            Array ids = Enum.GetValues(typeof(Route));
            List<RouteData.StageItem> horizontals = new List<RouteData.StageItem>();
            List<RouteData.StageItem> verticals = new List<RouteData.StageItem>();
            foreach (Route id in ids)
            {
                for (int i = 0; i < 4; i++)
                {
                    RouteData.StageItem stageItem = UnityEngine.Object.Instantiate<RouteData>(GetRouteDataStatic(id)).Stages[i];
                    if (stageItem.Area.IsHorizontal && id != Route.ROBOT)
                    {
                        horizontals.Add(stageItem);
                    }
                    else
                    {
                        verticals.Add(stageItem);
                    }
                }
            }
            verticals.RandomizeOrder<RouteData.StageItem>();
            for (int j = 0; j < 4; j++)
            {
                if (j == 2 && UnityEngine.Random.Range(0, 7) < 4)
                {
                    routeData.Stages[j] = horizontals.RandomPick<RouteData.StageItem>();
                }
                else
                {
                    routeData.Stages[j] = Combine(routeData.Stages[j], verticals[j]);
                }
            }
            routeData.Stages[0] = routeData.Stages[0];
            return routeData;
        }

        public static RouteData.StageItem Combine(RouteData.StageItem bkg, RouteData.StageItem elv)
        {
            RouteData.StageItem stage = new RouteData.StageItem();
            stage.Level = UnityEngine.Object.Instantiate<LevelData>(elv.Level);
            for (int i = 0; i < stage.Level.Phases.Length; i++)
            {
                stage.Level.Phases[i].BossPool = bkg.Area.AvailableBossPool.ToArray();
            }
            string area = bkg.Area.Name;
            if (area == "Black Powder Mine" && (elv.Area.IsHorizontal || elv.Level.name.Contains("Flying_Robot")))
            {
                stage.Area = ResourceManager.LoadAsset<AreaData>("Meta/AreaData_03_Mines_Horizontal");
            }
            else
            {
                stage.Area = bkg.Area;
            }
            if (elv.Level.name.Contains("Flying"))
            {
                string str = (area == "Forge" || area == "Hollow") ? "01_Forge_Flying" : "04_Gungeon_Flying";
                stage.BalanceData = ResourceManager.LoadAsset<BalanceData>("Meta/BalanceData_" + str);
            }
            else
            {
                stage.BalanceData = bkg.BalanceData;
            }
            return stage;
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("hi");
        }
    }
}
