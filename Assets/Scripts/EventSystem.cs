using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem instance;

    Queue<string> eventQueue1 = new Queue<string>();
    Queue<string> eventQueue2 = new Queue<string>();

    Dictionary<string, Action> eventHandlers = new Dictionary<string, Action>();

    private bool queue1IsActive = false;

    public void QueueEvent(string eventName)
    {
        if (queue1IsActive)
        {
            eventQueue2.Enqueue(eventName);
        }
        else
        {
            eventQueue1.Enqueue(eventName);
        }
    }
    public void ListenToEvent(string eventName, Action handler)
    {
        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers.Add(eventName, default);
        }
        eventHandlers[eventName] += handler;
    }
    public void IgnoreEvent(string eventName, Action handler)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] -= handler;
        }
    }

    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        queue1IsActive = !queue1IsActive;
        if (queue1IsActive)
        {
            while (eventQueue1.Count > 0)
            {
                string eventName = eventQueue1.Dequeue();
                if (eventHandlers.ContainsKey(eventName))
                {
                    eventHandlers[eventName]?.Invoke();
                }
            }
        }
        else
        {
            while (eventQueue2.Count > 0)
            {
                string eventName = eventQueue2.Dequeue();
                if (eventHandlers.ContainsKey(eventName))
                {
                    eventHandlers[eventName]?.Invoke();
                }
            }
        }
    }
}
