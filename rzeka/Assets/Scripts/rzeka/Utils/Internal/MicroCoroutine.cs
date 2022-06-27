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
using System.Collections.Generic;

namespace Rzeka.Utils
{
    /* 🌊 ---- ---- */

    /// <summary>
    /// Simple supports(only yield return null) lightweight, threadsafe coroutine dispatcher.
    /// </summary>
    internal class MicroCoroutine
    {
        const int INITIAL_SIZE = 16;

        readonly object _runningAndQueueLock = new object();
        readonly object _arrayLock = new object();
        readonly Action<Exception> _unhandledExceptionCallback;

        int _tail = 0;
        bool _running = false;
        IEnumerator[] _coroutines = new IEnumerator[INITIAL_SIZE];
        Queue<IEnumerator> _waitQueue = new Queue<IEnumerator>();

        public MicroCoroutine(Action<Exception> unhandledExceptionCallback)
        {
            this._unhandledExceptionCallback = unhandledExceptionCallback;
        }

        public void AddCoroutine(IEnumerator enumerator)
        {
            lock (_runningAndQueueLock)
            {
                if (_running)
                {
                    _waitQueue.Enqueue(enumerator);
                    return;
                }
            }

            // worst case at multi threading, wait lock until finish Run() but it is super rarely.
            lock (_arrayLock)
            {
                // Ensure Capacity
                if (_coroutines.Length == _tail)
                {
                    Array.Resize(ref _coroutines, checked(_tail * 2));
                }

                _coroutines[_tail++] = enumerator;
            }
        }

        public void Run()
        {
            lock (_runningAndQueueLock)
            {
                _running = true;
            }

            lock (_arrayLock)
            {
                var j = _tail - 1;

                // eliminate array-bound check for i
                for (int i = 0; i < _coroutines.Length; i++)
                {
                    var coroutine = _coroutines[i];

                    if (coroutine != null)
                    {
                        try
                        {
                            if (!coroutine.MoveNext())
                            {
                                // this will happen when a yield break was called
                                // inside of that routine
                                // we get rid of it's reference then
                                _coroutines[i] = null;
                            }
                            else
                            {
#if UNITY_EDITOR
                                // validation only on Editor.
                                if (coroutine.Current != null)
                                {
                                    UnityEngine.Debug.LogWarning("MicroCoroutine supports only yield return null. return value = " + coroutine.Current);
                                }
#endif

                                continue; // next i 
                            }
                        }
                        catch (Exception ex)
                        {
                            _coroutines[i] = null;
                            try
                            {
                                _unhandledExceptionCallback(ex);
                            }
                            catch { }
                        }
                    }

                    // find null, loop from tail
                    while (i < j)
                    {
                        var fromTail = _coroutines[j];

                        if (fromTail != null)
                        {
                            try
                            {
                                if (!fromTail.MoveNext())
                                {
                                    _coroutines[j] = null;
                                    j--;
                                    continue; // next j
                                }
                                else
                                {
#if UNITY_EDITOR
                                    // validation only on Editor.
                                    if (fromTail.Current != null)
                                    {
                                        UnityEngine.Debug.LogWarning("MicroCoroutine supports only yield return null. return value = " + coroutine.Current);
                                    }
#endif

                                    // swap
                                    _coroutines[i] = fromTail;
                                    _coroutines[j] = null;
                                    j--;

                                    goto NEXT_LOOP; // next i
                                }
                            }
                            catch (Exception ex)
                            {
                                _coroutines[j] = null;
                                j--;
                                try
                                {
                                    _unhandledExceptionCallback(ex);
                                }
                                catch { }
                                continue; // next j
                            }
                        }
                        else
                        {
                            j--;
                        }
                    }

                    _tail = i; // loop end
                    break; // LOOP END

                NEXT_LOOP:
                    continue;
                }


                lock (_runningAndQueueLock)
                {
                    _running = false;
                    while (_waitQueue.Count != 0)
                    {
                        if (_coroutines.Length == _tail)
                        {
                            Array.Resize(ref _coroutines, checked(_tail * 2));
                        }
                        _coroutines[_tail++] = _waitQueue.Dequeue();
                    }
                }
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */