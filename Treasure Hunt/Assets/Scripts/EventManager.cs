using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// code derived from:
// https://learn.unity.com/tutorial/create-a-simple-messaging-system-with-events#5cf5960fedbc2a281acd21fa
public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary;

    private static EventManager eventManager;
    
    // singleton design pattern
    public static EventManager instance
    {
        get
        {
            eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
            if (!eventManager)
            {
                Debug.LogError("PUT IT SOMEWHERE YA FOOL");
            }
            else
            {
                eventManager.Init();
            }
            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (eventManager == null) return;
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
