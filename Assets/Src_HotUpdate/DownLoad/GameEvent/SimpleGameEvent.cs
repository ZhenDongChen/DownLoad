
using System;
using System.Collections.Generic;

public delegate void EventCoreDelegate<T>(EventData<T> e);

public class EventData<T>
{
    public EventData(T data)
    {
        this.m_Data = data;
    }

    private T m_Data;

    public T Data
    {
        get
        {
            return m_Data;
        }
    }
    
}

public class SimpleGameEvent
{
    private static SimpleGameEvent instance;

    public static SimpleGameEvent Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SimpleGameEvent();
            }

            return instance;
        }
    }

    private Dictionary<string, Delegate> mDelegates = new Dictionary<string, Delegate>();
    
    public void Register<T>(string eventName,EventCoreDelegate<T> handler)
    {
        if (!mDelegates.ContainsKey(eventName))
        {
            mDelegates.Add(eventName,handler);
        }
    }

    public void Fire<T>(string eventName,EventData<T> data)
    {
        Delegate delegate1;
        if (mDelegates.TryGetValue(eventName, out delegate1))
        {
            EventCoreDelegate<T> callback = delegate1 as EventCoreDelegate<T>;
            callback?.Invoke(data);
        }
    }
}
