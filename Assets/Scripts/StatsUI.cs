using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public Text attackText;
    public Text wolfAttackText;
    public Text rangeText;
    public Text bloodTypeText;

    public void SetStats(Character character)
    {
        attackText.text = character.Attack.ToString();
        wolfAttackText.text = character.WolfAttack.ToString();
        rangeText.text = character.Range.ToString();
        bloodTypeText.text = character.bloodType.bloodTypeName;
    }
}
