using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [Header("Debug info")]
    public int id;
    public Character characterInNode = null;
    
    private Animator anim;

    public void Focus()
    {
        anim.Play("focus");
    }
    public void UnFocus()
    {
        anim.Play("default");
    }

    public void EnterNode(Character character)
    {
        characterInNode = character;
    }
    public void ExitNode(Character character)
    {
        if (characterInNode == character)
        {
            characterInNode = null;
        }
        else
        {
            Debug.LogError("Character " + character + " not in node " + id);
        }
    }

    public bool IsEmpty()
    {
        return characterInNode == null;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
}
