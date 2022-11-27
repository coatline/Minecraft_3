using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] MeshRenderer terrainRenderer;
    [SerializeField] MeshCollider terrainCollider;
    [SerializeField] MeshFilter terrainFilter;
    public MeshBuilder MeshBuilder { get; private set; }
    Mesh terrainMesh;
    Chunk chunk;

    public bool Generating { get; private set; }

    private void Awake()
    {
        // Makes startup much slower
        //transform.SetParent(FindObjectOfType<Game>().transform);
    }

    public void Setup(Chunk chunk)
    {
        this.chunk = chunk;

        terrainMesh = new Mesh();
        MeshBuilder = new MeshBuilder(chunk);

        terrainFilter.sharedMesh = terrainMesh;
    }

    public void GenerateMesh()
    {
        Generating = true;

        MeshBuilder.UpdateMesh(chunk.VisibleBlocks);
        chunk.MeshComplete();

        Generating = false;
    }

    public void AssignMesh()
    {
        MeshBuilder.AssignMesh(terrainMesh, terrainCollider);
        transform.position = new Vector3(chunk.WorldX, 0, chunk.WorldY);
        chunk.AssignedMesh();
    }

    public void UpdateBorder(Chunk chunk, Vector2Int border)
    {
        if (border.x < 0)
            MeshBuilder.UpdateSouthChunkBorder(chunk);
    }
}
