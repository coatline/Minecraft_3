using System.Text;
using UnityEngine;

public class World
{
    // Contains info specific to the world

    public readonly byte MaxTerrainHeight;
    public readonly Vector2 NoiseOffset;
    public readonly byte HeightLimit;
    public readonly byte WaterLevel;

    public readonly string StringSeed;
    public readonly int Seed;

    readonly WorldBuilder worldBuilder;

    public World(byte maxTerrainHeight, byte heightLimit, byte waterLevel, string stringSeed)
    {
        MaxTerrainHeight = maxTerrainHeight;
        HeightLimit = heightLimit;
        WaterLevel = waterLevel;
        StringSeed = stringSeed;

        if (stringSeed == "")
            Seed = Random.seed;
        else
        {
            byte[] bytes = Encoding.UTF8.GetBytes(stringSeed);
            Hash128 hash = new Hash128();
            HashUtilities.ComputeHash128(bytes, ref hash);

            Seed = hash.GetHashCode();
        }

        Random.InitState(Seed);

        NoiseOffset = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));

        worldBuilder = new WorldBuilder(SettingsManager.I.RenderDistance, 16, this);
    }
}
