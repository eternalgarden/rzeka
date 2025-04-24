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
        readonly Subject<long> _lateUpdate = new();
        readonly Subject<long> _fixedUpdate = new();
        internal IObservable<long> EveryUpdate => _everyUpdate.AsObservable();
        internal IObservable<long> EveryLateUpdate => _lateUpdate.AsObservable();
        internal IObservable<long> EveryFixedUpdate => _fixedUpdate.AsObservable();

        long _updateTick = 0;
        long _lateUpdateTick = 0;
        long _fixedUpdateTick = 0;

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

        static void Initialize()
        {
            if (!_initialized)
            {
                new GameObject("NewDispatcher").AddComponent<NewDispatcher>();
            }
        }

        void Awake()
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

        void Update()
        {
            _everyUpdate.OnNext(_updateTick++);
        }

        void LateUpdate()
        {
            _lateUpdate.OnNext(_lateUpdateTick++);
        }

        void FixedUpdate()
        {
            _fixedUpdate.OnNext(_fixedUpdateTick++);
        }

        void OnDestroy()
        {
            _initialized = false;
            _instance = null;
        }

        void RunUpdateObservables()
        {
            throw new NotImplementedException();
        }
    }
}
