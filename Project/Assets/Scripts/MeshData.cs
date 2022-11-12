using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    //readonly List<Vector3Int> blocksToRecheck;
    //readonly List<Vector3Int> blocksToBuildFaceOn;

    Chunk chunk;

    public MeshData(Chunk chunk)
    {
        this.chunk = chunk;
    }

    //public void Reset()
    //{
    //    blocksToBuildFaceOn.Clear();
    //    blocksToRecheck.Clear();
    //}

    //public void AddBlockToBuildFaceOn(Vector3Int pos)
    //{
    //    blocksToBuildFaceOn.Add(pos);
    //}

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();

    public void BuildMesh(List<Vector3Int> blocksToBuildFaceOn)
    {
        // TODO: Add some sort of List to contain the blocks to build a face on

        uvs.Clear();
        verts.Clear();
        colors.Clear();
        triangles.Clear();
        //blocksToRecheck.Clear();

        //Chunk topChunk = chunk.cl.GetChunkFromChunkCoords(chunk.chunkCoords + new Vector2Int(0, 1));
        //Chunk bottomChunk = chunk.cl.GetChunkFromChunkCoords(chunk.chunkCoords + new Vector2Int(0, -1));
        //Chunk leftChunk = chunk.cl.GetChunkFromChunkCoords(chunk.chunkCoords + new Vector2Int(-1, 0));
        //Chunk rightChunk = chunk.cl.GetChunkFromChunkCoords(chunk.chunkCoords + new Vector2Int(1, 0));

        for (int i = 0; i < blocksToBuildFaceOn.Count; i++)
        {
            int x = blocksToBuildFaceOn[i].x;
            int y = blocksToBuildFaceOn[i].y;
            int z = blocksToBuildFaceOn[i].z;

            int blockId = chunk[x, y, z];

            // If the block is air or water, continue
            if (blockId == 0 || blockId == 12)
                continue;

            Block block = BlockLoader.I.GetBlock(blockId);

            Vector3Int blockPos = new Vector3Int(x, y, z);

            #region TopAndBottom

            //create face on top of block
            if (y < chunk.HeightLimit - 1)
                if (IsTransparent(chunk[x, y + 1, z]))
                    AddTopFace(blockPos, block);

            //create face on bottom of block
            if (y > 0)
                if (IsTransparent(chunk[x, y - 1, z]))
                    AddBottomFace(blockPos, block);

            #endregion

            #region ChunkBorders

            //create face on left of block
            if (x > 0)
                if (IsTransparent(chunk[x - 1, y, z]))
                    AddLeftFace(blockPos, block);

            //else
            //{
            //    if (leftChunk)
            //    {
            //        while (leftChunk.generatingValues)
            //        {
            //            Thread.Sleep(1);
            //        }

            //        if (leftChunk.d.blocks[ChunkData.SIZE - 1, y, z] == 0 || leftChunk.meshData.GetBlockAt(ChunkData.SIZE - 1, y, z).transparent)
            //        {
            //            AddLeftFace(blockPos, block);
            //        }
            //    }
            //    else
            //    {
            //        blocksToRecheck.Add(blockPos);
            //    }
            //}

            //create face on right of block
            if (x < chunk.Size - 1)
                if (IsTransparent(chunk[x + 1, y, z]))
                    AddRightFace(blockPos, block);
            //else
            //{
            //    if (rightChunk)
            //    {
            //        while (rightChunk.generatingValues)
            //        {
            //            Thread.Sleep(1);
            //        }

            //        if (rightChunk.d.blocks[0, y, z] == 0 || rightChunk.meshData.GetBlockAt(0, y, z).transparent)
            //        {
            //            AddRightFace(blockPos, block);
            //        }
            //    }
            //    else
            //    {
            //        blocksToRecheck.Add(blockPos);
            //    }
            //}

            //create face on front of block
            if (z < chunk.Size - 1)
                if (IsTransparent(chunk[x, y, z + 1]))
                    AddFrontFace(blockPos, block);

            //else
            //{
            //    if (topChunk)
            //    {
            //        while (topChunk.generatingValues)
            //        {
            //            Thread.Sleep(1);
            //        }

            //        if (topChunk.d.blocks[x, y, 0] == 0 || topChunk.meshData.GetBlockAt(x, y, 0).transparent)
            //        {
            //            AddFrontFace(blockPos, block);
            //        }
            //    }
            //    else
            //    {
            //        blocksToRecheck.Add(blockPos);
            //    }
            //}

            //create face on back of block
            if (z > 0)
            {
                if (chunk[x, y, z - 1] == 0 /*|| GetBlockAt(x, y, z - 1).transparent*/)
                {
                    AddBackFace(blockPos, block);
                }
            }
            //else
            //{
            //    if (bottomChunk)
            //    {
            //        while (bottomChunk.generatingValues)
            //        {
            //            Thread.Sleep(1);
            //        }

            //        if (bottomChunk.d.blocks[x, y, ChunkData.SIZE - 1] == 0 || bottomChunk.meshData.GetBlockAt(x, y, ChunkData.SIZE - 1).transparent)
            //        {
            //            AddBackFace(blockPos, block);
            //        }
            //    }
            //    else
            //    {
            //        blocksToRecheck.Add(blockPos);
            //    }
            //}

            #endregion

            #region AddTriangles

            ////int tl = verts.Count - (4 * faces);

            ////for (int k = 0; k < faces; k++)
            ////{
            ////    //for each face, add 6 vertices's index
            ////    triangles.AddRange(new int[] { tl + (k * 4), tl + (k * 4) + 1, tl + (k * 4) + 2, tl + (k * 4) + 2, tl + (k * 4) + 1, tl + (k * 4) + 3 });
            ////}

            #endregion
        }

        //if((!topChunk && !leftChunk) || (!topChunk && !rightChunk) || (!bottomChunk && !leftChunk) || (!bottomChunk && !rightChunk))
        //{
        //    print("COORNER");
        //}
        //else
        //while (bottomChunk.regenerating)
        //{
        //    Thread.Sleep(1);
        //}

        //for (int k = 0; k < bottomChunk.meshData.blocksToRecheck.Count; k++)
        //{
        //    Vector3Int pos = bottomChunk.meshData.blocksToRecheck[k];

        //    if (pos.z != ChunkData.SIZE - 1) { continue; }

        //    if (chunk[pos.x, pos.y, 0] == 0)
        //    {
        //        AddFrontFace(new Vector3Int(pos.x, pos.y, -1), bottomChunk.meshData.GetBlockAt(pos.x, pos.y, pos.z));
        //    }
        //}

        #region AddFaceFunctions

        void AddTopFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
            {
                verts.Add(pos + Extensions.topFaces[k]);
            }

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetTopTextureOnTextureAtlas());
        }

        void AddBottomFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
                verts.Add(pos + Extensions.bottomFaces[k]);

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetBottomTextureOnTextureAtlas());
        }

        void AddLeftFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
                verts.Add(pos + Extensions.leftFaces[k]);

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetSideTextureOnTextureAtlas());
        }

        void AddRightFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
                verts.Add(pos + Extensions.rightFaces[k]);

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetSideTextureOnTextureAtlas());
        }

        void AddBackFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
                verts.Add(pos + Extensions.backFaces[k]);

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetSideTextureOnTextureAtlas());
        }

        void AddFrontFace(Vector3Int pos, Block block)
        {
            for (int k = 0; k < 4; k++)
                verts.Add(pos + Extensions.frontFaces[k]);

            triangles.AddRange(Extensions.GetTriangles(verts.Count));

            uvs.AddRange(block.GetSideTextureOnTextureAtlas());
        }
        #endregion
    }

    /// Returns true if it is air or transparent.
    bool IsTransparent(int id)
    {
        return id == 0 || BlockLoader.I.GetBlock(id).Transparent;
    }

    public void AssignMesh(Mesh terrainMesh, MeshCollider meshCollider)
    {
        terrainMesh.Clear();
        terrainMesh.SetVertices(verts);
        terrainMesh.SetTriangles(triangles, 0);
        terrainMesh.SetUVs(0, uvs);
        meshCollider.sharedMesh = terrainMesh;
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
