using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Paradox
{
    static class ParadoxNPCRoute_Selector
    {
        public static tk2dSprite GetSprite(NPCRoute_Selector inst, string name)
        {
            return inst.transform.Find("ElevatorIcon").Find(name).GetComponent<tk2dSprite>();
        }

        public static void AddSprite(NPCRoute_Selector inst, tk2dSpriteCollectionData collection, int id, string name)
        {
            GameObject spriteObject = Object.Instantiate<GameObject>(inst.HunterSprite.gameObject);
            tk2dSprite RouteSprite = spriteObject.GetComponent<tk2dSprite>();
            RouteSprite.SetSprite(collection, id);
            RouteSprite.Renderer.material = inst.HunterSprite.Renderer.sharedMaterial;
            spriteObject.transform.position = inst.HunterSprite.gameObject.transform.position;
            spriteObject.transform.parent = inst.transform.Find("ElevatorIcon");
            spriteObject.name = name;
        }

        public static void FixSprites(NPCRoute_Selector inst)
        {
            tk2dSprite ParadoxSprite = ParadoxNPCRoute_Selector.GetSprite(inst, "Paradox");
            tk2dSprite BossRushSprite = ParadoxNPCRoute_Selector.GetSprite(inst, "BossRush");
            ParadoxSprite.usesOverrideMaterial = true;
            ParadoxSprite.UsesShaderUVRect = true;
            BossRushSprite.usesOverrideMaterial = true;
            BossRushSprite.UsesShaderUVRect = true;
            ParadoxSprite.ForceBuild();
            BossRushSprite.ForceBuild();
        }

        public static Route GetNextRoute(PlayerController.PlayableCharacters playerIdentity, Route currentRoute, ref NPCRoute_Selector inst)
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
                    route = Route.BOSSRUSH;
                    break;
                case Route.BOSSRUSH:
                    route = Route.PILOT;
                    break;
            }
            PlayerController.PlayableCharacters id = (PlayerController.PlayableCharacters)route;
            if (inst.RouteIsUnlocked(id) || playerIdentity == id || route == Route.PARADOX || route == Route.BOSSRUSH)
            {
                return route;
            }
            return GetNextRoute(playerIdentity, route, ref inst);
        }

        public static void SetRouteSprite(NPCRoute_Selector inst, Route targetRoute)
        {
            tk2dSprite ParadoxSprite = GetSprite(inst, "Paradox");
            tk2dSprite BossRushSprite = GetSprite(inst, "BossRush");
            tk2dSprite previousSprite = null;
            tk2dSprite nextSprite = null;
            if (inst.PilotSprite.gameObject.activeSelf)
            {
                previousSprite = inst.PilotSprite;
            }
            if (inst.ConvictSprite.gameObject.activeSelf)
            {
                previousSprite = inst.ConvictSprite;
            }
            if (inst.MarineSprite.gameObject.activeSelf)
            {
                previousSprite = inst.MarineSprite;
            }
            if (inst.HunterSprite.gameObject.activeSelf)
            {
                previousSprite = inst.HunterSprite;
            }
            if (inst.RobotSprite.gameObject.activeSelf)
            {
                previousSprite = inst.RobotSprite;
            }
            if (inst.BulletSprite.gameObject.activeSelf)
            {
                previousSprite = inst.BulletSprite;
            }
            if (inst.CultistSprite.gameObject.activeSelf)
            {
                previousSprite = inst.CultistSprite;
            }
            if (ParadoxSprite.gameObject.activeSelf)
            {
                previousSprite = ParadoxSprite;
            }
            if (BossRushSprite.gameObject.activeSelf)
            {
                previousSprite = BossRushSprite;
            }
            switch (targetRoute)
            {
                case Route.PILOT:
                    nextSprite = inst.PilotSprite;
                    inst.PilotSprite.gameObject.SetActive(true);
                    break;
                case Route.CONVICT:
                    nextSprite = inst.ConvictSprite;
                    inst.ConvictSprite.gameObject.SetActive(true);
                    break;
                case Route.MARINE:
                    nextSprite = inst.MarineSprite;
                    inst.MarineSprite.gameObject.SetActive(true);
                    break;
                case Route.HUNTER:
                    nextSprite = inst.HunterSprite;
                    inst.HunterSprite.gameObject.SetActive(true);
                    break;
                case Route.ROBOT:
                    nextSprite = inst.RobotSprite;
                    inst.RobotSprite.gameObject.SetActive(true);
                    break;
                case Route.BULLET:
                    nextSprite = inst.BulletSprite;
                    inst.BulletSprite.gameObject.SetActive(true);
                    break;
                case Route.CULTIST:
                    nextSprite = inst.CultistSprite;
                    inst.CultistSprite.gameObject.SetActive(true);
                    break;
                case Route.PARADOX:
                    nextSprite = ParadoxSprite;
                    ParadoxSprite.gameObject.SetActive(true);
                    break;
                case Route.BOSSRUSH:
                    nextSprite = BossRushSprite;
                    BossRushSprite.gameObject.SetActive(true);
                    break;
            }
            MethodInfo HandleRouteSwitch = AccessTools.Method(typeof(NPCRoute_Selector), "HandleRouteSwitch");
            inst.StartCoroutine((IEnumerator)HandleRouteSwitch.Invoke(inst, new object[] { previousSprite, nextSprite }));
        }
    }
}
