using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameObject endDayButton;
    public GameObject gameOverOverlay;
    public GameObject winOverlay;
    public GameObject statsUI;
    public GameObject helpMenu;


    private void Update()
    {
        if (gameIsOver && Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    internal void ShowStats(Vector3 position, Character character)
    {
        statsUI.GetComponent<StatsUI>().SetStats(character);
        statsUI.transform.position = position;
        statsUI.SetActive(true);
    }

    internal void HideStats()
    {
        statsUI.SetActive(false);
    }

    public void EnableEndDayButton()
    {
        endDayButton.SetActive(true);
    }

    private bool gameIsOver = false;
    public void EndDay()
    {
        if (WorldData.instance.isActionPaused)
        {
            return;
        }
        WorldData.instance.cycleManager.EndDay();
        endDayButton.SetActive(false);
    }

    public void GameOver()
    {
        gameIsOver = true;
        gameOverOverlay.SetActive(true);
        WorldData.instance.isActionPaused = true;
    }

    public void WinGame()
    {
        gameIsOver = true;
        winOverlay.SetActive(true);
        WorldData.instance.isActionPaused = true;
    }

    public void ShowHelpMenu()
    {
        if (WorldData.instance.isActionPaused)
        {
            return;
        }
        WorldData.instance.isActionPaused = true;
        helpMenu.SetActive(true);
    }

    public void HideHelpMenu()
    {
        helpMenu.SetActive(false);
        WorldData.instance.isActionPaused = false;
    }
    private bool isLoadingMenu = false;
    public void MainMenu()
    {
        if (!isLoadingMenu)
        {
            isLoadingMenu = true;
            StartCoroutine(LoadMenu());
        }
    }

    IEnumerator LoadMenu()
    {
        isLoadingMenu = true;
        BlackScreen.instance.done += ScreenIsBlack;
        BlackScreen.instance.FadeToBlack();
        yield return new WaitUntil(() => isScreenBlack);
        GameInfo.wasInGame = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
    }
    private bool isScreenBlack = false;
    private void ScreenIsBlack()
    {
        isScreenBlack = true;
        BlackScreen.instance.done -= ScreenIsBlack;
    }

}
