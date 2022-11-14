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
    Chunk chunk;

    public void Setup(Chunk chunk)
    {
        this.chunk = chunk;

        terrainMesh = new Mesh();
        meshBuilder = new MeshBuilder(chunk);

        terrainFilter.sharedMesh = terrainMesh;
    }

    public void GenerateInitialMesh()
    {
        GenerateMesh();
        chunk.InitialMeshComplete();
    }

    public void GenerateMesh()
    {
        meshBuilder.BuildMesh(chunk.BlocksToBuildFaceOn);
    }

    public void AssignMesh()
    {
        meshBuilder.AssignMesh(terrainMesh, terrainCollider);
        transform.position = new Vector3(chunk.WorldX, 0, chunk.WorldY);
    }

    public void UpdateBorder(Vector2Int border)
    {

    }
}
