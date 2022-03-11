using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventActivator : MonoBehaviour
{
    public string eventName;
    public Tutorial tutorial;
    public bool disableOnActivate = true;
    public float delay = 0f;

    private void OnEnable()
    {
        EventSystem.instance.ListenToEvent(eventName, DelayedShowTutorial);
    }
    private void OnDisable()
    {
        EventSystem.instance.IgnoreEvent(eventName, DelayedShowTutorial);
    }
    IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(delay);
        tutorial.ShowTutorial();
        if (disableOnActivate)
        {
            gameObject.SetActive(false);
        }
    }
    private void DelayedShowTutorial()
    {
        StartCoroutine(ShowTutorial());
    }
}
