using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    //[SerializeField] ParticleSystem blockBreakingParticlesPrefab;
    //[SerializeField] Texture2D[] blockBreakingFrames;
    //[SerializeField] Material blockBreakingMaterial;
    //[SerializeField] LayerMask terrainLayer;
    //SelfDestructTimer currentbbpSelfDestruct;
    //ParticleSystem currentbbp;
    //GameObject blockBreaking;
    //ChunkLoader cl;
    //Camera cam;

    //void Start()
    //{
    //    blockBreaking = GameObject.Find("BlockBreaking");
    //    cl = FindObjectOfType<ChunkLoader>();
    //    cam = GetComponentInChildren<Camera>();
    //}

    //float currentHardness;
    //Vector3Int prevBlock;

    //void Update()
    //{
    //    bool leftClicked = Input.GetMouseButton(0);
    //    bool rightClicked = Input.GetMouseButtonDown(1);

    //    Vector3 dir = cam.transform.forward;

    //    if (leftClicked || rightClicked)
    //    {
    //        RaycastHit hitInfo;

    //        if (Physics.Raycast(cam.transform.position, dir, out hitInfo, 7.5f, terrainLayer))
    //        {
    //            Vector3 pointInTargetBlock;

    //            if (leftClicked)
    //            {
    //                pointInTargetBlock = hitInfo.point + dir * .01f;
    //            }
    //            else
    //                pointInTargetBlock = hitInfo.point - dir * .01f;

    //            FindObjectOfType<ChunkLoader>().TryGetBlockAtGlobal(Mathf.FloorToInt(pointInTargetBlock.x), Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z));
    //            return;
    //            //print($"{pointInTargetBlock} {FindObjectOfType<ChunkLoader>().TryGetBlockAtGlobal((int)pointInTargetBlock.x, (int)pointInTargetBlock.y, (int)pointInTargetBlock.z).name}");
    //            //Vector2Int chunkPos = new Vector2Int(Mathf.FloorToInt((pointInTargetBlock.x - 1) / BlockData.SIZE), Mathf.FloorToInt((pointInTargetBlock.z - 1) / BlockData.SIZE));
    //            Vector2Int chunkPos = ChunkLoader.GetChunkCoordsFromGlobalCoords(pointInTargetBlock);
    //            Chunk chunk = cl.GetChunkFromChunkCoords(chunkPos);

    //            Vector3Int blockPos = ChunkLoader.GetLocalChunkCoords(pointInTargetBlock, chunkPos);
    //            Block block = chunk.meshData.GetBlockAt(blockPos.x, blockPos.y, blockPos.z);

    //            if (leftClicked)
    //            {
    //                // If the block has already been broken just not updated yet or the block is unbreakable return
    //                if (!block || block.hardness < 0) { blockBreaking.gameObject.SetActive(false); return; }

    //                Vector3 worldBlockPos = new Vector3(Mathf.FloorToInt(pointInTargetBlock.x) + .5f, Mathf.FloorToInt(pointInTargetBlock.y) + .5f, Mathf.FloorToInt(pointInTargetBlock.z) + .5f);

    //                if (!currentbbp || blockPos != prevBlock)
    //                {
    //                    StopEffects();
    //                    currentbbp = Instantiate(blockBreakingParticlesPrefab, worldBlockPos, Quaternion.identity);
    //                    currentbbpSelfDestruct = currentbbp.GetComponent<SelfDestructTimer>();
    //                    currentbbp.startColor = block.particleColor;
    //                }

    //                if (blockPos != prevBlock)
    //                {
    //                    currentHardness = block.hardness;
    //                    blockBreaking.SetActive(true);
    //                }

    //                blockBreaking.transform.position = worldBlockPos;
    //                blockBreakingMaterial.mainTexture = blockBreakingFrames[(int)((blockBreakingFrames.Length - 1) * ((block.hardness - currentHardness) / block.hardness))];

    //                // Break block
    //                if (currentHardness <= 0)
    //                {
    //                    if (chunk.updateMeshes) { return; }

    //                    chunk.ModifyBlock(blockPos, 0);

    //                    //if (blockPos.x == 1)
    //                    //{
    //                    //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x - 1, chunkPos.y));
    //                    //    c.d.blocks[ChunkData.SIZE + 1, blockPos.y, blockPos.z] = 0;
    //                    //    c.meshData.UpdateTerrain = true;
    //                    //    //c.RemoveBlock(new Vector3Int(BlockData.SIZE + 1, blockPos.y, blockPos.z));
    //                    //}
    //                    //else if (blockPos.x == ChunkData.SIZE)
    //                    //{
    //                    //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x + 1, chunkPos.y));
    //                    //    c.d.blocks[0, blockPos.y, blockPos.z] = 0;
    //                    //    c.meshData.UpdateTerrain = true;
    //                    //    //c.RemoveBlock(new Vector3Int(0, blockPos.y, blockPos.z));
    //                    //}
    //                    //if (blockPos.z == 1)
    //                    //{
    //                    //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y - 1));
    //                    //    c.d.blocks[blockPos.x, blockPos.y, ChunkData.SIZE + 1] = 0;
    //                    //    c.meshData.UpdateTerrain = true;
    //                    //    //c.RemoveBlock(new Vector3Int(blockPos.x, blockPos.y, BlockData.SIZE + 1));
    //                    //}
    //                    //else if (blockPos.z == ChunkData.SIZE)
    //                    //{
    //                    //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y + 1));
    //                    //    c.d.blocks[blockPos.x, blockPos.y, 0] = 0;
    //                    //    c.meshData.UpdateTerrain = true;
    //                    //    //c.RemoveBlock(new Vector3Int(blockPos.x, blockPos.y, 0));
    //                    //}

    //                    currentbbp.Emit(15);
    //                    StopEffects();
    //                    currentbbp = null;
    //                }
    //                else
    //                {
    //                    currentHardness = Mathf.Clamp(currentHardness - Time.deltaTime, 0, block.hardness);
    //                }
    //            }
    //            else
    //            {
    //                if (Vector2.Distance(pointInTargetBlock, cam.transform.position) < .75f)
    //                {
    //                    return;
    //                }

    //                chunk.ModifyBlock(blockPos, 10);

    //                //if (blockPos.x == 1)
    //                //{
    //                //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x - 1, chunkPos.y));
    //                //    c.d.blocks[ChunkData.SIZE + 1, blockPos.y, blockPos.z] = 7;
    //                //    c.meshData.UpdateTerrain = true;
    //                //    //c.AddBlock(blockPos, 7);
    //                //}
    //                //else if (blockPos.x == ChunkData.SIZE)
    //                //{
    //                //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x + 1, chunkPos.y));
    //                //    c.d.blocks[0, blockPos.y, blockPos.z] = 7;
    //                //    c.meshData.UpdateTerrain = true;
    //                //    //c.AddBlock(blockPos, 7);
    //                //}
    //                //if (blockPos.z == 1)
    //                //{
    //                //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y - 1));
    //                //    c.d.blocks[blockPos.x, blockPos.y, ChunkData.SIZE + 1] = 7;
    //                //    c.meshData.UpdateTerrain = true;
    //                //    //c.AddBlock(blockPos, 7);
    //                //}
    //                //else if (blockPos.z == ChunkData.SIZE)
    //                //{
    //                //    var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y + 1));
    //                //    c.d.blocks[blockPos.x, blockPos.y, 0] = 7;
    //                //    c.meshData.UpdateTerrain = true;
    //                //    //c.AddBlock(blockPos, 7);
    //                //}

    //                blockBreaking.SetActive(false);
    //            }

    //            prevBlock = blockPos;
    //        }
    //        else
    //        {
    //            StopEffects();
    //        }
    //    }
    //    else
    //    {
    //        StopEffects();
    //        prevBlock = new Vector3Int(0, -100, 0);
    //    }
    //}

    //void StopEffects()
    //{
    //    if (currentbbpSelfDestruct)
    //    {
    //        currentbbp.Stop();
    //        currentbbpSelfDestruct.Die();
    //        currentbbpSelfDestruct = null;
    //        currentbbp = null;
    //    }

    //    blockBreaking.SetActive(false);

    //}
}


//        if (leftClicked || rightClicked)
//        {
//            RaycastHit hitInfo;

//            if (Physics.Raycast(cam.transform.position, dir, out hitInfo, 7.5f, terrainLayer))
//            {
//                Vector3 pointInTargetBlock;

//                if (leftClicked)
//                {
//                    pointInTargetBlock = hitInfo.point + dir * .01f;
//                }
//                else
//                    pointInTargetBlock = hitInfo.point - dir * .01f;


//                Vector2Int chunkPos = new Vector2Int(Mathf.FloorToInt((pointInTargetBlock.x - 1) / BlockData.SIZE), Mathf.FloorToInt((pointInTargetBlock.z - 1) / BlockData.SIZE));
//                Chunk chunk = cl.GetChunkFromChunkCoords(chunkPos);

//                Vector3Int blockPos = ChunkLoader.GetLocalChunkCoords(pointInTargetBlock, chunkPos);
//                Block block = chunk.meshData.GetBlockAt(blockPos.x, blockPos.y, blockPos.z);


//                if (leftClicked)
//                {
//                    // If the block has already been broken just not updated yet or the block is unbreakable return
//                    if (!block || block.hardness < 0) { blockBreaking.gameObject.SetActive(false); return; }

//                    Vector3 worldBlockPos = new Vector3(Mathf.FloorToInt(pointInTargetBlock.x) + .5f, Mathf.FloorToInt(pointInTargetBlock.y) + .5f, Mathf.FloorToInt(pointInTargetBlock.z) + .5f);

