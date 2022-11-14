using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    readonly WorldBuilder worldBuilder;
    readonly Chunk chunk;

    public MeshBuilder(Chunk chunk)
    {
        this.worldBuilder = chunk.WorldBuilder;
        this.chunk = chunk;
    }

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

        // Check all neighboring chunks for building a face
        bool checkAllChunkBorders = chunk.GenerationDirection.magnitude == 0;

        for (int x = 0; x < chunk.Size; x++)
            for (int z = 0; z < chunk.Size; z++)
            {
                int height = chunk.GetHeightMapAt(x, z);
                for (int y = 0; y < height; y++)
                {
                    //int x = blocksToBuildFaceOn[y].x;
                    //int y = blocksToBuildFaceOn[y].y;
                    //int z = blocksToBuildFaceOn[y].z;

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

                    // If we are not checking out of bounds (Checking west)
                    if (x > 0)
                    {
                        if (IsTransparent(chunk[x - 1, y, z]))
                            AddLeftFace(blockPos, block);
                    }
                    //// We were generated east of the player, check west.
                    //else /*if (chunk.GenerationDirection.x > 0 || checkAllChunkBorders)*/
                    //{
                    //    Chunk westChunk = worldBuilder.TryGetChunkAt(chunk.X - 1, chunk.Y);

                    //    if (westChunk != null)
                    //    {
                    //        if (IsTransparent(westChunk[westChunk.Size - 1, y, z]))
                    //        {
                    //            //Debug.Log($"{chunk.X}, {chunk.Y}, {westChunk.X}, {westChunk.Y} west");
                    //            AddLeftFace(blockPos, block);
                    //        }
                    //    }
                    //    //else
                    //    //{
                    //    //    Debug.Log("oof west");
                    //    //}
                    //}

                    // If we are not checking out of bounds (Checking east)
                    if (x < chunk.Size - 1)
                    {
                        if (IsTransparent(chunk[x + 1, y, z]))
                            AddRightFace(blockPos, block);
                    }
                    //// We were generated west of the player, check east.
                    //else /*if (chunk.GenerationDirection.x < 0 || checkAllChunkBorders)*/
                    //{
                    //    Chunk eastChunk = worldBuilder.TryGetChunkAt(chunk.X + 1, chunk.Y);

                    //    if (eastChunk != null)
                    //    {
                    //        if (IsTransparent(eastChunk[0, y, z]))
                    //        {
                    //            //Debug.Log($"{chunk.X}, {chunk.Y}, {eastChunk.X}, {eastChunk.Y} east");
                    //            AddRightFace(blockPos, block);
                    //        }
                    //    }
                    //    //else
                    //    //{
                    //    //    Debug.Log("oof east");
                    //    //}
                    //}

                    // If we are not checking out of bounds (Check north)
                    if (z < chunk.Size - 1)
                    {
                        if (IsTransparent(chunk[x, y, z + 1]))
                            AddFrontFace(blockPos, block);
                    }
                    //// We were generated south of the player, check north.
                    //else /*if (chunk.GenerationDirection.y < 0 || checkAllChunkBorders)*/
                    //{
                    //    Chunk northChunk = worldBuilder.TryGetChunkAt(chunk.X, chunk.Y + 1);

                    //    if (northChunk != null)
                    //    {
                    //        if (IsTransparent(northChunk[x, y, 0]))
                    //        {
                    //            //Debug.Log($"{chunk.X}, {chunk.Y}, {northChunk.X}, {northChunk.Y} north");
                    //            AddFrontFace(blockPos, block);
                    //        }
                    //    }
                    //    //else
                    //    //{
                    //    //    Debug.Log("oof north");
                    //    //}
                    //}

                    // If we are not checking out of bounds (Check south)
                    if (z > 0)
                    {
                        if (IsTransparent(chunk[x, y, z - 1]))
                            AddBackFace(blockPos, block);
                    }
                    //// We were generated north of the player, check south.
                    //else /*if (chunk.GenerationDirection.y > 0 || checkAllChunkBorders)*/
                    //{
                    //    Chunk southChunk = worldBuilder.TryGetChunkAt(chunk.X, chunk.Y - 1);

                    //    if (southChunk != null)
                    //    {

                    //        if (IsTransparent(southChunk[x, y, southChunk.Size - 1]))
                    //        {
                    //            //Debug.Log($"{chunk.X}, {chunk.Y}, {southChunk.X}, {southChunk.Y} south");
                    //            AddBackFace(blockPos, block);
                    //        }
                    //    }
                    //    //else
                    //    //{
                    //    //    Debug.Log("oof south");
                    //    //}
                    //}
                }
            }

        //for (int k = 0; k < bottomChunk.meshData.blocksToRecheck.Count; k++)
        //{
        //    Vector3Int pos = bottomChunk.meshData.blocksToRecheck[k];

        //    if (pos.z != ChunkData.SIZE - 1) { continue; }

        //    if (chunk[pos.x, pos.y, 0] == 0)
        //    {
        //        AddFrontFace(new Vector3Int(pos.x, pos.y, -1), bottomChunk.meshData.GetBlockAt(pos.x, pos.y, pos.z));
        //    }
        //}

        #endregion

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
