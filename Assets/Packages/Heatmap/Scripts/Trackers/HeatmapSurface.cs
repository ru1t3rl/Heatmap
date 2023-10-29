using System.Collections.Generic;
using System.Linq;
using Ru1t3rl.Heatmap.Models;
using UnityEngine;

namespace Ru1t3rl.Heatmap.Trackers
{
    public class HeatmapSurface : MonoBehaviour
    {
        [SerializeField] private Gradient heatmapGradient;
        [SerializeField, Range(0f, 1f)] private float heatmapTransparency = 1f;
        [SerializeField, Range(0f, 1f)] private float blendIntensity = 1f;
        [SerializeField] private float colorIncrement = .1f;
        [SerializeField] private int drawSize = 5;
        [SerializeField] private string mainTextureProperty = "_MainTex";

        private new MeshRenderer renderer;

        private HeatmapCollisionInfo collisionInfo;

        private float previousTransparency = 1f;

        private void Start()
        {
            renderer = GetComponent<MeshRenderer>() ?? GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                throw new MissingComponentException(
                    $"[{nameof(HeatmapSurface)}] No mesh renderer could be found, which is required to create the heatmap!");
            }

            collisionInfo = new HeatmapCollisionInfo(new MaterialPropertyBlock(), mainTextureProperty);
            renderer.SetPropertyBlock(collisionInfo.materialPropertyBlock);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (previousTransparency != heatmapTransparency)
            {
                collisionInfo.UpdateHeatmapTransparency(heatmapTransparency, blendIntensity);
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

            uv.x *= collisionInfo.TextureWidth;
            uv.y *= collisionInfo.TextureHeight;

            UpdateCollisionPixels(uv);
        }

        private void UpdateCollisionPixels(Vector2 uv)
        {
            Color currentCenterColor = collisionInfo.heatmapTexture.GetPixel((int)uv.x, (int)uv.y);
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

                        Color currentPixelColor = collisionInfo.heatmapTexture.GetPixel(pixX, pixY);
                        Color nextCurrentPixelColor = heatmapGradient.Evaluate(nextCenterColor.a * alpha);

                        if (nextCurrentPixelColor.a > currentPixelColor.a)
                        {
                            Color interpolated = Color.Lerp(
                                currentPixelColor,
                                nextCurrentPixelColor,
                                1 - blendIntensity + nextCurrentPixelColor.a * blendIntensity
                            );

                            interpolated.a = nextCurrentPixelColor.a;

                            collisionInfo.heatmapTexture.SetPixel(pixX, pixY, interpolated);
                        }
                    }
                }
            }

            collisionInfo.heatmapTexture.Apply();
            ProcessTextures(updatedPixels.ToArray());
        }

        private void ProcessTextures(Vector2Int[] pixels)
        {
            if (renderer == null)
                throw new MissingComponentException(
                    $"[{nameof(HeatmapSurface)}] No mesh renderer could be found, which is required to create the heatmap!");

            Color originalColor, heatmapColor;

            for (int i = 0; i < pixels.Length; i++)
            {
                originalColor = collisionInfo.originalTexture.GetPixel(pixels[i].x, pixels[i].y);
                heatmapColor = collisionInfo.heatmapTexture.GetPixel(pixels[i].x, pixels[i].y);
                heatmapColor = Color.Lerp(originalColor, heatmapColor, heatmapColor.a);

                collisionInfo.combinedTexture.SetPixel(
                    pixels[i].x,
                    pixels[i].y,
                    Color.Lerp(
                        originalColor,
                        heatmapColor,
                        heatmapTransparency
                    )
                );
            }

            collisionInfo.combinedTexture.Apply();
        }
    }
}