using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    /// <summary>
    /// EnterとExitのタイミングで通知が送られる
    /// </summary>
    public class TriggerSubject : MonoBehaviour, IObservable<bool>
    {
        [SerializeField] private string _targetTag;
        private List<IObserver<bool>> _observers = new();

        // 購読解除のためのヘルパークラス
        private class UnScriber : IDisposable
        {
            private readonly TriggerSubject _subject;
            private readonly IObserver<bool> _observer;
            public UnScriber(TriggerSubject subject, IObserver<bool> observer)
            {
                _observer = observer;
                _subject = subject;
            }
            public void Dispose()
            {
                _subject._observers.Remove(_observer);
            }
        }
        public IDisposable Subscribe(IObserver<bool> observer)
        {
            _observers.Add(observer);
            observer.OnNext(Triggerd);
            return new UnScriber(this, observer);
        }

        // EnterとExitのタイミングで通知が送られる
        public bool Triggerd { get; private set; } = false;

        private void OnTriggerEnter2D(Collider2D collider2D)
        {

            if (collider2D.CompareTag(_targetTag))
            {
                Triggerd = true;
                foreach (var observer in _observers)
                    observer.OnNext(Triggerd);
            }
        }

        private void OnTriggerExit2D(Collider2D collider2D)
        {
            if (collider2D.CompareTag(_targetTag))
            {
                Triggerd = false;
                foreach (var observer in _observers)
                    observer.OnNext(Triggerd);
            }
        }
        private void OnDestroy()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}