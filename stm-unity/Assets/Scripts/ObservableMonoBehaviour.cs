using UnityEngine;
using System;
using System.Collections.Generic;

public class ObservableMonoBehaviour : MonoBehaviour, IObservable<KeyValueMessage>
{
    private List<IObserver<KeyValueMessage>> _observers = new List<IObserver<KeyValueMessage>>();

    public IDisposable Subscribe(IObserver<KeyValueMessage> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
        return new Unsubscriber(_observers, observer);
    }

    protected void ShareEvent(string typeName, string methodName)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(new KeyValueMessage(typeName, methodName));
        }
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<KeyValueMessage>> _observers;
        private IObserver<KeyValueMessage> _observer;

        public Unsubscriber(List<IObserver<KeyValueMessage>> observers, IObserver<KeyValueMessage> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}

