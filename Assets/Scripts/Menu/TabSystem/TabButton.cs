using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    public Text buttonText;
    public TabContent content;

    void Start()
    {
        buttonText.text = content.header;
    }
}
