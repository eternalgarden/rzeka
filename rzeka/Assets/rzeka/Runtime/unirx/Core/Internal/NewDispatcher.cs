using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka.Unirx
{
    internal class NewDispatcher : MonoBehaviour
    {
        static NewDispatcher _instance;
        static bool _initialized;

        readonly Subject<long> _everyUpdate = new();
        internal IObservable<long> EveryUpdate => _everyUpdate.AsObservable();

        long _updateTick = 0;

        public static bool IsInitialized
        {
            get { return _initialized && _instance != null; }
        }

        internal static NewDispatcher Instance
        {
            get
            {
                if (Application.isPlaying == false)
                {
                    Debug.LogError("not ready to work with editor scirpts");
                    return null;
                }
                Initialize();
                return _instance;
            }
        }

        private static void Initialize()
        {
            if (!_initialized)
            {
                new GameObject("NewDispatcher").AddComponent<NewDispatcher>();
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _initialized = true;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != _instance)
                {
                    Debug.Log("There is already a MainThreadDispatcher in the scene. Removing myself...");
                    Destroy(this);
                }
            }
        }

        private void Update()
        {
            _everyUpdate.OnNext(_updateTick++);
        }

        private void OnDestroy()
        {
            _initialized = false;
            _instance = null;
        }

        private void RunUpdateObservables()
        {
            throw new NotImplementedException();
        }
    }
}
