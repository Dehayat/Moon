using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public Text hpText;

    private Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }
    private void OnEnable()
    {
        character.damaged += UpdateHP;
    }
    private void OnDisable()
    {
        character.damaged -= UpdateHP;
    }

    private void UpdateHP()
    {
        hpText.text = character.currentHealth + " / " + character.Health;
    }

    private void Start()
    {
        UpdateHP();
    }

}
