using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class DataLibrary : Singleton<DataLibrary>
{
    public ChunkRenderer ChunkRendererPrefab { get; private set; }

    public Getter<Block> Blocks { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        ChunkRendererPrefab = Resources.LoadAll<ChunkRenderer>("Prefabs")[0];
        Blocks = new Getter<Block>(Resources.LoadAll<Block>("Blocks"));

        //Sounds = new Getter<Sound>(Resources.LoadAll<Item>("Sounds"));
    }

}
