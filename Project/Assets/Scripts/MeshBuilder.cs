using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MeshBuilder
{
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    readonly WorldBuilder worldBuilder;
    readonly Chunk chunk;

    bool generating;

    public MeshBuilder(Chunk chunk)
    {
        this.worldBuilder = chunk.WorldBuilder;
        this.chunk = chunk;

        triangles = new();
        colors = new();
        verts = new();
        uvs = new();
    }

    readonly List<Vector3> verts;
    readonly List<Vector2> uvs;
    readonly List<int> triangles;
    readonly List<Color> colors;

    public void UpdateMesh(List<Vector3Int> visibleBlocks)
    {
        //stopwatch.Restart();

        try
        {
            //if (generating)
            //    Thread.Sleep(1);

            // Generating borders. Wait.
            while (generatingBorder)
            {
                Debug.Log("Generatnig border.. Waiting...");
                Thread.Sleep(1);
            }


            generating = true;

            BuildMesh(visibleBlocks);

            generating = false;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        //stopwatch.Stop();
        //Debug.Log($"Meshbuilding: {stopwatch.ElapsedMilliseconds}");
    }

    Chunk queueSouthBorder;
    bool generatingBorder;
    public Chunk NorthChunk { get; set; }
    public Chunk SouthChunk { get; set; }
    public Chunk WestChunk { get; set; }
    public Chunk EastChunk { get; set; }

    public void BuildMesh(List<Vector3Int> visibleBlocks)
    {
        uvs.Clear();
        verts.Clear();
        colors.Clear();
        triangles.Clear();
        //blocksToRecheck.Clear();

        // Do this so we don't have to check for them every time. If they are null, don't do border checks; wait for them to generate where they will call updateborders on this
        NorthChunk = worldBuilder.TryGetChunkAt(chunk.X, chunk.Y + 1);
        SouthChunk = worldBuilder.TryGetChunkAt(chunk.X, chunk.Y - 1);
        WestChunk = worldBuilder.TryGetChunkAt(chunk.X - 1, chunk.Y);
        EastChunk = worldBuilder.TryGetChunkAt(chunk.X + 1, chunk.Y);

        if (SouthChunk == null)
            Debug.Log("Gulp... This had better be right!");

        //if (NorthChunk != null)
        //    while (NorthChunk.Generating)
        //    {
        //        Debug.Log("Waiting...");
        //        Thread.Sleep(1);
        //    }

        //if (SouthChunk != null)
        //    while (SouthChunk.Generating)
        //    {
        //        Debug.Log("Waiting...");
        //        Thread.Sleep(1);
        //    }

        //if (WestChunk != null)
        //    while (WestChunk.Generating)
        //    {
        //        Debug.Log("Waiting...");
        //        Thread.Sleep(1);
        //    }

        //if (EastChunk != null)
        //    while (EastChunk.Generating)
        //    {
        //        Debug.Log("Waiting...");
        //        Thread.Sleep(1);
        //    }

        if (NorthChunk != null)
            if (NorthChunk.Generating)
                NorthChunk = null;

        if (SouthChunk != null)
            if (SouthChunk.Generating)
                SouthChunk = null;

        if (WestChunk != null)
            if (WestChunk.Generating)
                WestChunk = null;

        if (EastChunk != null)
            if (EastChunk.Generating)
                EastChunk = null;

        for (int i = 0; i < visibleBlocks.Count; i++)
        {
            Vector3Int blockPos = visibleBlocks[i];
            int x = blockPos.x;
            int y = blockPos.y;
            int z = blockPos.z;

            int blockId = chunk[x, y, z];

            // If the block is air or water, continue
            if (blockId == 0 || blockId == 12)
                continue;

            Block block = BlockLoader.I.GetBlock(blockId);

            #region TopAndBottom

            // Create a face on the top of block
            if (y < chunk.HeightLimit - 1)
                if (IsTransparent(chunk[x, y + 1, z]))
                    AddFace(blockPos, Extensions.topFaces, block.GetTopTextureUvs());

            // Create a face on the bottom of block
            if (y > 0)
                if (IsTransparent(chunk[x, y - 1, z]))
                    AddFace(blockPos, Extensions.bottomFaces, block.GetBottomTextureUvs());

            #endregion

            #region ChunkBorders

            // If we are not checking out of bounds (Checking west)
            if (x > 0)
            {
                if (IsTransparent(chunk[x - 1, y, z]))
                    AddFace(blockPos, Extensions.leftFaces, block.GetSideTextureUvs());
            }
            // We were generated east of the player, check west.
            else
            {
                if (WestChunk != null)
                {
                    //while (westChunk.Generating)
                    //Thread.Sleep(1);

                    if (IsTransparent(WestChunk[WestChunk.Size - 1, y, z]))
                        AddFace(blockPos, Extensions.leftFaces, block.GetSideTextureUvs());
                }
                else
                {

                }
            }

            // If we are not checking out of bounds (Checking east)
            if (x < chunk.Size - 1)
            {
                if (IsTransparent(chunk[x + 1, y, z]))
                    AddFace(blockPos, Extensions.rightFaces, block.GetSideTextureUvs());
            }
            // We were generated west of the player, check east.
            else
            {
                if (EastChunk != null)
                {
                    //while (eastChunk.Generating)
                    //Thread.Sleep(1);

                    if (IsTransparent(EastChunk[0, y, z]))
                        AddFace(blockPos, Extensions.rightFaces, block.GetSideTextureUvs());
                }
                else
                {

                }
            }

            // If we are not checking out of bounds (Check north)
            if (z < chunk.Size - 1)
            {
                if (IsTransparent(chunk[x, y, z + 1]))
                    AddFace(blockPos, Extensions.frontFaces, block.GetSideTextureUvs());
            }
            // We were generated south of the player, check north.
            else
            {
                if (NorthChunk != null)
                {
                    //while (northChunk.Generating)
                    //Thread.Sleep(1);

                    if (IsTransparent(NorthChunk[x, y, 0]))
                        AddFace(blockPos, Extensions.frontFaces, block.GetSideTextureUvs());
                }
                else
                {

                }
            }

            // If we are not checking out of bounds (Check south)
            if (z > 0)
            {
                if (IsTransparent(chunk[x, y, z - 1]))
                    AddFace(blockPos, Extensions.backFaces, block.GetSideTextureUvs());
            }
            // We were generated north of the player, check south.
            else
            {
                if (SouthChunk != null)
                {
                    //while (southChunk.Generating)
                    //Thread.Sleep(1);

                    if (IsTransparent(SouthChunk[x, y, SouthChunk.Size - 1]))
                        AddFace(blockPos, Extensions.backFaces, block.GetSideTextureUvs());
                }
                else
                {

                }
            }
        }

        #endregion

        if (NorthChunk != null)
        {
            MeshBuilder mb = NorthChunk.ChunkRenderer.MeshBuilder;

            if (mb.SouthChunk == null)
            {
                // Tell the north chunk to update their southern border so the updates attach to their mesh
                //Debug.Log("I am telling other chunk to update southern...");
                //mb.UpdateSouthChunkBorder(chunk);
            }
        }

        if (SouthChunk != null)
            if (SouthChunk.ChunkRenderer.MeshBuilder.NorthChunk == null)
            {

            }

        // Update borders that we have queued up

        if (queueSouthBorder != null)
        {
            //Debug.Log("It was queued...");
            //UpdateSouthChunkBorder(queueSouthBorder, false);
        }
    }

    public void UpdateSouthChunkBorder(Chunk southChunk, bool callAssignMesh = true)
    {
        queueSouthBorder = null;

        // We are generating our new mesh...
        if (generating)
            if (SouthChunk == null)
            {
                // Queue it
                queueSouthBorder = southChunk;
                return;
            }
            else
                // We will be generating those borders. No need to worry.
                return;

        generatingBorder = true;

        SouthChunk = southChunk;

        for (int i = 0; i < chunk.SouthBorderBlocks.Count; i++)
        {
            Vector2Int blockPos = chunk.SouthBorderBlocks[i];

            int blockId = chunk[blockPos.x, blockPos.y, 0];
            Block block = BlockLoader.I.GetBlock(blockId);

            // If there is no block there, generate a face
            if (SouthChunk.NorthBorderBlocks.Contains(blockPos) == false)
                AddFace(new Vector3Int(blockPos.x, blockPos.y, 0), Extensions.backFaces, block.GetTopTextureUvs());
        }

        #region OldMethod
        //for (int x = 0; x < chunk.Size; x++)
        //{
        //    for (int y = 0; y < chunk.HeightLimit; y++)
        //    {
        //        int blockId = chunk[x, y, 0];

        //        // If the block is air or water, continue
        //        if (blockId == 0 || blockId == 12)
        //            continue;

        //        Block block = BlockLoader.I.GetBlock(blockId);

        //        if (IsTransparent(southChunk[x, y, southChunk.Size - 1]))
        //            AddFace(new Vector3Int(x, y, 0), Extensions.backFaces, block.GetSideTextureUvs());
        //    }
        //}
        #endregion

        if (callAssignMesh)
            chunk.MeshComplete();

        generatingBorder = false;
    }

    public void UpdateNorthChunkBorder(Chunk northChunk)
    {
        for (int x = 0; x < chunk.Size; x++)
        {
            for (int y = 0; y < chunk.HeightLimit; y++)
            {
                int blockId = chunk[x, y, chunk.Size - 1];

                // If the block is air or water, continue
                if (blockId == 0 || blockId == 12)
                    continue;

                Block block = BlockLoader.I.GetBlock(blockId);

                if (IsTransparent(northChunk[x, y, 0]))
                    AddFace(new Vector3Int(x, y, chunk.Size - 1), Extensions.frontFaces, block.GetSideTextureUvs());
            }
        }

        chunk.MeshComplete();
    }

    public void UpdateEastChunkBorder(Chunk eastChunk)
    {

    }

    public void UpdateWestChunkBorder(Chunk westChunk)
    {

    }

    void AddFace(Vector3Int pos, Vector3[] vertices, Vector2[] textureUvs)
    {
        for (int k = 0; k < 4; k++)
            verts.Add(pos + vertices[k]);

        triangles.AddRange(Extensions.GetTriangles(verts.Count));

        uvs.AddRange(textureUvs);
    }

    /// Returns true if it is air or transparent.
    bool IsTransparent(int id)
    {
        return id == 0 || BlockLoader.I.GetBlock(id).Transparent;
    }

    public void AssignMesh(Mesh terrainMesh, MeshCollider meshCollider)
    {
        if (generating) { Debug.LogWarning("Started generating new chunk before I could even assign the new mesh!"); return; }
        if (verts.Count != uvs.Count)
        {
            Debug.LogWarning("WRong!!!!");
            return;
        }

        terrainMesh.Clear();
        terrainMesh.SetVertices(verts);
        terrainMesh.SetTriangles(triangles, 0);
        terrainMesh.SetUVs(0, uvs);

        //meshCollider.sharedMesh = terrainMesh;
    }

    #region WaterMesh

    List<Vector3> waterVerts = new List<Vector3>();
    List<Vector2> waterUvs = new List<Vector2>();
    List<int> waterTris = new List<int>();

    void RebuildWaterMesh()
    {
        waterVerts.Clear();
        waterTris.Clear();
        waterUvs.Clear();

        //for (int i = 0; i < d.waterBlocksToBuildFaceOn.Count; i++)
        //{
        //    int x = d.waterBlocksToBuildFaceOn[i].x;
        //    int y = d.waterBlocksToBuildFaceOn[i].y;
        //    int z = d.waterBlocksToBuildFaceOn[i].z;

        //    if (d.blocks[x, y, z] != 12) { Debug.Log("huh"); }

        //    Vector3 blockPos = new Vector3(x, y, z);

        //    byte faces = 0;

        //    //create face on top of block
        //    if (d.blocks[x, y + 1, z] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 0));
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 1));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 1));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].topTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].topTileTexture.textureY));
        //    }

        //    //create face on bottom of block
        //    if (d.blocks[x, y - 1, z] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 0));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].bottomTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].bottomTileTexture.textureY));
        //    }

        //    //create face on left of block
        //    if (x > 0 && d.blocks[x - 1, y, z] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 1));
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 0));
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 0));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureY));
        //    }

        //    //create face on right of block
        //    if (x < ChunkData.SIZE - 1 && d.blocks[x + 1, y, z] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 1));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureY));
        //    }

        //    //create face on back of block
        //    if (z > 0 && d.blocks[x, y, z - 1] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 0));
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 0));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 0));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureY));
        //    }

        //    //create face on front of block
        //    if (z < ChunkData.SIZE - 1 && d.blocks[x, y, z + 1] == 0)
        //    {
        //        waterVerts.Add(blockPos + new Vector3(1, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(1, 1, 1));
        //        waterVerts.Add(blockPos + new Vector3(0, 0, 1));
        //        waterVerts.Add(blockPos + new Vector3(0, 1, 1));
        //        faces++;

        //        waterUvs.AddRange(BlockHandler.I.GetTextureOnTextureAtlas(BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureX, BlockHandler.I.blocks[d.blocks[x, y, z]].sideTileTexture.textureY));
        //    }

        //    int tl = waterVerts.Count - 4 * faces;

        //    for (int k = 0; k < faces; k++)
        //    {
        //        //for each face, add 6 vertices's index
        //        waterTris.AddRange(new int[] { tl + (k * 4), tl + (k * 4) + 1, tl + (k * 4) + 2, tl + (k * 4) + 2, tl + (k * 4) + 1, tl + (k * 4) + 3 });
        //    }
        //}
    }

    public void AssignWaterMesh()
    {
        //chunk.waterMesh.Clear();
        //chunk.waterMesh.SetVertices(waterVerts);
        //chunk.waterMesh.SetTriangles(waterTris, 0);
        //chunk.waterMesh.SetUVs(0, waterUvs);
    }

    #endregion
}
