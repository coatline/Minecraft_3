using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block", fileName = "New Block")]

public class Block : ScriptableObject
{
    [SerializeField] Tile bottomTexture;
    [SerializeField] Tile sideTexture;
    [SerializeField] Tile topTexture;
    [SerializeField] bool transparent;
    [SerializeField] bool isAir;
    [SerializeField] byte id;

    public Tile BottomTexture => bottomTexture;
    public Tile SideTexture => sideTexture;
    public Tile TopTexture => topTexture;
    public bool Transparent => transparent;
    public bool IsAir => isAir;
    public byte Id => id;

    /// Dev tool
    public void SetId(byte newid) => id = newid;

    const short TEXTURE_ATLAS_SIZE = 1600;
    const byte BLOCK_PIXEL_SIZE = 16;

    public Vector2[] GetTextureOnTextureAtlas(byte x, byte y)
    {
        // Instead of doing this just hard code it into the block scriptable object for speed
        // Basically do the calculation for it.

        // 0,0  1,0  0,1  1,1

        return new Vector2[]{
            new Vector2(((float)(x * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE), ((float)(y * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE)),
            new Vector2(((float)(x * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE), (((float)(y + 1) * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE)),
            new Vector2((((float)(x + 1) * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE), ((float)(y * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE)),
            new Vector2((((float)(x + 1) * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE), (((float)(y + 1) * BLOCK_PIXEL_SIZE) / TEXTURE_ATLAS_SIZE)),
        };
    }

    // Hardcode this
    public Vector2[] GetTopTextureUvs() => GetTextureOnTextureAtlas(topTexture.TextureX, topTexture.TextureY);
    public Vector2[] GetSideTextureUvs() => GetTextureOnTextureAtlas(sideTexture.TextureX, sideTexture.TextureY);
    public Vector2[] GetBottomTextureUvs() => GetTextureOnTextureAtlas(bottomTexture.TextureX, bottomTexture.TextureY);

    //public Color particleColor;
    //public bool transparent;
    //public bool seeThrough;
    //public bool isWater;
    //public float hardness;
}

[System.Serializable]
public class Tile
{
    [SerializeField] byte textureX;
    [SerializeField] byte textureY;

    public byte TextureX => textureX;
    public byte TextureY => textureY;
}