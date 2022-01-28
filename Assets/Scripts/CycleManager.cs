using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class CycleManager : MonoBehaviour
{
    public float stayNightDuration = 2.4f;
    public float durationBetweenWolfAttacks = 0.6f;

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
            StartCoroutine(NightSequence());
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
        yield return new WaitUntil(() => nightCallbackDone);
        WorldData.instance.nightAnim.blackScreenDone -= BlackScreenDone;
        EndNightCycle();
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
