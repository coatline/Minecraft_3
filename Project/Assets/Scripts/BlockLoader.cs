using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class BlockLoader : Singleton<BlockLoader>
{
    [SerializeField] bool setBlockIds;
    Block[] blocks;

    protected override void Awake()
    {
        Block[] unorganizedBlocks = (Resources.LoadAll<Block>("Blocks"));

        blocks = new Block[unorganizedBlocks.Length];

        for (int i = 0; i < unorganizedBlocks.Length; i++)
        {
            Block block = unorganizedBlocks[i];

            if (setBlockIds)
                if (block.IsAir)
                    block.SetId(0);
                else
                    for (byte k = 1; k < blocks.Length; k++)
                        if (blocks[k] == null)
                            block.SetId(k);

            blocks[block.Id] = block;
        }

        base.Awake();
    }

    public Block GetBlock(int id) => blocks[id];
}
