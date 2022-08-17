using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public class Rzeka : IRzeka
    {
        readonly Subject<TMatter> AllMatterStream = new();
        readonly Dictionary<Type, object> ObservableStreams = new();
            
        public IDisposable Pluck<T>(object who, IObservable<T> observable) where T : TMatter
        {
            T value = default;
            Type key = typeof(T);
            Giver<T> provider = null;

            if (ObservableStreams.ContainsKey(key))
            {
                // TODO throw error or check for merging allowance
            }
            else
            {
                provider = new()
                {
                    Who = who,
                    Observable = observable
                };

                ObservableStreams.Add(key, provider);
            }

            return Disposable.Create(() => ObservableStreams.Remove(key));
        }
        
        public IDisposable Weave<T>(object who, Func<IObservable<T>, IDisposable> takerSpell) where T : TMatter
        {
            T value = default;
            Type key = typeof(T);
            IDisposable weaving = null;

            if (ObservableStreams.ContainsKey(key))
            {
                Giver<T> giver = ObservableStreams[key] as Giver<T>;
                IDisposable spellDisposable = takerSpell.Invoke(giver.Observable);
                weaving = spellDisposable;
            }
            else
            {
                // TODO register as awaiting for a provider
            }

            return weaving;
        }
    }
}