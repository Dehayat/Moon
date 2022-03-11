using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "tabContent", menuName = "Moon Data/TabContent")]
public class TabContent : ScriptableObject
{
    public string header;
    [TextArea]
    public string description;
}
