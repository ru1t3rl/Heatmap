using System.Collections.Generic;
using System.Linq;
using Ru1t3rl.Heatmap.Models;
using UnityEngine;

namespace Ru1t3rl.Heatmap.Trackers
{
    public class TrackedObjectHeatmap : MonoBehaviour
    {
        [SerializeField] private Gradient heatmapGradient;
        [SerializeField, Range(0f, 1f)] private float heatmapTransparency = 1f;
        [SerializeField, Range(0f, 1f)] private float blendIntensity = 1f;
        [SerializeField] private float colorIncrement = .1f;
        [SerializeField] private int drawSize = 5;
        [SerializeField] private string mainTextureProperty = "_MainTex";

        private Dictionary<int, HeatmapCollisionInfo> collisionsInfo = new();
        List<Vector2Int> updatedPixels = new();

        private float previousTransparency = 1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (previousTransparency != heatmapTransparency)
            {
                foreach (var key in collisionsInfo.Keys)
                {
                    collisionsInfo[key].UpdateHeatmapTransparency(heatmapTransparency, blendIntensity);
                }

                previousTransparency = heatmapTransparency;
            }
        }
#endif

        private void OnCollisionEnter(Collision other)
        {
            UpdateTexture(other);
        }

        private void OnCollisionStay(Collision other)
        {
            UpdateTexture(other);
        }

        private void UpdateTexture(Collision collision)
        {
            if (!collisionsInfo.TryGetValue(collision.gameObject.GetInstanceID(), out var collisionInfo))
            {
                SetupNewCollisionObject(collision, out collisionInfo);
            }

            Vector2 uv = Vector2.zero;

            Vector3 averageNormal = Vector3.zero;
            collision.contacts.ToList().ForEach(contact => { averageNormal += contact.normal; });
            averageNormal /= collision.contacts.Length;

            Ray ray = new Ray(transform.position, -averageNormal);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider is not MeshCollider)
                {
                    return;
                }

                uv = hit.textureCoord;
            }
            else
            {
                return;
            }

            uv.x *= collisionInfo.heatmapTexture.width;
            uv.y *= collisionInfo.heatmapTexture.height;

            Color currentCenterColor = collisionInfo.heatmapTexture.GetPixel((int)uv.x, (int)uv.y);
            Color nextCenterColor = heatmapGradient.Evaluate(currentCenterColor.a + colorIncrement);

            updatedPixels.Clear();

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
            ProcessTextures(collisionInfo, updatedPixels.ToArray());
        }

        private void ProcessTextures(HeatmapCollisionInfo collisionInfo, Vector2Int[] pixels)
        {
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
                        1
                    )
                );
            }

            collisionInfo.combinedTexture.Apply();
        }

        private void SetupNewCollisionObject(Collision other, out HeatmapCollisionInfo collisionInfo)
        {
            var renderer = other.gameObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                collisionInfo = null;
                return;
            }

            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);

            collisionInfo = new HeatmapCollisionInfo(mpb, mainTextureProperty);
            collisionsInfo.Add(other.gameObject.GetInstanceID(), collisionInfo);

            renderer.SetPropertyBlock(collisionInfo.materialPropertyBlock);
        }
    }
}