using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ObservableMonoBehaviour : MonoBehaviour, IObservable<KeyValueMessage>
{
    protected readonly List<IObserver<KeyValueMessage>> _observers = new List<IObserver<KeyValueMessage>>();

    public IDisposable Subscribe(IObserver<KeyValueMessage> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
        return new Unsubscriber(_observers, observer);
    }

    protected void ShareEvent(string typeName, string methodName, params object[] passed)
    {
        var currentObservers = new List<IObserver<KeyValueMessage>>(_observers);
        var passedList = passed.ToList();
        passedList.Insert(0, gameObject);
        passed = passedList.ToArray();
        foreach (var observer in currentObservers)
        {
            if (_observers.Contains(observer))
            {
                observer.OnNext(new KeyValueMessage(typeName, methodName, passed));
            }
        }
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<KeyValueMessage>> _observers;
        private readonly IObserver<KeyValueMessage> _observer;

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

