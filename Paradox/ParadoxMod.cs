using MelonLoader;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Paradox
{
    public class ParadoxMod : MelonMod
    {
        [HarmonyPatch(typeof(RouteData), nameof(RouteData.GetRouteData))]
        static class Patch
        {
            public static bool Prefix(ref PlayerController.PlayableCharacters id, ref RouteData __result)
            {
				if (id == PlayerController.PlayableCharacters.CULTIST)
				{
					__result = GetRouteDataRandom();
					return false;
				}
				else
                {
					return true;
                }
            }

			public static RouteData GetRouteDataStatic(PlayerController.PlayableCharacters id)
			{
				RouteData routeData = null;
				switch (id)
				{
					case PlayerController.PlayableCharacters.PILOT:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Pilot");
						break;
					case PlayerController.PlayableCharacters.CONVICT:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Convict");
						break;
					case PlayerController.PlayableCharacters.MARINE:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Marine");
						break;
					case PlayerController.PlayableCharacters.HUNTER:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Hunter");
						break;
					case PlayerController.PlayableCharacters.ROBOT:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Robot");
						break;
					case PlayerController.PlayableCharacters.BULLET:
						routeData = ResourceManager.LoadAsset<RouteData>("Meta/RouteData_Bullet");
						break;
					case PlayerController.PlayableCharacters.CULTIST:
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
				Array ids = Enum.GetValues(typeof(PlayerController.PlayableCharacters));
				List<RouteData.StageItem> horizontals = new List<RouteData.StageItem>();
				List<RouteData.StageItem> verticals = new List<RouteData.StageItem>();
				foreach (PlayerController.PlayableCharacters id in ids)
				{
					for (int i = 0; i < 4; i++)
					{
						RouteData.StageItem stageItem = UnityEngine.Object.Instantiate<RouteData>(GetRouteDataStatic(id)).Stages[i];
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
		}

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("hi");
        }
    }
}
