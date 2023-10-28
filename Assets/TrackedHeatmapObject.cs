using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TrackedHeatmapObject : MonoBehaviour
{
    [SerializeField] private Texture3D collisionMap;
    private static readonly int CollisionMap = Shader.PropertyToID("_CollisionMap");
    private static readonly int CollisionRoomSize = Shader.PropertyToID("_CollisionRoomSize");

    [SerializeField] private float trackedRoomSize = 30;

    [SerializeField] private float precision = .5f;

    private void Awake()
    {
        collisionMap = Shader.GetGlobalTexture(CollisionMap) as Texture3D;
        if (collisionMap == null)
        {
            collisionMap = new Texture3D(128, 128, 128, TextureFormat.RGBA32, false);
        }

        collisionMap.SetPixels(collisionMap.GetPixels().Select(c => Color.black).ToArray());
        collisionMap.Apply();
        Shader.SetGlobalTexture(CollisionMap, collisionMap);
    }

    private void OnTriggerEnter(Collider other)
    {
        UpdateColor(other);
    }

    private void OnTriggerStay(Collider other)
    {
        UpdateColor(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Shader.SetGlobalFloat(CollisionRoomSize, trackedRoomSize);
        Shader.SetGlobalTexture(CollisionMap, collisionMap);
    }

    private void UpdateColor(Collider other)
    {
        //collisionMap = Shader.GetGlobalTexture(CollisionMap) as Texture3D;

        Vector3 collisionPosition = other.ClosestPoint(transform.position);
        // collisionPosition += new Vector3(trackedRoomSize, trackedRoomSize, trackedRoomSize);
        collisionPosition *= 10;

        //collisionPosition += new Vector3(trackedRoomSize, trackedRoomSize, trackedRoomSize);

        var color = collisionMap.GetPixel(
            Mathf.RoundToInt(collisionPosition.x),
            Mathf.RoundToInt(collisionPosition.y),
            Mathf.RoundToInt(collisionPosition.z)
        );

        color.r += 1;
        color.g += 1;
        color.b += 1;

        collisionMap.SetPixel(
            Mathf.RoundToInt(collisionPosition.x),
            Mathf.RoundToInt(collisionPosition.y),
            Mathf.RoundToInt(collisionPosition.z),
            color
        );

        collisionMap.Apply();
        Debug.Log($"Updated Pixels: {Mathf.RoundToInt(collisionPosition.x)},{Mathf.RoundToInt(collisionPosition.y)},{Mathf.RoundToInt(collisionPosition.z)} ");
    }
}