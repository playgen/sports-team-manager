using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ObserverMonoBehaviour : MonoBehaviour, IObserver<KeyValueMessage>
{
    [SerializeField]
    protected KeyValueMessage[] _triggers;
    protected readonly List<IDisposable> _unsubscribers = new List<IDisposable>();

    protected virtual void OnEnable()
    {
        foreach (var trigger in _triggers)
        {
            var type = Type.GetType(trigger.TypeName);
            if (type != null && type.IsSubclassOf(typeof(ObservableMonoBehaviour)))
            {
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (type.GetMethod(trigger.MethodName, flags) != null)
                {
                    var providers = transform.root.gameObject.GetComponentsInChildren(type, true);
                    foreach (var provider in providers)
                    {
                        Subscribe(provider as IObservable<KeyValueMessage>);
                    }
                }
                else
                {
                    Debug.LogError(trigger.MethodName + " is invalid", this);
                }
            }
            else
            {
                Debug.LogError(trigger.TypeName + " is invalid", this);
            }
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

    public virtual void OnNext(KeyValueMessage message)
    {
        
    }
}
