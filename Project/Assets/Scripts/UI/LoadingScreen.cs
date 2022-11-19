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
    [SerializeField] TMP_Text loadedRatio;
    [SerializeField] Game game;
    WorldBuilder worldBuilder;
    bool valuesGenerated;
    int totalChunks;
    bool finished;

    void Start()
    {
        worldBuilder = game.World.WorldBuilder;

        totalChunks = worldBuilder.TotalChunks;
        worldBuilder.WorldLoaded += FinishedLoading;
    }

    void FinishedLoading()
    {
        finished = true;
        loadingScreen.SetActive(false);
    }

    void Update()
    {
        if (finished == false)
        {
            int loaded = worldBuilder.ChunksLoaded;

            loadedRatio.text = $"{loaded} / {worldBuilder.TotalChunks}";

            if (loaded < totalChunks)
            {
                loadingText.text = "Generating Values...";
                loadingBarFill.fillAmount = (float)(loaded / (float)worldBuilder.TotalChunks);
            }
            else if (loaded < totalChunks * 2)
            {
                loadingText.text = "Building Mesh...";
                loadingBarFill.fillAmount = (float)((loaded - totalChunks) / (float)worldBuilder.TotalChunks);
            }
            else
            {
                loadingText.text = "Assigning Mesh...";
                loadingBarFill.fillAmount = (float)((loaded - (totalChunks * 2)) / (float)worldBuilder.TotalChunks);
            }
        }
    }
}
