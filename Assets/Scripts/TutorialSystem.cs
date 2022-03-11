using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSystem : MonoBehaviour
{
    //public Tutorial firstTutorial;
    public GameObject firstTutorialActivator;

    private void OnEnable()
    {
        //firstTutorial.closed += Tutorial1Closed;
        //WorldData.instance.isActionPaused = true;
        //firstTutorial.gameObject.SetActive(true);
        firstTutorialActivator.SetActive(true);
    }
    private void OnDisable()
    {
        //firstTutorial.closed -= Tutorial1Closed;
    }

    private void Tutorial1Closed()
    {
        //WorldData.instance.isActionPaused = false;
        //firstTutorial.closed -= Tutorial1Closed;
    }
}