//                    if (!currentbbp || blockPos != prevBlock)
//                    {
//                        StopEffects();
//                        currentbbp = Instantiate(blockBreakingParticlesPrefab, worldBlockPos, Quaternion.identity);
//                        currentbbpSelfDestruct = currentbbp.GetComponent<SelfDestructTimer>();
//                        currentbbp.startColor = block.particleColor;
//                    }

//                    if (blockPos != prevBlock)
//                    {
//                        currentHardness = block.hardness;
//                        blockBreaking.SetActive(true);
//                    }

//                    blockBreaking.transform.position = worldBlockPos;
//                    blockBreakingMaterial.mainTexture = blockBreakingFrames[(int)((blockBreakingFrames.Length - 1) * ((block.hardness - currentHardness) / block.hardness))];

//                    // Break block
//                    if (currentHardness <= 0)
//                    {
//                        if (chunk.updateTerrain) { return; }

//                        chunk.RemoveBlock(new Vector3Int(blockPos.x, blockPos.y, blockPos.z));

//                        if (blockPos.x == 1)
//                        {
//                            var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x - 1, chunkPos.y));
//                            c.d.blocks[BlockData.SIZE + 1, blockPos.y, blockPos.z] = 0;
//                            c.meshData.UpdateTerrain = true;
//                            //c.RemoveBlock(new Vector3Int(BlockData.SIZE + 1, blockPos.y, blockPos.z));
//                        }
//                        else if (blockPos.x == BlockData.SIZE)
//                        {
//                            var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x + 1, chunkPos.y));
//                            c.d.blocks[0, blockPos.y, blockPos.z] = 0;
//                            c.meshData.UpdateTerrain = true;
//                            //c.RemoveBlock(new Vector3Int(0, blockPos.y, blockPos.z));
//                        }
//                        if (blockPos.z == 1)
//                        {
//                            var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y - 1));
//                            c.d.blocks[blockPos.x, blockPos.y, BlockData.SIZE + 1] = 0;
//                            c.meshData.UpdateTerrain = true;
//                            //c.RemoveBlock(new Vector3Int(blockPos.x, blockPos.y, BlockData.SIZE + 1));
//                        }
//                        else if (blockPos.z == BlockData.SIZE)
//                        {
//                            var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y + 1));
//                            c.d.blocks[blockPos.x, blockPos.y, 0] = 0;
//                            c.meshData.UpdateTerrain = true;
//                            //c.RemoveBlock(new Vector3Int(blockPos.x, blockPos.y, 0));
//                        }

//                        currentbbp.Emit(15);
//                        StopEffects();
//                        currentbbp = null;
//                    }
//                    else
//                    {
//                        currentHardness = Mathf.Clamp(currentHardness - Time.deltaTime, 0, block.hardness);
//                    }
//                }
//                else
//                {
//                    if (Vector2.Distance(pointInTargetBlock, cam.transform.position) < .75f)
//                    {
//                        return;
//                    }

//                    chunk.AddBlock(blockPos, 7);

//                    if (blockPos.x == 1)
//                    {
//                        var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x - 1, chunkPos.y));
//                        c.d.blocks[BlockData.SIZE + 1, blockPos.y, blockPos.z] = 7;
//                        c.meshData.UpdateTerrain = true;
//                        //c.AddBlock(blockPos, 7);
//                    }
//                    else if (blockPos.x == BlockData.SIZE)
//                    {
//                        var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x + 1, chunkPos.y));
//                        c.d.blocks[0, blockPos.y, blockPos.z] = 7;
//                        c.meshData.UpdateTerrain = true;
//                        //c.AddBlock(blockPos, 7);
//                    }
//                    if (blockPos.z == 1)
//                    {
//                        var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y - 1));
//                        c.d.blocks[blockPos.x, blockPos.y, BlockData.SIZE + 1] = 7;
//                        c.meshData.UpdateTerrain = true;
//                        //c.AddBlock(blockPos, 7);
//                    }
//                    else if (blockPos.z == BlockData.SIZE)
//                    {
//                        var c = cl.GetChunkFromChunkCoords(new Vector2Int(chunkPos.x, chunkPos.y + 1));
//                        c.d.blocks[blockPos.x, blockPos.y, 0] = 7;
//                        c.meshData.UpdateTerrain = true;
//                        //c.AddBlock(blockPos, 7);
//                    }

//                    blockBreaking.SetActive(false);
//                }

//                prevBlock = blockPos;
//            }
//            else
//            {
//                StopEffects();
//            }
//        }
//        else
//        {
//            StopEffects();
//            prevBlock = new Vector3Int(0, -100, 0);
//        }
//    }

//    void StopEffects()
//    {
//        if (currentbbpSelfDestruct)
//        {
//            currentbbp.Stop();
//            currentbbpSelfDestruct.Die();
//            currentbbpSelfDestruct = null;
//            currentbbp = null;
//        }

//        blockBreaking.SetActive(false);

//    }
//}