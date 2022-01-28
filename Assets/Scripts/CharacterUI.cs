using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public GameObject statsPanel;
    public Text hpText;
    public Text attackText;
    public Text wolfAttackText;
    public Text rangeText;
    public Text bloodTypeText;


    public void ShowStats()
    {
        statsPanel.SetActive(true);
    }
    public void HideStats()
    {
        statsPanel.SetActive(false);
    }

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
        attackText.text = character.Attack.ToString();
        wolfAttackText.text = character.WolfAttack.ToString();
        rangeText.text = character.Range.ToString();
        bloodTypeText.text = character.bloodType.bloodTypeName;
        UpdateHP();
    }

}
