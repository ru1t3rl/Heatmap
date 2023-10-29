using System;
using System.Linq;
using UnityEngine;

namespace Ru1t3rl.Heatmap.Models
{
    [Serializable]
    public class HeatmapCollisionInfo
    {
        private const int TextureSizeOverride = 1024;

        public MaterialPropertyBlock materialPropertyBlock;

        public Texture2D originalTexture = null;
        public Texture2D heatmapTexture = null;
        public Texture2D combinedTexture = null;

        public int TextureWidth => originalTexture.width;
        public int TextureHeight => originalTexture.height;

        public HeatmapCollisionInfo(MaterialPropertyBlock materialPropertyBlock, string mainTextureProperty)
        {
            this.materialPropertyBlock = materialPropertyBlock;
            originalTexture = this.materialPropertyBlock.GetTexture(mainTextureProperty) as Texture2D;

            if (originalTexture == null)
            {
                originalTexture = new Texture2D(TextureSizeOverride, TextureSizeOverride)
                {
                    name = "Albedo"
                };
                originalTexture.SetPixels(originalTexture.GetPixels().Select(_ => Color.white).ToArray());
                originalTexture.Apply();
            }

            SetupHeatmap();
            SetupCombinedTexture();

            materialPropertyBlock.SetTexture(mainTextureProperty, combinedTexture);
        }

        private void SetupHeatmap()
        {
            heatmapTexture = new Texture2D(originalTexture.width, originalTexture.height)
            {
                name = "Heatmap"
            };
            heatmapTexture.SetPixels(heatmapTexture.GetPixels().Select(_ => Color.clear).ToArray());
            heatmapTexture.Apply();
        }

        private void SetupCombinedTexture()
        {
            combinedTexture = new Texture2D(originalTexture.width, originalTexture.height);
            combinedTexture.SetPixels(originalTexture.GetPixels().Select(c => c).ToArray());
            combinedTexture.Apply();
        }

        public void UpdateHeatmapTransparency(float transparency, float blendIntensity)
        {
            Color originalColor, heatmapColor;

            for (int i = 0, x, y; i < TextureWidth * TextureHeight; i++)
            {
                x = i % TextureWidth;
                y = Mathf.FloorToInt(i / TextureWidth);

                originalColor = originalTexture.GetPixel(x, y);
                heatmapColor = heatmapTexture.GetPixel(x, y);
                heatmapColor = Color.Lerp(originalColor, heatmapColor, heatmapColor.a * (1 + blendIntensity));

                combinedTexture.SetPixel(
                    x,
                    y,
                    Color.Lerp(
                        originalColor,
                        heatmapColor,
                        transparency
                    )
                );
            }

            combinedTexture.Apply();
        }
    }
}