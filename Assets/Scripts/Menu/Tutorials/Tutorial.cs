using System;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Action closed;

    public void ShowTutorial()
    {
        gameObject.SetActive(true);
        WorldData.instance.isActionPaused = true;
    }

    public void CloseTutorial()
    {
        WorldData.instance.isActionPaused = false;
        gameObject.SetActive(false);
        closed?.Invoke();
    }
}
