namespace Rzeka;
  public interface ILoomingSpell<TOut> : IBindingSpell, IStrandingSpell<TOut> 
    where TOut : IMatter { }
