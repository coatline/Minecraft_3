using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Image loadingBarFill;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] Game game;
    WorldBuilder worldBuilder;
    bool valuesGenerated;
    bool finished;

    void Start()
    {
        worldBuilder = game.World.WorldBuilder;
        worldBuilder.InitialValuesGenerated += InitialValuesGenerated;
        worldBuilder.WorldLoaded += FinishedLoading;

        loadingText.text = "Generating Values...";
    }

    void InitialValuesGenerated()
    {
        loadingText.text = "Building Mesh...";
    }

    void FinishedLoading()
    {
        finished = true;
        loadingScreen.SetActive(false);
    }

    void Update()
    {
        if (finished == false)
            loadingBarFill.fillAmount = (float)(worldBuilder.ChunksLoaded / (float)worldBuilder.TotalChunks);
    }
}
