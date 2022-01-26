using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum BloodType
{
    O = 1,
    A = 2,
    B = 4,
    AB = 8,
    IGNORE = 1024
}

[CreateAssetMenu(fileName = "bloodType", menuName = "Moon Data/BloodType")]
public class BloodTypeData : ScriptableObject
{
    public BloodType bloodType;
    public string bloodTypeName;

    public BloodType bloodTypePreference;
}
