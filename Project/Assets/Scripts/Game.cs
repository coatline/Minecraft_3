using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    public World World { get; private set; }

    [SerializeField] byte maxTerrainHeight;
    [SerializeField] TMP_Text seedNumberText;
    [SerializeField] Player playerPrefab;
    [SerializeField] string seedString;

    void Awake()
    {
        int seed = Random.seed;

        if (seedString.Length != 0)
            seed = World.GetSeedFromHash(seedString);

        seedNumberText.text += seed;

        World = new World(new WorldSettings(255, maxTerrainHeight, 40, 16), seed, playerPrefab);
    }

    private void Update()
    {
        World.Update(Time.deltaTime);
    }
}
