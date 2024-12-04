using System.IO;
using UnityEngine;

namespace GrowthNectarDisplay
{
    static class Assets
    {
        public static Sprite GrowthNectarBuffIndicatorSprite { get; private set; }

        public static void Load()
        {
            string assetsPath = Path.Combine(Path.GetDirectoryName(GrowthNectarDisplayPlugin.Instance.Info.Location), "assets");

            GrowthNectarBuffIndicatorSprite = loadSprite(Path.Combine(assetsPath, "Indicator.png"), "GrowthNectarBuffIndicator");
        }

        static Sprite loadSprite(string filePath, string name)
        {
            Texture2D texture = loadTexture(filePath, name);
            if (!texture)
                return null;

            Sprite sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), new Vector2(0.5f, 0.5f));
            sprite.name = name;

            return sprite;
        }

        static Texture2D loadTexture(string filePath, string name)
        {
            if (!File.Exists(filePath))
            {
                Log.Error($"Failed to load texture '{name}': File {filePath} does not exist");
                return null;
            }

            byte[] fileBytes = File.ReadAllBytes(filePath);

            Texture2D texture = new Texture2D(1, 1);
            if (!texture.LoadImage(fileBytes))
                return null;

            texture.name = name;

            return texture;
        }
    }
}
