
## Approach considerations
---

### 🌿 Open vs. specialized publishers
Publishing directly through the IRzeka interface vs. publishing through a specialized proxy that is obtained from IRzeka interface.

That is either having following method in IRzeka interface...
```c++
void IRzeka.Pluck<T>(T Thought) where T : ThoughtBase
```
or having it within a ILoom interface returned by IRzeka interface..
```c++
ILoom IRzeka.MakeLoom<T,M>()
    where T : ThoughtBase
    where M : Matter

void ILoom.Pluck<T>(T Thought) where T : ThoughtBase
```
- (+) having it going through the loom makes it possible to observe which 'publishers' are currently live, this however requires an additional object `loom` being returned which contains a disposable to evoke when the publisher goes offline

### 🌿 Looms as user-implemented classes or objects provided by Rzeka

A*. *User-implemented classes*
- user implements ILooming interface
```c++
public interface ILooming<out T> 
    where T : ThoughtBase
{
    IObservable<T> GetObservable();
}
```
- (+) it seems this way a single responsibility principle is encouraged, that 
- (-) it seems kindof counterintuitive towards the vibe of rx programming, heavy vibe of object-oriented paradigm
- (-) since it is a class of it's own, anyone can reference it and create it's instance

B*. *Looms are provided by Rzeka*

```c++
public IRzeka.Loom<T, M> Loom<T, M>(object who)
    where T : Thought<M>, new()
    where M : Matter
{
    ...
}
```

- (+) kindof more functional vibe
- (+) less scripts, less classes

### 🌿 .Plucking() vs. providing custom observables
- Should Rzeka (or someone related) contain a..
```C++
void Pluck<T>(T thought) where T : ThoughtBase
```
- or this could be altogether skipped by the useage of custom observables 
provided to the Loom?

### 🌿 Using Matter as core data carrier and using only generic Thoughts
- Thoughts are sealed classes that are not impelented themselves
- Matter implementations are data carriers
- (+) simplified construction of events
- (+) thoughts can be easily pooled and reused
- (+) thoughts only contain information about context and circumstances
- (?) allow matter to be defined as structs

### 🌿 Using structs instead of classes for data carriers
- (+/-) enforce usage of immutable value types for data carried by the river
- (+) that gives the possibility for time travel
- (-) might be quite limiting in development

## Distant ideas
---

### 🌾 Having separate local-streams that the main river is knowledgeable of
- might be too much, when all you've got is a hammer...