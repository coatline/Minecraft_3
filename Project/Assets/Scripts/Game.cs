using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] TMP_Text seedNumber;
    [SerializeField] string seed;
    World world;

    void Awake()
    {
        world = new World(150, 255, 40, seed);
    }

    private void Start()
    {
        if (world.StringSeed == "")
            seedNumber.text = world.Seed.ToString();
        else
            seedNumber.text = world.StringSeed;
    }
}
