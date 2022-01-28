using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class WorldData : MonoBehaviour
{
    public static WorldData instance;

    [Header("Scene Data")]
    public WorldMap map;
    public CycleManager cycleManager;
    public LayerMask charactersLayer;
    public LayerMask nodesLayer;
    public GameUI gameUI;
    public NightAnim nightAnim;

    [Header("Gameplay Data")]
    public int wolfCount = 2;
    public bool randomizePositions = false;

    [Header("Game State")]
    public bool isActionPaused = false;

    private void Awake()
    {
        instance = this;
    }
}
