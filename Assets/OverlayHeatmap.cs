using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CollisionHeatmap : MonoBehaviour
{
    [SerializeField] private bool drawGizmos;
    [SerializeField] private Material heatmapMaterial;

    [SerializeField] private Gradient heatmapGradient;
    [SerializeField] private float colorIncrement = .25f;
    [SerializeField, Range(0f, 1f)] private float blendIntensity = 1f;
    [SerializeField] int drawSize = 5;
        
    private Texture2D heatmapTexture;
    private List<Vector3> _impactPoints = new();

    void Start()
    {
        // Create heatmap texture
        heatmapTexture = new Texture2D(1024, 1024);

        // Initialize heatmap pixels to zero
        for (int i = 0; i < heatmapTexture.width; i++)
        {
            for (int j = 0; j < heatmapTexture.height; j++)
            {
                heatmapTexture.SetPixel(i, j, Color.clear);
            }
        }

        // Assign texture to material
        heatmapMaterial.mainTexture = heatmapTexture;
    }

    void OnCollisionEnter(Collision collision)
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

        Color currentCenterColor = heatmapTexture.GetPixel((int)uv.x, (int)uv.y);
        Color nextCenterColor = heatmapGradient.Evaluate(currentCenterColor.a + colorIncrement);

        for (int i = -drawSize; i <= drawSize - 1; i++)
        {
            for (int j = -drawSize; j <= drawSize - 1; j++)
            {
                // Check if in circle radius
                if (i * i + j * j <= drawSize * drawSize + 1)
                {
                    int pixX = (int)uv.x + i;
                    int pixY = (int)uv.y + j;

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

        _impactPoints.Add(averagePoint);
        heatmapTexture.Apply();
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        for (int i = 0; i < _impactPoints.Count; i++)
        {
            Gizmos.DrawSphere(_impactPoints[i], .25f);
        }
    }
}