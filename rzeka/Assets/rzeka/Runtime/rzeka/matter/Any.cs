/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    public interface ISpellMemorable
    {
        public Type MatterType { get; }
    }

    public class Strain<T> : ISpellMemorable
        where T : TMatter
    {
        public Type MatterType => typeof(T);
        public Dictionary<IObservable<T>, IDisposable> StrandSubscriptions;
        // public IObservable<T> Observable => strainSubject.AsObservable();
        public IObservable<T> Observable => _strainSubject.AsObservable();


        readonly ISubject<T> _strainSubject;
        bool IsActive
        {
            get;
            set; // TODO possibly add Eris notifications
        }

        public Strain()
        {
            StrandSubscriptions = new();
            
            // TODO At the moment (probably a long one) only such subject is allowed
            // A custom buffer size subject could be made depending on type attributes for example
            // Also a type that doesnt store any value and sompletely fades away on completion
            _strainSubject = new ReplaySubject<T>(1);
        }

        public IDisposable RegisterConjurer(IObservable<T> strand)
        {
            AddStrand(strand);
            return Disposable.Create(() => RemoveStrand(strand));
        }

        void AddStrand(IObservable<T> strand)
        {
            IDisposable subscription = strand.Subscribe(_strainSubject);
            
            StrandSubscriptions.Add(strand, subscription);

            if (IsActive is false) IsActive = true;
        }

        void RemoveStrand(IObservable<T> strand)
        {
            StrandSubscriptions.Remove(strand);

            if (StrandSubscriptions.Count == 0) IsActive = false;
        }
    }
    
    public class Library
    {
        public Dictionary<Type, ISpellMemorable> Subjects { get; private set; }

        public Library()
        {
            Subjects = new();
        }
        
        // TODO could return a token aswell
        public void GetStrain<T>(out IObservable<T> strain)
        {
            
        }

        public IDisposable RegisterConjurer<T>(IObservable<T> strand)
            where T : TMatter
        {
            Type key = typeof(T);

            Strain<T> strain;
            if (Subjects.ContainsKey(key) is false)
            {
                strain = new();
                Subjects[key] = strain;
            }
            else
            {
                strain = Subjects[key] as Strain<T>;
            }

            Debug.Assert(strain != null, nameof(strain) + " != null");

            return strain.RegisterConjurer(strand);
        }

        public IDisposable RegisterConjurer<T>(IObservable<T> strand, out IObservable<T> observableStrain)
            where T : TMatter
        {
            Type key = typeof(T);

            Strain<T> strain;
            if (Subjects.ContainsKey(key) is false)
            {
                strain = new();
                Subjects[key] = strain;
            }
            else
            {
                strain = Subjects[key] as Strain<T>;
            }

            Debug.Assert(strain != null, nameof(strain) + " != null");

            observableStrain = strain.Observable;
            
            return strain.RegisterConjurer(strand);
        }
    }
    
    public abstract class Any : IEnumerable<Type>
    {
        protected abstract IEnumerable<Type> Types { get; }

        public IEnumerator<Type> GetEnumerator()
        {
            return Types.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Types.GetEnumerator();
        }
    }

    public class Any<TOne, TTwo> : Any
        where TOne : TMatter
        where TTwo : TMatter
    {
        readonly Type[] _types = { typeof(TOne), typeof(TTwo) };

        protected override IEnumerable<Type> Types => _types;
    }

}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 24 January 2023 🌊 */