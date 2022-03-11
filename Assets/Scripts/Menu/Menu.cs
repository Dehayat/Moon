using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject main;
    public GameObject credits;
    public GameObject creditsButton;
    public GameObject instructions;
    public GameObject instructionsButton;
    public UnityEngine.EventSystems.EventSystem eventSystem;

    private void Start()
    {
        if (GameInfo.wasInGame)
        {
            GameInfo.wasInGame = false;
            if (BlackScreen.instance != null)
            {
                BlackScreen.instance.FadeFromBlack();
            }
        }
    }

    private bool isLoading = false;

    public void Tutorial()
    {
        Play();
        GameInfo.isTutorial = true;
    }

    public void Play()
    {
        GameInfo.isTutorial = false;
        if (isLoading)
        {
            return;
        }
        isLoading = true;
        eventSystem.SetSelectedGameObject(null);
        StartCoroutine(LoadGame());
    }
    IEnumerator LoadGame()
    {
        BlackScreen.instance.done += ScreenIsBlack;
        BlackScreen.instance.FadeToBlack();
        yield return new WaitUntil(() => isScreenBlack);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TestScene 1");
    }
    private bool isScreenBlack = false;
    private void ScreenIsBlack()
    {
        isScreenBlack = true;
        BlackScreen.instance.done -= ScreenIsBlack;
    }

    private GameObject lastSelected;
    public void Credits()
    {
        lastSelected = eventSystem.currentSelectedGameObject;
        main.SetActive(false);
        credits.SetActive(true);
        credits.GetComponent<Animator>().Play("Credits");
        eventSystem.SetSelectedGameObject(creditsButton);
    }
    public void OpenMenu()
    {
        instructions.SetActive(false);
        credits.SetActive(false);
        main.SetActive(true);
        eventSystem.SetSelectedGameObject(lastSelected);
    }
    public void OpenInstructions()
    {
        main.SetActive(false);
        instructions.SetActive(true);
        eventSystem.SetSelectedGameObject(instructionsButton);
    }
}
