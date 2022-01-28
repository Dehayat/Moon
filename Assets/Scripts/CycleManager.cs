using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class CycleManager : MonoBehaviour
{
    private List<Character> allCharacters;
    private void Awake()
    {
        allCharacters = new List<Character>();
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
        ChooseWolfs();
        if (WorldData.instance.randomizePositions)
        {
            RandomizePositions();
        }
        StartCoroutine(DelayStartDay());
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
            StartCoroutine(DelayStartNight());
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

    IEnumerator DelayStartNight()
    {
        yield return new WaitForSeconds(0.5f);
        StartNightCycle();
    }

    private void StartNightCycle()
    {
        for (int i = 0; i < wolfs.Count; i++)
        {
            if (allCharacters[wolfs[i]].isAlive)
            {
                allCharacters[wolfs[i]].ChooseWolfAttack();
            }
        }
        EndNightCycle();
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
