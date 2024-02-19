using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Paradox
{
    class ParadoxResourceManager
    {
        private static AssetBundle bundle;

        public static tk2dSpriteCollectionData CreateRouteSprites()
        {
            Texture texture = bundle.LoadAsset<Texture>("assets/routesprites.png");
            int w = 83;
            int h = 52;
            Vector2 anchor = new Vector2(40, 50);
            tk2dSpriteCollectionData result = tk2dSpriteCollectionData.CreateFromTexture(
                texture,
                tk2dSpriteCollectionSize.Default(),
                new string[] { "Paradox", "BossRush" },
                new Rect[] { new Rect(0, 0, w, h), new Rect(0, h, w, h*2) },
                new Vector2[] { anchor, anchor }
            );
            result.spriteCollectionName = "RouteSprites";
            result.name = "RouteSprites";
            return result;
        }

        public static bool LoadBundle()
        {
            byte[] bytes = ExtractResource("Resources.paradoxbundle");
            bundle = AssetBundle.LoadFromMemory(bytes);
            return (bundle != null);
        }

        public static byte[] ExtractResource(String filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(a.GetName().Name + "." + filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
    }
}
