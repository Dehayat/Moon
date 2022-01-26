using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject endDayButton;
    public GameObject gameOverOverlay;
    public GameObject winOverlay;

    public void EnableEndDayButton()
    {
        endDayButton.SetActive(true);
    }

    public void EndDay()
    {
        WorldData.instance.cycleManager.EndDay();
        endDayButton.SetActive(false);
    }

    public void GameOver()
    {
        gameOverOverlay.SetActive(true);
    }

    public void WinGame()
    {
        winOverlay.SetActive(true);
    }
}
