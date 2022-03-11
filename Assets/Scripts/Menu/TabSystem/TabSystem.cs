using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabSystem : MonoBehaviour
{
    public Text header;
    public Text description;
    public TabContent defaultContent;

    private void Start()
    {
        OpenTab(defaultContent);
    }

    public void OpenTab(TabContent content)
    {
        header.text = content.header;
        description.text = content.description;
    }
}
