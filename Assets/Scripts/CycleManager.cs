using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class CycleManager : MonoBehaviour
{
    public float stayNightDuration = 2.4f;
    public float durationBetweenWolfAttacks = 0.6f;
    public float fadeMusicNightDuration = 1f;

    private List<Character> allCharacters;
    private void Awake()
    {
        allCharacters = new List<Character>();
        if (GameInfo.isTutorial)
        {
            WorldData.instance.tutorial.enabled = true;
        }
    }

    public void EndDay()
    {
        EndDayCycle();
    }

    public void AddCharacter(Character character)
    {
        allCharacters.Add(character);
    }
    public void RemoveCharacter(Character character)
    {
        if (allCharacters.Contains(character))
        {
            allCharacters.Remove(character);
        }
        else
        {
            Debug.LogError("Character " + character + " not found in cycle manager character list");
        }
    }

    private void Start()
    {
        if (BlackScreen.instance != null)
        {
            BlackScreen.instance.done += ScreenIsNotBlack;
            BlackScreen.instance.FadeFromBlack();
        }
        else
        {
            ScreenIsNotBlack();
        }
        if (!GameInfo.isTutorial)
        {
            ChooseWolfs();
        }
        else
        {
            AddWolfs();
        }
        if (WorldData.instance.randomizePositions && !GameInfo.isTutorial)
        {
            RandomizePositions();
        }
    }

    private void AddWolfs()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].isWolf)
            {
                wolfs.Add(i);
            }
        }
    }

    private void ScreenIsNotBlack()
    {
        BlackScreen.instance.done -= ScreenIsNotBlack;
        StartCoroutine(DelayStartDay());
        EventSystem.instance.QueueEvent("GameStarted");
    }

    private void RandomizePositions()
    {
        var nodes = WorldData.instance.map.GetRandomNodes(allCharacters.Count);
        for (int i = 0; i < allCharacters.Count; i++)
        {
            allCharacters[i].startNode = nodes[i];
        }
    }

    private List<int> wolfs = new List<int>();
    private void ChooseWolfs()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            allCharacters[i].isWolf = false;
        }
        while (wolfs.Count < WorldData.instance.wolfCount)
        {
            int random = UnityEngine.Random.Range(0, allCharacters.Count);
            if (!wolfs.Contains(random))
            {
                wolfs.Add(random);
                allCharacters[random].isWolf = true;
            }
        }
    }

    IEnumerator DelayStartDay()
    {
        yield return new WaitForSeconds(1f);
        StartDayCycle();
        EventSystem.instance.QueueEvent("DayStarted");
    }

    public void StartDayCycle()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].isAlive)
            {
                allCharacters[i].EnabelAction();
            }
        }
        WorldData.instance.gameUI.EnableEndDayButton();
    }

    public void EndDayCycle()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].isAlive)
            {
                allCharacters[i].DisableAction();
            }
            allCharacters[i].ResetInfo();
        }
        if (AreAllWolvesDead())
        {
            WorldData.instance.gameUI.WinGame();
        }
        else
        {
            StartCoroutine(NightSequence());
            StartCoroutine(FadeAwayMusic());
        }
    }

    private float savedAudioVolume;
    public IEnumerator FadeAwayMusic()
    {
        AudioSource audio = WorldData.instance.audioSource;
        float timer = 0f;
        float startVol = audio.volume;
        savedAudioVolume = startVol;
        float endVold = 0.01f;
        while (timer < fadeMusicNightDuration)
        {
            audio.volume = Mathf.Lerp(startVol, endVold, timer / fadeMusicNightDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private bool AreAllWolvesDead()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].isWolf && allCharacters[i].isAlive)
            {
                return false;
            }
        }
        return true;
    }

    private bool nightCallbackDone = false;
    IEnumerator NightSequence()
    {

        WorldData.instance.nightAnim.ShowClouds();
        yield return new WaitForSeconds(0.5f);

        WorldData.instance.nightAnim.blackScreenDone += BlackScreenDone;
        nightCallbackDone = false;
        WorldData.instance.nightAnim.FadeToBlack();
        yield return new WaitUntil(() => nightCallbackDone);
        RemoveDeadPlayers();

        var listOfAttackedHumans = GetAndCalcAttackedHumans();

        for (int i = 0; i < listOfAttackedHumans.Count; i++)
        {
            listOfAttackedHumans[i].WolfAttackEffect();
            yield return new WaitForSeconds(durationBetweenWolfAttacks);
        }

        yield return new WaitForSeconds(stayNightDuration);

        nightCallbackDone = false;
        WorldData.instance.nightAnim.FadeFromBlack();
        WorldData.instance.nightAnim.HideClouds();
        yield return new WaitUntil(() => nightCallbackDone);
        WorldData.instance.nightAnim.blackScreenDone -= BlackScreenDone;
        StartCoroutine(FadeBackMusic());
        EndNightCycle();
    }

    public IEnumerator FadeBackMusic()
    {
        AudioSource audio = WorldData.instance.audioSource;
        float timer = 0f;
        float startVol = 0.01f;
        float endVold = savedAudioVolume;
        while (timer < fadeMusicNightDuration)
        {
            audio.volume = Mathf.Lerp(startVol, endVold, timer / fadeMusicNightDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void BlackScreenDone()
    {
        nightCallbackDone = true;
    }

    private void RemoveDeadPlayers()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (!allCharacters[i].isAlive && !allCharacters[i].isRemoved)
            {
                allCharacters[i].Remove();
            }
        }
    }
    private List<Character> GetAndCalcAttackedHumans()
    {

        List<Character> attackedCharacters = new List<Character>();
        for (int i = 0; i < wolfs.Count; i++)
        {
            if (allCharacters[wolfs[i]].isAlive)
            {
                var attackedCharacter = allCharacters[wolfs[i]].ChooseWolfAttack();
                if (attackedCharacter != null)
                {
                    attackedCharacters.Add(attackedCharacter);
                }
            }
        }
        return attackedCharacters;
    }

    private void EndNightCycle()
    {
        if (AreAllHumansDead())
        {
            GameOver();
        }
        else
        {
            StartCoroutine(DelayStartDay());
        }
    }

    private void GameOver()
    {
        WorldData.instance.gameUI.GameOver();
    }

    private bool AreAllHumansDead()
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (!allCharacters[i].isWolf && allCharacters[i].isAlive)
            {
                return false;
            }
        }
        return true;
    }

    public bool CanFindBloodType(BloodType bloodTypePreference)
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].isAlive && !allCharacters[i].isWolf && (allCharacters[i].bloodType.bloodType & bloodTypePreference) != 0)
            {
                return true;
            }
        }
        return false;
    }
}
