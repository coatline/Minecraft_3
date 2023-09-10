using System.Text;
using UnityEngine;

public class World
{
    // Contains info specific to the world

    public readonly WorldBuilder WorldBuilder;

    public readonly WorldSettings WorldSettings;
    public readonly Vector2 NoiseOffset;

    public readonly string StringSeed;
    public readonly int Seed;

    public World(WorldSettings worldSettings, int seed, Player playerPrefab)
    {
        this.WorldSettings = worldSettings;

        this.Seed = seed;

        Random.InitState(Seed);

        NoiseOffset = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));

        WorldBuilder = new WorldBuilder(SettingsManager.I.RenderDistance, this, playerPrefab);
    }

    public static int GetSeedFromHash(string stringSeed)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(stringSeed);
        Hash128 hash = new Hash128();
        HashUtilities.ComputeHash128(bytes, ref hash);

        return hash.GetHashCode();
    }

    public void Update(float deltaTime)
    {
        WorldBuilder.Update(deltaTime);
    }
}
