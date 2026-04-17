namespace Rzeka;
  public interface TLoomingSpell<TOut> : TBindingSpell, TStrandingSpell<TOut> 
    where TOut : TMatter { }
