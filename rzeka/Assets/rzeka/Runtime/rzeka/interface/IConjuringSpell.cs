using System;

namespace Rzeka
{
    public interface IConjuringSpell : TSpell
    {
        Type ConjuredType { get; }
    }
}