/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Subjects;
using RzekaRiver.Utils;
using UnityEngine;

namespace RzekaRiver
{
    /*

    TODO QUESTION

    definitely much to decipher here

    */

    public partial class MainThreadDispatcher : MonoBehaviour
    {
        /* 🌊 ---- ---- */

        public enum CullingMode
        {
            /// <summary>
            /// Won't remove any MainThreadDispatchers.
            /// </summary>
            Disabled,

            /// <summary>
            /// Checks if there is an existing MainThreadDispatcher on Awake(). If so, the new dispatcher removes itself.
            /// </summary>
            Self,

            /// <summary>
            /// Search for excess MainThreadDispatchers and removes them all on Awake().
            /// </summary>
            All
        }

        ThreadSafeQueueWorker _queueWorker = new ThreadSafeQueueWorker();
        Action<Exception> _unhandledExceptionCallback = ex => Debug.LogException(ex); // default

        MicroCoroutine _updateMicroCoroutine = null;
        MicroCoroutine _fixedUpdateMicroCoroutine = null;
        MicroCoroutine _endOfFrameMicroCoroutine = null;


        [ThreadStatic] static object _mainThreadToken;
        static MainThreadDispatcher _instance;
        static bool _initialized;
        static bool _isQuitting;

        static MainThreadDispatcher Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        public static bool IsInMainThread
        {
            get
            {
                return (_mainThreadToken != null);
            }
        }

        public static CullingMode cullingMode = CullingMode.Self;

        public static string InstanceName
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException("MainThreadDispatcher is not initialized.");
                }

