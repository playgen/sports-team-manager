using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ObserverMonoBehaviour : MonoBehaviour, IObserver<KeyValueMessage>
{
    [SerializeField]
    private string _typeName;
    [SerializeField]
    private string _methodName;
    private List<IDisposable> _unsubscribers = new List<IDisposable>();

    protected virtual void OnEnable()
    {
        var type = Type.GetType(_typeName);
        if (type != null && type.IsSubclassOf(typeof(ObservableMonoBehaviour)))
        {
            if (type.GetMethod(_methodName) != null)
            {
                var providers = FindObjectsOfType(type).ToList();
                foreach (var provider in providers)
                {
                    Subscribe(provider as IObservable<KeyValueMessage>);
                }
            }
            else
            {
                Debug.LogError(_methodName + " is invalid", this);
            }
        }
        else
        {
            Debug.LogError(_typeName + " is invalid", this);
        }
        
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    public virtual void Subscribe(IObservable<KeyValueMessage> provider)
    {
        if (provider != null)
        {
            _unsubscribers.Add(provider.Subscribe(this));
        }
    }

    public virtual void Unsubscribe()
    {
        foreach (var unsubscriber in _unsubscribers)
        {
            unsubscriber.Dispose();
        }
        _unsubscribers.Clear();
    }

    public void OnCompleted()
    {
        Unsubscribe();
    }

    public void OnError(Exception error)
    {

    }

    public void OnNext(KeyValueMessage message)
    {
        if (message.TypeName == _typeName && message.MethodName == _methodName)
        {
            Debug.Log("It worked!");
        }
    }
}
