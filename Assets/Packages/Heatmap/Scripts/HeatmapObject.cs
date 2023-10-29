using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ru1t3rl.Heatmap
{
    public class HeatmapObject : MonoBehaviour
    {
        private const int TextureSizeOverride = 1024;

        [SerializeField] private Gradient heatmapGradient;
        [SerializeField, Range(0f, 1f)] private float heatmapTransparency = 1f;
        [SerializeField] private float colorIncrement = .25f;
        [SerializeField, Range(0f, 1f)] private float blendIntensity = 1f;
        [SerializeField] private int drawSize = 5;
        [SerializeField] private string mainTextureKeyword = "_MainTex";

        private MaterialPropertyBlock materialPropertyBlock;
        private new MeshRenderer renderer;

        private Texture2D heatmapTexture;
        private Texture2D originalTexture;
        private Texture2D combinedTexture;

        private float previousTransparency = 1f;

        private void Start()
        {
            renderer = GetComponent<MeshRenderer>() ?? GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                throw new MissingComponentException(
                    $"[{nameof(HeatmapObject)}] No mesh renderer could be found, which is required to create the heatmap!");
            }

            materialPropertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(materialPropertyBlock);

            originalTexture = materialPropertyBlock.GetTexture(mainTextureKeyword) as Texture2D;
            if (originalTexture == null)
            {
                originalTexture = new Texture2D(TextureSizeOverride, TextureSizeOverride);
                originalTexture.SetPixels(originalTexture.GetPixels().Select(_ => Color.white).ToArray());
                originalTexture.Apply();
            }

            heatmapTexture = new Texture2D(originalTexture.width, originalTexture.height);
            heatmapTexture.SetPixels(heatmapTexture.GetPixels().Select(_ => Color.clear).ToArray());
            heatmapTexture.Apply();

            ProcessTextures(originalTexture.width, originalTexture.height);

            materialPropertyBlock.SetTexture(mainTextureKeyword, combinedTexture);
            renderer.SetPropertyBlock(materialPropertyBlock);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (previousTransparency != heatmapTransparency)
            {
                ProcessTextures(combinedTexture.width, combinedTexture.height);
                previousTransparency = heatmapTransparency;
            }
        }
#endif

        private void OnCollisionEnter(Collision collision)
        {
            UpdateTexture(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            UpdateTexture(collision);
        }

        private void UpdateTexture(Collision collision)
        {
            Vector2 uv = Vector2.zero;

            Vector3 averageNormal = Vector3.zero;
            Vector3 averagePoint = Vector3.zero;
            collision.contacts.ToList().ForEach(contact =>
            {
                averagePoint += contact.point;
                averageNormal += contact.normal;
            });
            averagePoint /= collision.contacts.Length;
            averageNormal /= collision.contacts.Length;

            RaycastHit hit;
            Ray ray = new Ray(averagePoint - averageNormal, averageNormal);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider is not MeshCollider) return;

                uv = hit.textureCoord;
            }
            else
            {
                return;
            }

            uv.x *= heatmapTexture.width;
            uv.y *= heatmapTexture.height;

            UpdateCollisionPixels(uv);
        }

        private void UpdateCollisionPixels(Vector2 uv)
        {
            Color currentCenterColor = heatmapTexture.GetPixel((int)uv.x, (int)uv.y);
            Color nextCenterColor = heatmapGradient.Evaluate(currentCenterColor.a + colorIncrement);

            List<Vector2Int> updatedPixels = new();

            for (int i = -drawSize; i <= drawSize - 1; i++)
            {
                for (int j = -drawSize; j <= drawSize - 1; j++)
                {
                    // Check if in circle radius
                    if (i * i + j * j <= drawSize * drawSize + 1)
                    {
                        int pixX = (int)uv.x + i;
                        int pixY = (int)uv.y + j;

                        updatedPixels.Add(new Vector2Int(pixX, pixY));

                        float distanceFromCenter =
                            (pixX - (int)uv.x) * (pixX - (int)uv.x) +
                            (pixY - (int)uv.y) * (pixY - (int)uv.y);

                        float alpha = 1f - distanceFromCenter / (drawSize * drawSize);

                        Color currentPixelColor = heatmapTexture.GetPixel(pixX, pixY);
                        Color nextCurrentPixelColor = heatmapGradient.Evaluate(nextCenterColor.a * alpha);

                        if (nextCurrentPixelColor.a > currentPixelColor.a)
                        {
                            Color interpolated = Color.Lerp(
                                currentPixelColor,
                                nextCurrentPixelColor,
                                1 - blendIntensity + nextCurrentPixelColor.a * blendIntensity
                            );

                            interpolated.a = nextCurrentPixelColor.a;

                            heatmapTexture.SetPixel(pixX, pixY, interpolated);
                        }
                    }
                }
            }

            heatmapTexture.Apply();
            ProcessTextures(updatedPixels.ToArray());
        }

        private void ProcessTextures(Vector2Int[] pixels)
        {
            if (renderer == null)
                throw new MissingComponentException(
                    $"[{nameof(HeatmapObject)}] No mesh renderer could be found, which is required to create the heatmap!");

            combinedTexture ??= new Texture2D(originalTexture.width, originalTexture.height);

            Color originalColor, heatmapColor;

            for (int i = 0; i < pixels.Length; i++)
            {
                originalColor = originalTexture.GetPixel(pixels[i].x, pixels[i].y);
                heatmapColor = heatmapTexture.GetPixel(pixels[i].x, pixels[i].y);
                heatmapColor = Color.Lerp(originalColor, heatmapColor, heatmapColor.a);

                combinedTexture.SetPixel(
                    pixels[i].x,
                    pixels[i].y,
                    Color.Lerp(
                        originalColor,
                        heatmapColor,
                        heatmapTransparency
                    )
                );
            }

            combinedTexture.Apply();
        }

        private void ProcessTextures(int width, int height)
        {
            if (renderer == null)
                throw new MissingComponentException(
                    $"[{nameof(HeatmapObject)}] No mesh renderer could be found, which is required to create the heatmap!");

            combinedTexture ??= new Texture2D(originalTexture.width, originalTexture.height);

            Color originalColor, heatmapColor;

            for (int i = 0, x, y; i < width * height; i++)
            {
                x = i % width;
                y = Mathf.FloorToInt(i / width);

                originalColor = originalTexture.GetPixel(x, y);
                heatmapColor = heatmapTexture.GetPixel(x, y);
                heatmapColor = Color.Lerp(originalColor, heatmapColor, heatmapColor.a * (1 + blendIntensity));

                combinedTexture.SetPixel(
                    x,
                    y,
                    Color.Lerp(
                        originalColor,
                        heatmapColor,
                        heatmapTransparency
                    )
                );
            }

            combinedTexture.Apply();
        }
    }
}