                return _instance.name;
            }
        }

        public static bool IsInitialized
        {
            get { return _initialized && _instance != null; }
        }

        //
        // ⛺ ─── Initialization & Awake ───────────────────────────────────────────────────
        //
        #region Initialization & Awake

        public static void Initialize()
        {
            if (!_initialized)
            {
                if (AreWeInEditMode())
                {
                    // Don't try to add a GameObject when the scene is not playing. Only valid in the Editor, EditorView.
                    return;
                }

                MainThreadDispatcher dispatcher = null;

                try
                {
                    dispatcher = GameObject.FindObjectOfType<MainThreadDispatcher>();
                }
                catch
                {
                    // Throw exception when calling from a worker thread.
                    var ex = new Exception("UniRx requires a MainThreadDispatcher component created on the main thread. Make sure it is added to the scene before calling UniRx from a worker thread.");
                    UnityEngine.Debug.LogException(ex);
                    throw ex;
                }

                if (_isQuitting)
                {
                    // don't create new instance after quitting
                    // avoid "Some objects were not cleaned up when closing the scene find target" error.
                    return;
                }

                if (dispatcher == null)
                {
                    // awake call immediately from UnityEngine
                    new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
                }
                else
                {
                    dispatcher.Awake(); // force awake
                }
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _mainThreadToken = new object();
                _initialized = true;

                _updateMicroCoroutine = new MicroCoroutine(ex => _unhandledExceptionCallback(ex));
                _fixedUpdateMicroCoroutine = new MicroCoroutine(ex => _unhandledExceptionCallback(ex));
                _endOfFrameMicroCoroutine = new MicroCoroutine(ex => _unhandledExceptionCallback(ex));

                StartCoroutine(RunUpdateMicroCoroutine());
                StartCoroutine(RunFixedUpdateMicroCoroutine());
                StartCoroutine(RunEndOfFrameMicroCoroutine());

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != _instance)
                {
                    if (cullingMode == CullingMode.Self)
                    {
                        // Try to destroy this dispatcher if there's already one in the scene.
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene. Removing myself...");
                        DestroyDispatcher(this);
                    }
                    else if (cullingMode == CullingMode.All)
                    {
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene. Cleaning up all excess dispatchers...");
                        CullAllExcessDispatchers();
                    }
                    else
                    {
                        Debug.LogWarning("There is already a MainThreadDispatcher in the scene.");
                    }
                }
            }
        }

        #endregion // ---------------------------------- Initialization & Awake -------------------------


        //
        // ⛺ ─── Public Observable Lifecycle ───────────────────────────────────────────────────
        //
        #region Public Observable Lifecycle

        Subject<Unit> update;
        Subject<Unit> lateUpdate;
        Subject<bool> onApplicationFocus;
        Subject<bool> onApplicationPause;
        Subject<Unit> onApplicationQuit;


        void Update()
        {
            if (update != null)
            {
                try
                {
                    update.OnNext(Unit.Default);
                }
                catch (Exception ex)
                {
                    _unhandledExceptionCallback(ex);
                }
            }

            _queueWorker.ExecuteAll(_unhandledExceptionCallback);
        }

        public static IObservable<Unit> UpdateAsObservable()
        {
            return Instance.update ?? (Instance.update = new Subject<Unit>());
        }

        void LateUpdate()
        {
            if (lateUpdate != null) lateUpdate.OnNext(Unit.Default);
        }

        public static IObservable<Unit> LateUpdateAsObservable()
        {
            return Instance.lateUpdate ?? (Instance.lateUpdate = new Subject<Unit>());
        }

        void OnApplicationFocus(bool focus)
        {
            if (onApplicationFocus != null) onApplicationFocus.OnNext(focus);
        }

        public static IObservable<bool> OnApplicationFocusAsObservable()
        {
            return Instance.onApplicationFocus ?? (Instance.onApplicationFocus = new Subject<bool>());
        }

        void OnApplicationPause(bool pause)
        {
            if (onApplicationPause != null) onApplicationPause.OnNext(pause);
        }

        public static IObservable<bool> OnApplicationPauseAsObservable()
        {
            return Instance.onApplicationPause ?? (Instance.onApplicationPause = new Subject<bool>());
        }

        void OnApplicationQuit()
        {
            _isQuitting = true;
            if (onApplicationQuit != null) onApplicationQuit.OnNext(Unit.Default);
        }

        public static IObservable<Unit> OnApplicationQuitAsObservable()
        {
            return Instance.onApplicationQuit ?? (Instance.onApplicationQuit = new Subject<Unit>());
        }

        #endregion // ---------------------------------- Public Observable Lifecycle -------------------------


        //
        // ⛺ ─── Private Enumerators ───────────────────────────────────────────────────
        //
        #region Private Enumerators

        IEnumerator RunUpdateMicroCoroutine()
        {
            while (true)
            {
                yield return null;
                _updateMicroCoroutine.Run();
            }
        }

        IEnumerator RunFixedUpdateMicroCoroutine()
        {
            while (true)
            {
                yield return YieldInstructionCache.WaitForFixedUpdate;
                _fixedUpdateMicroCoroutine.Run();
            }
        }

        IEnumerator RunEndOfFrameMicroCoroutine()
        {
            while (true)
            {
                yield return YieldInstructionCache.WaitForEndOfFrame;
                _endOfFrameMicroCoroutine.Run();
            }
        }

        #endregion // ---------------------------------- Private Enumerators -------------------------


        //
        // ⛺ ─── Post & Send ───────────────────────────────────────────────────
        //
        #region Post & Send

        /// <summary>Dispatch Asyncrhonous action.</summary>
        public static void Post(Action<object> action, object state)
        {
            if (EnqueuedActionOnEditorThreadInstead(action, state)) return;

            var dispatcher = Instance;
            if (!_isQuitting && !object.ReferenceEquals(dispatcher, null))
            {
                dispatcher._queueWorker.Enqueue(action, state);
            }
        }

        /// <summary>Dispatch Synchronous action if possible.</summary>
        public static void Send(Action<object> action, object state)
        {
            if (EnqueuedActionOnEditorThreadInstead(action, state)) return;

            if (_mainThreadToken != null)
            {
                try
                {
                    action(state);
                }
                catch (Exception ex)
                {
                    var dispatcher = MainThreadDispatcher.Instance;
                    if (dispatcher != null)
                    {
                        dispatcher._unhandledExceptionCallback(ex);
                    }
                }
            }
            else
            {
                Post(action, state);
            }
        }

        /// <summary>Run Synchronous action.</summary>
        public static void UnsafeSend(Action action)
        {
            if (UnsafelyInvokedActionOnEditorThreadInstead(action)) return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var dispatcher = MainThreadDispatcher.Instance;
                if (dispatcher != null)
                {
                    dispatcher._unhandledExceptionCallback(ex);
                }
            }
        }

        /// <summary>Run Synchronous action.</summary>
        public static void UnsafeSend<T>(Action<T> action, T state)
        {
            if (UnsafelyInvokedActionOnEditorThreadInstead(action, state)) return;

            try
            {
                action(state);
            }
            catch (Exception ex)
            {
                var dispatcher = MainThreadDispatcher.Instance;
                if (dispatcher != null)
                {
                    dispatcher._unhandledExceptionCallback(ex);
                }
            }
        }

        #endregion // ---------------------------------- Post & Send -------------------------


        //
        // ⛺ ─── Starting Coroutines ───────────────────────────────────────────────────
        //
        #region Starting Coroutines

        new public static Coroutine StartCoroutine(IEnumerator routine)
        {
            if (DispatchedRoutineOnEditorThreadInstead(routine)) return null;

            var dispatcher = Instance;
            if (dispatcher != null)
            {
                return (dispatcher as MonoBehaviour).StartCoroutine(routine);
            }
            else
            {
                return null;
            }
        }

        /// <summary>ThreadSafe StartCoroutine.</summary>
        public static void SendStartCoroutine(IEnumerator routine)
        {
            if (_mainThreadToken != null)
            {
                StartCoroutine(routine);
            }
            else
            {
                if (DispatchedRoutineOnEditorThreadInstead(routine)) return;

                var dispatcher = Instance;
                if (!_isQuitting && !object.ReferenceEquals(dispatcher, null))
                {
                    dispatcher._queueWorker.Enqueue(_ =>
                    {
                        var dispacher2 = Instance;
                        if (dispacher2 != null)
                        {
                            (dispacher2 as MonoBehaviour).StartCoroutine(routine);
                        }
                    }, null);
                }
            }
        }

        #region MicroCoroutines

        public static void StartUpdateMicroCoroutine(IEnumerator routine)
        {
            if (DispatchedRoutineOnEditorThreadInstead(routine)) return;

            // TODO Honestly is this dispatcher variable necessary at all
            // TODO cant we just call _updateMicroCoroutine
            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher._updateMicroCoroutine.AddCoroutine(routine);
            }
        }

        public static void StartFixedUpdateMicroCoroutine(IEnumerator routine)
        {
            if (DispatchedRoutineOnEditorThreadInstead(routine)) return;

            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher._fixedUpdateMicroCoroutine.AddCoroutine(routine);
            }
        }

        public static void StartEndOfFrameMicroCoroutine(IEnumerator routine)
        {
            if (DispatchedRoutineOnEditorThreadInstead(routine)) return;

            var dispatcher = Instance;
            if (dispatcher != null)
            {
                dispatcher._endOfFrameMicroCoroutine.AddCoroutine(routine);
            }
        }

        #endregion // Microcoroutines

        #endregion // ---------------------------------- Starting Coroutines -------------------------


        //
        // ⛺ ─── Cleanup I'd say ───────────────────────────────────────────────────
        //
        #region Cleanup I'd say

        static void DestroyDispatcher(MainThreadDispatcher aDispatcher)
        {
            if (aDispatcher != _instance)
            {
                // Try to remove game object if it's empty
                var components = aDispatcher.gameObject.GetComponents<Component>();
                if (aDispatcher.gameObject.transform.childCount == 0 && components.Length == 2)
                {
                    if (components[0] is Transform && components[1] is MainThreadDispatcher)
                    {
                        Destroy(aDispatcher.gameObject);
                    }
                }
                else
                {
                    // Remove component
                    MonoBehaviour.Destroy(aDispatcher);
                }
            }
        }

        static void CullAllExcessDispatchers()
        {
            var dispatchers = GameObject.FindObjectsOfType<MainThreadDispatcher>();
            for (int i = 0; i < dispatchers.Length; i++)
            {
                DestroyDispatcher(dispatchers[i]);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = GameObject.FindObjectOfType<MainThreadDispatcher>();
                _initialized = _instance != null;

                /* old neuecc comment

                Although `this` still refers to a gameObject, it won't be found.
                var foundDispatcher = GameObject.FindObjectOfType<MainThreadDispatcher>();

                if (foundDispatcher != null)
                {
                    select another game object
                    Debug.Log("new instance: " + foundDispatcher.name);
                    instance = foundDispatcher;
                    initialized = true;
                }
                */
            }
        }


        public static void RegisterUnhandledExceptionCallback(Action<Exception> exceptionCallback)
        {
            if (exceptionCallback == null) throw new ArgumentNullException("exceptiionCallback");

            Instance._unhandledExceptionCallback = exceptionCallback;
        }

        #endregion // ---------------------------------- Cleanup I'd say -------------------------


        //
        // ⛺ ─── Edit mode checking methods ───────────────────────────────────────────────────
        //
        #region Edit mode checking methods

        static bool AreWeInEditMode()
        {
#if UNITY_EDITOR
            return !ScenePlaybackDetector.IsPlaying;
#else
            return false;
#endif
        }

        /// <remarks>
        /// If scene isn't playing invoke action unsafely on editor thread.
        /// </remarks>
        static bool UnsafelyInvokedActionOnEditorThreadInstead(Action action)
        {
#if UNITY_EDITOR
            if (!ScenePlaybackDetector.IsPlaying)
            {
                EditorThreadDispatcher.Instance.UnsafeInvoke(action);
                return true;
            }
            else return false;
#else
            return false;
#endif
        }

        static bool UnsafelyInvokedActionOnEditorThreadInstead<T>(Action<T> action, T state)
        {
#if UNITY_EDITOR
            if (!ScenePlaybackDetector.IsPlaying)
            {
                EditorThreadDispatcher.Instance.UnsafeInvoke(action, state);
                return true;
            }
            else return false;
#else
            return false;
#endif
        }

        /// <remarks>
        /// If scene isn't playing dispatch requested routine on an editor thread
        /// dispatcher.
        /// </remarks>
        static bool EnqueuedActionOnEditorThreadInstead(Action<object> action, object state)
        {
#if UNITY_EDITOR
            if (!ScenePlaybackDetector.IsPlaying)
            {
                EditorThreadDispatcher.Instance.Enqueue(action, state);
                return true;
            }
            else return false;
#else
            return false;
#endif
        }

        /// <remarks>
        /// If scene isn't playing dispatch requested routine on an editor thread
        /// dispatcher.
        /// </remarks>
        static bool DispatchedRoutineOnEditorThreadInstead(IEnumerator routine)
        {
#if UNITY_EDITOR
            if (!ScenePlaybackDetector.IsPlaying)
            {
                EditorThreadDispatcher.Instance.PseudoStartCoroutine(routine);
                return true;
            }
            else return false;
#else
            return false;
#endif
        }

        #endregion // ---------------------------------- Edit mode checking methods -------------------------

        /* ---- ---- ⛺ */
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */