using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NewOverlayHeadmap : MonoBehaviour
{
    [SerializeField] private bool drawGizmos;

    [SerializeField] private Gradient heatmapGradient;
    [SerializeField] private float colorIncrement = .25f;
    [SerializeField, Range(0f, 1f)] private float blendIntensity = 1f;
    [SerializeField] int drawSize = 5;

    private Dictionary<int, Texture2D> _heatmapTextures = new();
    private List<Vector3> _impactPoints = new();

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
        if (!_heatmapTextures.TryGetValue(collision.gameObject.GetInstanceID(), out var heatmapTexture))
        {
            SetupNewCollisionObject(collision, out heatmapTexture);
        }


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

        Ray ray = new Ray(transform.position, -averageNormal);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 5);
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

    private void SetupNewCollisionObject(Collision other, out Texture2D texture)
    {
        var renderer = other.gameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            texture = new Texture2D(128, 128);
            return;
        }

        texture = renderer.material.mainTexture as Texture2D;

        if (texture == null)
        {
            texture = (renderer.material.mainTexture = new Texture2D(1028, 1028)) as Texture2D;
            texture!.SetPixels(texture.GetPixels().Select(_ => Color.clear).ToArray());
        }

        _heatmapTextures.Add(other.gameObject.GetInstanceID(), texture);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        for (int i = 0; i < _impactPoints.Count; i++)
        {
            Gizmos.DrawSphere(_impactPoints[i], .5f);
        }
    }
}