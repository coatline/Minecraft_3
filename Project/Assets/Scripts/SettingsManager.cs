using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public event Action RenderDistanceChanged;

    [SerializeField] byte renderDistance;

    public byte RenderDistance => renderDistance;
}
