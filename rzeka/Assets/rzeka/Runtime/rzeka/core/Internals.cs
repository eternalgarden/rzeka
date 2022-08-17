using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rzeka
{
    internal class Giver<T> where T : TMatter
    {
        public object Who { get; set; }
        public IObservable<T> Observable { get; set; }
    }
}
