using UnityEngine;

namespace _Bludoku.Scripts.Effects
{
    // The selected CC0 source textures are kept in Resources so scene feedback can be assembled
    // without adding a large imported asset package or changing existing prefabs.
    public static class ComboVfxAssets
    {
        private const string Path = "ComboVfx/";

        public static Shader AdditiveShader => Resources.Load<Shader>(Path + "ComboAdditive");
        public static Texture2D Star => Resources.Load<Texture2D>(Path + "Star");
        public static Texture2D Flame => Resources.Load<Texture2D>(Path + "Flame");

        public static Texture2D[] LoadLightningFrames()
        {
            var frames = new Texture2D[4];
            frames[0] = Resources.Load<Texture2D>(Path + "Lightning");
            for (int i = 1; i < frames.Length; i++)
                frames[i] = Resources.Load<Texture2D>(Path + "Lightning" + i);
            return frames;
        }

        public static Texture2D[] LoadAnimation(string name)
        {
            var sheets = new Texture2D[4];
            for (int i = 0; i < sheets.Length; i++)
                sheets[i] = Resources.Load<Texture2D>(Path + name + i);
            return sheets;
        }

        public static Rect FrameUv(int frame)
        {
            const float inset = 0.5f / 512f; // Avoid sampling the neighbouring atlas frame.
            return new Rect((frame % 4) * 0.25f + inset,
                (3 - frame / 4 % 4) * 0.25f + inset,
                0.25f - inset * 2f, 0.25f - inset * 2f);
        }
    }
}
