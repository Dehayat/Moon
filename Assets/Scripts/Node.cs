using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int id;


    public List<Character> charactersInNode;

    public void EnterNode(Character character)
    {
        charactersInNode.Add(character);
    }
    public void ExitNode(Character character)
    {
        if (charactersInNode.Contains(character))
        {
            charactersInNode.Remove(character);
        }
        else
        {
            Debug.LogError("Character " + character + " not in node " + id);
        }
    }

    private void Awake()
    {
        charactersInNode = new List<Character>();
    }
}
