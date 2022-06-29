/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/

#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#define SupportCustomYieldInstruction
#endif

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Rzeka
{
    public partial class MainThreadDispatcher
    {
#if UNITY_EDITOR

        // In UnityEditor's EditorMode can't instantiate and work MonoBehaviour.Update.
        // EditorThreadDispatcher use EditorApplication.update instead of MonoBehaviour.Update.
        class EditorThreadDispatcher
        {
            static object gate = new object();
            static EditorThreadDispatcher instance;

            public static EditorThreadDispatcher Instance
            {
                get
                {
                    // Activate EditorThreadDispatcher is dangerous, completely Lazy.
                    lock (gate)
                    {
                        if (instance == null)
                        {
                            instance = new EditorThreadDispatcher();
                        }

                        return instance;
                    }
                }
            }

            ThreadSafeQueueWorker editorQueueWorker = new ThreadSafeQueueWorker();

            EditorThreadDispatcher()
            {
                UnityEditor.EditorApplication.update += Update;
            }

            public void Enqueue(Action<object> action, object state)
            {
                editorQueueWorker.Enqueue(action, state);
            }

            public void UnsafeInvoke(Action action)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            public void UnsafeInvoke<T>(Action<T> action, T state)
            {
                try
                {
                    action(state);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            public void PseudoStartCoroutine(IEnumerator routine)
            {
                editorQueueWorker.Enqueue(_ => ConsumeEnumerator(routine), null);
            }

            void Update()
            {
                editorQueueWorker.ExecuteAll(x => Debug.LogException(x));
            }

            void ConsumeEnumerator(IEnumerator routine)
            {
                if (routine.MoveNext())
                {
                    var current = routine.Current;
                    if (current == null)
                    {
                        goto ENQUEUE;
                    }

                    var type = current.GetType();
#if UNITY_2018_3_OR_NEWER
#pragma warning disable CS0618
#endif
                    if (type == typeof(WWW))
                    {
                        var www = (WWW)current;
                        editorQueueWorker.Enqueue(_ => ConsumeEnumerator(UnwrapWaitWWW(www, routine)), null);
                        return;
                    }
#if UNITY_2018_3_OR_NEWER
#pragma warning restore CS0618
#endif
                    else if (type == typeof(AsyncOperation))
                    {
                        var asyncOperation = (AsyncOperation)current;
                        editorQueueWorker.Enqueue(_ => ConsumeEnumerator(UnwrapWaitAsyncOperation(asyncOperation, routine)), null);
                        return;
                    }
                    else if (type == typeof(WaitForSeconds))
                    {
                        var waitForSeconds = (WaitForSeconds)current;
                        var accessor = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
                        var second = (float)accessor.GetValue(waitForSeconds);
                        editorQueueWorker.Enqueue(_ => ConsumeEnumerator(UnwrapWaitForSeconds(second, routine)), null);
                        return;
                    }
                    else if (type == typeof(Coroutine))
                    {
                        Debug.Log("Can't wait coroutine on UnityEditor");
                        goto ENQUEUE;
                    }
#if SupportCustomYieldInstruction
                    else if (current is IEnumerator)
                    {
                        var enumerator = (IEnumerator)current;
                        editorQueueWorker.Enqueue(_ => ConsumeEnumerator(UnwrapEnumerator(enumerator, routine)), null);
                        return;
                    }
#endif

                ENQUEUE:
                    editorQueueWorker.Enqueue(_ => ConsumeEnumerator(routine), null); // next update
                }
            }

#if UNITY_2018_3_OR_NEWER
#pragma warning disable CS0618
#endif
            IEnumerator UnwrapWaitWWW(WWW www, IEnumerator continuation)
            {
                while (!www.isDone)
                {
                    yield return null;
                }
                ConsumeEnumerator(continuation);
            }
#if UNITY_2018_3_OR_NEWER
#pragma warning restore CS0618
#endif

            IEnumerator UnwrapWaitAsyncOperation(AsyncOperation asyncOperation, IEnumerator continuation)
            {
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
                ConsumeEnumerator(continuation);
            }

            IEnumerator UnwrapWaitForSeconds(float second, IEnumerator continuation)
            {
                var startTime = DateTimeOffset.UtcNow;
                while (true)
                {
                    yield return null;

                    var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
                    if (elapsed >= second)
                    {
                        break;
                    }
                };
                ConsumeEnumerator(continuation);
            }

            IEnumerator UnwrapEnumerator(IEnumerator enumerator, IEnumerator continuation)
            {
                while (enumerator.MoveNext())
                {
                    yield return null;
                }
                ConsumeEnumerator(continuation);
            }
        }
#endif
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */