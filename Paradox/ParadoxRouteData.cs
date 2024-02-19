using System;
using System.Collections.Generic;
using System.Linq;

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
        PARADOX,
        BOSSRUSH
    }

    class ParadoxRouteData
    {
        public static RouteData GetRouteData(Route id)
        {
            switch (id)
            {             
                case Route.PARADOX:
                    return GetRouteDataRandom();
                case Route.BOSSRUSH:
                    return GetRouteDataBossRush();                    
                default:
                    return RouteData.GetRouteData((PlayerController.PlayableCharacters)id);
            }
        }

        public static RouteData GetRouteDataRandom()
        {
            RouteData routeData = UnityEngine.Object.Instantiate<RouteData>(RouteData.GetRouteData(PlayerController.PlayableCharacters.HUNTER));
            Array ids = Enum.GetValues(typeof(PlayerController.PlayableCharacters));
            List<RouteData.StageItem> horizontals = new List<RouteData.StageItem>();
            List<RouteData.StageItem> verticals = new List<RouteData.StageItem>();
            foreach (PlayerController.PlayableCharacters id in ids)
            {
                for (int i = 0; i < 4; i++)
                {
                    RouteData.StageItem stageItem = UnityEngine.Object.Instantiate<RouteData>(RouteData.GetRouteData(id)).Stages[i];
                    if (stageItem.Area.IsHorizontal && id != PlayerController.PlayableCharacters.ROBOT)
                    {
                        horizontals.Add(stageItem);
                    }
                    else
                    {
                        verticals.Add(stageItem);
                    }
                }
            }
            verticals = verticals.Shuffle();
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
            RouteData.StageItem stage = new RouteData.StageItem
            {
                Level = UnityEngine.Object.Instantiate<LevelData>(elv.Level)
            };
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

        public static RouteData GetRouteDataBossRush()
        {
            RouteData result = UnityEngine.Object.Instantiate<RouteData>(RouteData.GetRouteData(PlayerController.PlayableCharacters.HUNTER));
            Dictionary<Route, RouteData> routes = new Dictionary<Route, RouteData>();
            Route[] all = new Route[] { Route.PILOT, Route.CONVICT, Route.MARINE, Route.HUNTER, Route.ROBOT, Route.BULLET, Route.CULTIST };
            foreach (Route route in all)
            {
                routes.Add(route, UnityEngine.Object.Instantiate(RouteData.GetRouteData((PlayerController.PlayableCharacters)route)));
            }
            BossData[] bosses = new BossData[] { };
            foreach (BossData[] data in GetBossData(UnityEngine.Random.Range(0, 2) == 1))
            {
                bosses = bosses.Concat(ShuffleArray(data)).ToArray();
            }
            result.Stages = new List<RouteData.StageItem>();
            foreach (BossData boss in bosses)
            {
                RouteData choice = routes[boss.AllowedRoutes.RandomPick<Route>()];
                result.Stages.Add(MakeBoss(choice.Stages[boss.StageNum], boss.BossNum));
            }
            result.Stages.Add(MakeBoss(routes[Route.HUNTER].Stages[4], 0));
            result.Stages.Add(routes[Route.HUNTER].Stages[5]);
            return result;
        }

        public static RouteData.StageItem MakeBoss(RouteData.StageItem stage, int boss)
        {
            RouteData.StageItem result = new RouteData.StageItem()
            {
                Area = stage.Area,
                BalanceData = stage.BalanceData,
                Level = UnityEngine.Object.Instantiate<LevelData>(stage.Level)
            };
            result.Level.Phases = new AreaPhase[]
            {
                Array.Find<AreaPhase>(result.Level.Phases, p => p.PhaseType == AreaPhaseType.Boss || p.PhaseType == AreaPhaseType.TransitionToDragunElevator)
            };
            if (!result.Area.Name.Contains("Mine"))
            {
                result.Level.Phases[0].BossPool = new CharacterReference[]
                {
                    result.Area.AvailableBossPool[boss]
                };
            }
            return result;
        }

        public struct BossData
        {
            public int StageNum;
            public int BossNum;
            public Route[] AllowedRoutes;

            public BossData(int stageNum, int bossNum, Route[] allowedRoutes)
            {
                StageNum = stageNum;
                BossNum = bossNum;
                AllowedRoutes = allowedRoutes;
            }
        }

        public static BossData[][] GetBossData(bool s)
        {   // put this in a resource
            Route[] all = new Route[] { Route.PILOT, Route.CONVICT, Route.MARINE, Route.HUNTER, Route.ROBOT, Route.BULLET, Route.CULTIST };
            return new BossData[][]
            {
                new BossData[]
                {
                    new BossData(0, 0,     new Route[] { Route.PILOT, Route.CONVICT, Route.MARINE, Route.HUNTER, Route.CULTIST }), // Chancellor
                    new BossData(0, 1,     new Route[] { Route.PILOT, Route.CONVICT, Route.MARINE, Route.HUNTER, Route.CULTIST }), // Buffammo
                    new BossData(0, s?2:3, all),                                                                                   // Fuselier or Meowitzer

                },
                new BossData[]
                {
                    new BossData(1, s?2:4, new Route[] { Route.CONVICT, Route.MARINE, Route.HUNTER, Route.ROBOT, Route.BULLET, Route.CULTIST }), // Meowitzer or Fuselier
                },
                new BossData[]
                {
                    new BossData(1, 1,     new Route[] { Route.CONVICT, Route.MARINE, Route.HUNTER, Route.ROBOT, Route.BULLET, Route.CULTIST }), // Cannonbalrog
                    new BossData(1, 0,     new Route[] { Route.CONVICT, Route.BULLET }),                                                         // Gungamesh
                },
                new BossData[]
                {
                    new BossData(2, 0, all), // Priest
                    new BossData(2, 1, all), // Python
                    new BossData(2, 2, all), // Head

                },
                new BossData[]
                {
                    new BossData(3, 0, new Route[] { Route.PILOT, Route.CONVICT, Route.MARINE, Route.ROBOT }), // Killinder
                    new BossData(3, 1, new Route[] { Route.CONVICT, Route.MARINE }),                           // Geist
                    new BossData(3, 2, new Route[] { Route.PILOT })                                            // Wall
                }
            };
        }

        public static RouteData GetRouteDataEnd()
        {
            RouteData result = UnityEngine.Object.Instantiate<RouteData>(RouteData.GetRouteData(PlayerController.PlayableCharacters.HUNTER));
            result.Stages = new List<RouteData.StageItem>
            {
                result.Stages[result.Stages.Count - 1]
            };
            result.Stages[0].Level.Phases = new AreaPhase[]
            {
                result.Stages[0].Level.Phases[result.Stages[0].Level.Phases.Length - 1]
            };
            return result;
        }

        static T[] ShuffleArray<T>(T[] array)
        {
            Random random = new Random();
            return array.OrderBy(x => random.Next()).ToArray();
        }
    }
}