using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] MeshRenderer terrainRenderer;
    [SerializeField] MeshCollider terrainCollider;
    [SerializeField] MeshFilter terrainFilter;
    MeshBuilder meshBuilder;
    Mesh terrainMesh;

    Vector3 nextPosition;

    public void Setup(Chunk chunk)
    {
        terrainMesh = new Mesh();
        meshBuilder = new MeshBuilder(chunk);

        terrainFilter.sharedMesh = terrainMesh;

        chunk.MeshDataReady += MeshDataReady;
        chunk.Generated += ChunkGenerated;
    }

    void MeshDataReady()
    {
        meshBuilder.AssignMesh(terrainMesh, terrainCollider);
        transform.position = nextPosition;
    }

    void ChunkGenerated(Vector2Int position, Chunk chunk)
    {
        meshBuilder.BuildMesh(chunk.blocksToBuildFaceOn);

        nextPosition = new Vector3(position.x * chunk.Size, 0, position.y * chunk.Size);
    }
}
