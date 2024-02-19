using System.Reflection;
using HarmonyLib;

namespace Paradox
{
    static class ParadoxSaveData
    {
        public static Route GetCurrentRoute()
        {
            return GetLastRouteForCharacter(CharacterManager.Instance.GetPrimaryPlayerController().Identity);
        }

        private static FieldInfo LastRouteField(PlayerController.PlayableCharacters identity)
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
        }

        public static Route GetLastRouteForCharacter(PlayerController.PlayableCharacters identity)
        {
            SaveData save = SaveData.Current;
            if (!save.TestFlag(SaveFlags.ROUTE_SELECTOR_AVAILABLE))
            {
                save.ResetRoutesToDefault();
            }
            FieldInfo field = LastRouteField(identity);
            int value = (int)field.GetValue(save);
            if (value <= 0)
            {
                return (Route)identity;
            }
            return (Route)(value - 1);
        }
    }
}
