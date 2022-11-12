using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    public World World { get; private set; }

    [SerializeField] byte maxTerrainHeight;
    [SerializeField] TMP_Text seedNumber;
    [SerializeField] Player playerPrefab;
    [SerializeField] string seed;

    void Awake()
    {
        World = new World(maxTerrainHeight, 255, 40, seed, playerPrefab);

        if (World.StringSeed == "")
            seedNumber.text = World.Seed.ToString();
        else
            seedNumber.text = World.StringSeed;
    }

    private void Update()
    {
        World.Update(Time.deltaTime);
    }
}
