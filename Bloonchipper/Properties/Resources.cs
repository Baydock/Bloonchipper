using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Bloonchipper.Properties {
    internal static class Resources {

        private static readonly Assembly thisAssembly = Assembly.GetExecutingAssembly();
        private static readonly string assemblyName = thisAssembly.GetName().Name.Replace(" ", "");
        private static readonly string[] resourceNames = thisAssembly.GetManifestResourceNames();

        private static byte[] GetResource(string resourceName) {
            string fullName = $"{assemblyName}.Resources.{resourceName}";

            if (!resourceNames.Contains(fullName))
                return null;

            using MemoryStream resourceStream = new();
            try {
                thisAssembly.GetManifestResourceStream(fullName).CopyTo(resourceStream);
                return resourceStream.ToArray();
            } catch {
                return null;
            }
        }

        public static Texture2D LoadTexture(string resourceName) {
            byte[] data = GetResource(resourceName + ".png");
            if (data is null)
                return null;
            Texture2D tex = new(0, 0);
            ImageConversion.LoadImage(tex, data);
            return tex;
        }

        public static Sprite LoadSprite(string resourceName) {
            Texture2D tex = LoadTexture(resourceName);
            if (tex is null)
                return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        public static AssetBundle LoadAssetBundle(string resourceName) {
            byte[] data = GetResource($"AssetBundles.{resourceName}");
            if (data is null)
                return null;
            var a = new AssetBundle();
            return AssetBundle.LoadFromMemory(data);
        }
    }
}
