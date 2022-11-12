using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] MeshRenderer terrainRenderer;
    [SerializeField] MeshCollider terrainCollider;
    [SerializeField] MeshFilter terrainFilter;
    MeshData meshData;
    Mesh terrainMesh;

    public void Setup(Chunk chunk)
    {
        terrainMesh = new Mesh();
        meshData = new MeshData(chunk);

        terrainFilter.sharedMesh = terrainMesh;

        chunk.Generated += ChunkGenerated;
    }

    void ChunkGenerated(Vector2Int position, Chunk chunk)
    {
        meshData.BuildMesh(chunk.blocksToBuildFaceOn);
        meshData.AssignMesh(terrainMesh, terrainCollider);
    }
}
