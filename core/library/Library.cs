using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    // TODO wouldn't it be a good idea to push entire library and eris as internal classes in the end?
    public class Library : IDisposable
    {
        Eris Eris { get; }
        Dictionary<Type, ISpellStream> Streams { get; }
        readonly IDisposable _conjurerAvailabilitySubscription;

        internal int StreamsCount => Streams.Count;

        public IObservable<IManaInformationProvideable> ConjurerAvailability { get; }

        public Library(Eris eris)
        {
            Eris = eris;
            Streams = new();

            IConnectableObservable<IManaInformationProvideable> availability =
                BuildConjurerAvailability();
            _conjurerAvailabilitySubscription = availability.Connect();
            ConjurerAvailability = availability;
        }

        IConnectableObservable<IManaInformationProvideable> BuildConjurerAvailability() =>
            Eris.SpellOccurences
                .Where(occ =>
                    occ.Source.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding or SpellSchool.Shuttling
                )
                .Where(occ =>
                    occ.SpellOccurenceCategory
                        is SpellOccurenceCategory.HasMana
                            or SpellOccurenceCategory.NoMana
                            or SpellOccurenceCategory.Forgotten
                )
                .Scan(
                    (false, new AvailableConjurers()),
                    (acc, current) =>
                    {
                        TStrandingSpell sourceAsStranding =
                            current.Source as TStrandingSpell
                            ?? throw new InvalidOperationException();

                        AvailableConjurers accumulator = acc.Item2;
                        Type conjuredType = sourceAsStranding.ConjuredType;
                        bool wasManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);

                        if (current.SpellOccurenceCategory is SpellOccurenceCategory.HasMana)
                            accumulator.ActivateConjurer(sourceAsStranding);
                        else
                            accumulator.DectivateConjurer(sourceAsStranding);

                        bool isManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);
                        bool hasAnythingChanged = wasManaAvailable != isManaAvailable;

                        return (hasAnythingChanged, accumulator);
                    }
                )
                .Where(accumulator => accumulator.Item1)
                .Select(accumulator => accumulator.Item2 as IManaInformationProvideable)
                .StartWith(new AvailableConjurers())
                .Multicast(new ReplaySubject<IManaInformationProvideable>(1));

        public void Dispose()
        {
            _conjurerAvailabilitySubscription.Dispose();
        }

        public bool WasStreamCreated<T>() where T : TMatter
        {
            Type key = typeof(T);
            return WasStreamCreated(key);
        }

        public bool WasStreamCreated(Type key)
        {
            return Streams.ContainsKey(key);
        }
        
        public IDisposable RegisterConjurer<T>(IObservable<T> strand)
            where T : TMatter
        {
            if (strand == null) throw new ArgumentNullException(nameof(strand));
            
            Stream<T> stream = GetStream<T>();

            Debug.Assert(stream != null, nameof(stream) + " != null");
            
            IDisposable registrationToken = stream.RegisterConjurer(strand);

            return registrationToken;
        }

        public IObservable<T> GetConjurer<T>() where T : TMatter
        {
            Stream<T> stream = GetStream<T>();
            IObservable<T> conjurer = stream.GetStreamAsObservable();
            return conjurer;
        }
        
        /// <summary>
        /// TODO IMPORTANT CONSIDERATION
        /// At the moment all created streams stay for the duration of application running
        /// They are never un-created only become inactive when they lack sources.
        /// They are also created BOTH by a conjurer or a stream request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Stream<T> GetStream<T>() where T : TMatter
        {
            Type key = typeof(T);
            
            Stream<T> stream;
            
            if (Streams.ContainsKey(key) is false)
            {
                stream = new();
                Streams.Add(key, stream);
            }
            else
            {
                stream = Streams[key] as Stream<T>;
            }
            
            return stream;
        }
    }
}
