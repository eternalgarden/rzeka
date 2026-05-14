# rzeka

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/EternalGarden.Rzeka?logo=nuget)](https://www.nuget.org/packages/EternalGarden.Rzeka)

**A reactive event bus for C# that tracks causality.**

rzeka ("_river_" in Polish) is a single-threaded event bus built on Rx.NET. Components publish typed events into the river and react to events flowing through it, without holding references to each other.

What makes rzeka different from a typical event bus or pub/sub library is **causality tracking**: every event automatically carries a record of the events that caused it. You can ask any event "_what caused you?_" and get the full chain back. Combined with Eris, rzeka's built-in debugger, this means you can step through the entire causal chain of anything that happens in your system - live, in a browser, while the game runs.

rzeka is single-threaded by design. This is the constraint that makes everything else possible: it guarantees that circumstance tracking, mana transitions, and spell lifecycle are always consistent. Async operations are handled within defined boundaries - see [Async Operations](addlink).

**Status**: rzeka was originally built for [sanctuary](https://github.com/eternalgarden/sanctuary), a 3D journaling application shipped on Unity. It is currently being refactored alongside sanctuary's port to Godot. **The core API is stable**. The Godot integration and Eris UI are actively evolving.

## 🪞 Grimoire

rzeka uses river, textile and magic themed naming system:
- events are **Matter**
- transformations are **Spells**
- a Spell's required Matter types fulfillment status is called **Mana**
- Spell-defining methods (_Strand, Loom, Shuttle, Weave, Pluck_) follow textile-production vocabulary, inspired by [Zeros and Ones by Sadie Plant](https://www.goodreads.com/en/book/show/927879.Zeros_and_Ones))

**Eris**, the debugger, borrows her name from the Greek goddess of discord.

The metaphor is consistent and once you let it work its magic, the API becomes self-describing.

## 💾 Installation

Rzeka targets `net8.0` and depends only on `System.Reactive`.

```python
dotnet add package EternalGarden.Rzeka
```

### Godot 4

Godot does not resolve transitive NuGet dependencies, so add System.Reactive explicitly:

```python
dotnet add package System.Reactive
```

## 🌱 Getting Started

Create a single river at startup and share its IRzeka reference with the systems that need it:

```csharp
IRzeka rzeka = new Spring().Create("Nile");
```

> 📜💎 You should have **one rzeka** per application - v1 is designed around single instance. Running multiple instances will break [circumstance chain tracking](#Circumstances).

Multi-rzeka topologies are considered for a v2 version. 

The name passed to Create serves currently a purely mythical role, it has no direct usage in rzeka codebase, but one should have the capacity to name the river that they live along.

### Hosting rzeka in Godot

The simplest pattern is a [Autoload](https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html) that owns the river instance and a main-thread scheduler:
```csharp
using Godot;
using Rzeka;
using System.Reactive.Concurrency;
using System.Threading;

public partial class River : Node
{
    public static IRzeka Rzeka { get; private set; }
    public static IScheduler MainThread { get; private set; }

    public override void _Ready()
    {
        SynchronizationContext.SetSynchronizationContext(new GodotMainThreadContext());
        MainThread = new SynchronizationContextScheduler(SynchronizationContext.Current);

        Rzeka = new Spring().Create("MyGame");
    }

    // Posts callbacks to Godot's main thread via CallDeferred - backs the MainThread scheduler above.
    private sealed class GodotMainThreadContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state) =>
            Callable.From(() => d(state)).CallDeferred();
    }
}
```

Nodes access the river via `River.Rzeka` and the main-thread scheduler via `River.MainThread`. In larger projects you may want to wire both through a DI container.

All Godot lifecycle callbacks (`_Ready`, `_Process`, `_PhysicsProcess`, etc.) run on the main thread, so Strand, Pluck, Loom, and Weave all work without any extra setup. 

Async operations that return on a background thread need manual circumstance handling *and* must marshal back through `River.MainThread` before their results re-enter the river - see [Async Operations](#async-operations).

For the live debugger during development, see [Eris](#eris).

Once you summoned your rzeka, you are ready to shape Matter.

## 🪽 Matter - Events

> 📜 Matter is the base carrier of event data. Every matter has a `Guid` (its unique identity) and a list of `Circumstances` (a collection of matter that led to the emission of this one). 

Guid and Circumstances attachment is handled by rzeka automatically so it can be later observed through the provided [Eris debugger tool](#Eris).

Extend `Matter` to define your own event types:

```csharp
class PlayerDied : Matter
{
    public string Cause { get; }
    public PlayerDied(string cause) => Cause = cause;
}

class EnemyDefeated : Matter
{
    public int XpReward { get; }
    public EnemyDefeated(int xpReward) => XpReward = xpReward;
}
```

> 📜🧨 Keep your Matter instances **immutable** - avoid mutable reference type fields. Eris holds references to emitted matter for the causal graph. Mutating after emission corrupts that history and can produce subtle bugs when multiple subscribers hold the same instance.

### Circumstances

> 📜 Circumstances describe context of a given matter emission.

For example, a `DamageDealt` event could carry the `AttackEvent` that triggered it as a circumstance.

This allows you to track the causality chains through [Eris, rzeka's debugger](#Eris) or to check in your game logic if a given matter is caused by another (`damage_dealt.IsCircumstancedBy(wizard_fireball_attack)`). 

```csharp
var attack = new AttackEvent(attacker: "dragon");
var damage = new DamageDealt(amount: 40).WithCircumstances<DamageDealt>(attack);

// Later, check if a piece of matter is causally linked to something specific
bool causedByDragon = damage.IsCircumstancedBy(attack); // true
```

#### Automatic vs. Manual Circumstances

> 📜 Rzeka tracks circumstance attachment automatically inside **Loom** - when your spell produces output matter, the input matter that triggered it is attached as a circumstance. You don't need to do anything for this to work.

If your output matter already has circumstances attached (via `.WithCircumstances()`), Rzeka will leave them alone and skip the automatic tracking. This means `.WithCircumstances()` inside a Loom lambda is an **active decision to override the default tracking**.

**Where automatic tracking works:**
- Synchronous Loom chains - the default, no action needed
- `Shuttle` responses - if the responder does not stamp manually, Shuttle attaches the triggering request as the circumstance for you. If the responder *does* stamp manually (e.g. to thread Scry'd context matter), Shuttle leaves your stamp alone - see the [Shuttle stamping rule](#shuttle---requestresponse).

**Where you must attach circumstances manually:**
- Inside `Pluck` and `Ask` calls - pre-stamp the matter via `.WithCircumstances<T>(trigger)` when you have the triggering matter in scope
- Inside async boundaries within Loom (see [Async Operations](#async-operations) below)

**Where circumstances are not touched:**
- `Strand` - it is used for root matter emissions (eg. caused by user input)

## 🧬 API

> 📜 Communciation through rzeka is carried through a set of specialised API methods.

All API methods accept a `who` object (the registering owner, used for diagnostics) and return `IDisposable` to unregister. Observables and lambda functions you pass into them are called *spells*.

A common pattern is to collect them into rzeka's `CollectibleDisposable`:

```csharp
CollectibleDisposable Q = new();

Q += rzeka.Loom<PlayerInputState, PlayerMovementState>(...)

// on destroy / cleanup:
Q.Dispose();
```

`CollectibleDisposable` is a wrapper around [CompositeDisposable][https://learn.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh228980(v=vs.103)], it overloads `+` operator allowing you to neatly add your rzeka subscriptions to it. It does not implement `.Clear()` method of the `CompositeDisposable` because that would likely lead you to accidental memory leaks.

Loom and Weave API methods below have overloads that allow two input matter types. If you need more, pull additional streams manually via `Scry`:

```csharp
Q += rzeka.Loom<A, B, Out>(
    this,
    (a, b) =>
        a.WithLatestFrom(b, rzeka.Scry<C>())
         .Select((aVal, bVal, cVal) => new Out(...))
);
```
---

### 🧬 Strand - publisher

> 📜 Registers a source `IObservable<T>` into the river.

Any Loom or Weave that listens to `T` will receive these values.

```csharp
// Expose a button's click stream as PlayerJumpRequested events
Q += rzeka.Strand(
    this,
    jumpButton.OnClickAsObservable().Select(_ => new PlayerJumpRequested())
);

// Expose a timer tick as a recurring event
Q += rzeka.Strand(
    this,
    Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(_ => new GameClockTick()) //TODO is interval running on the main thread?
);
```

---

### 🧬 Pluck - fire once publisher

> 📜 Publish a single matter value into rzeka imperatively, without an ongoing stream.

Pluck is fully visible to Eris - it appears as a `Plucking` spell with a `Created → Shaped → Forgotten` lifecycle attributed to the `who` you pass in.

```csharp
rzeka.Pluck(this, new GameStarted());
```

Pluck has no automatic upstream tracking - it does not know what caused it. When you *do* know (inside a Loom or Weave where the triggering matter is in scope), pre-stamp the matter via `.WithCircumstances<T>()`:

```csharp
// Inside a Weave or other context where you have the triggering matter:
rzeka.Pluck(this, new GamePaused().WithCircumstances(triggeringEvent));
```

---

### 🧬 Loom - transform

> 📜 Listens to one or more streams and produces a new stream. 

Use it for mapping, combining, or reacting to matter. Intentional side effects belong inside `.Reacting()` (see [Reacting](#reacting)), not bare `.Do()` or `.Select(...)` with imperative bodies.

Loom automatically attaches the triggering input matter as a circumstance on the output. You do not need to call `.WithCircumstances()` manually - and if you do, it will override the automatic tracking (see [Automatic vs Manual Circumstances](#automatic-vs-manual-circumstances)).

**Single input:**
```csharp
// Transform health change events into UI update events
Q += rzeka.Loom<HealthChanged, HealthBarUpdateRequested>(
    this,
    health => health.Select(e => new HealthBarUpdateRequested(e.NewValue, e.MaxValue))
);
```

**Two inputs - combine latest:**
```csharp
//TODO can we simplify this example, explicit types in select make it less readable i think
// Produce a rendering update whenever position OR animation state changes
Q += rzeka.Loom<PositionChanged, AnimationStateChanged, RenderUpdateRequested>(
    this,
    (positions, animations) => positions.CombineWith(animations) // or .WithLatestFrom(...), see extension methods
        .Select(((PositionChanged pos, AnimationStateChanged anim) pair) =>
            new RenderUpdateRequested(pair.pos.Position, pair.anim.Clip))
);
```
**Three inputs:**
```csharp
Q += rzeka.Loom<InputEvent, PhysicsState, GameState, MovementCommand>(
    this,
    (inputs, physics, game) => inputs
        .CombineLatest(physics, game)
        .Select(/* ... */)
);
```
For neither Loom or Weave overloads with more than three input-matter types

---

### 🧬 Weave - subscriber

> 📜 Final subscriber - consumes streams and produces nothing. 

Use for final effects: rendering, audio, persistence, etc. Other publishing/transforming rzeka methods will already work on their own even if the final `.Weave()` subscriber is not active yet.

**Single stream:**
```csharp
Q += rzeka.Weave<PlayerDied>(
    this,
    deaths => deaths.Subscribe(e => ShowDeathScreen(e.Cause))
);
```

**Two streams - react when both have values:**
```csharp
Q += rzeka.Weave<EquipmentChanged, PlayerStats>(
    this,
    (equipment, stats) => equipment.WithContext(stats) //todo this is still using old withcontext, replace
        .Subscribe(((EquipmentChanged eq, PlayerStats s) pair) =>
            UpdateEquipmentUI(pair.eq, pair.s))
);
```

**Raw observer:**
```csharp
Q += rzeka.Weave<GameClockTick>(this, _clockDisplayObserver);

TODO: add a potential pseudocode for how such observer is implemented
```

---

### 🧬 Scry - raw observable

> 📜 TODO: Add Scry notes, they were incorrectly desciribng multi-rzeka situation which we suspended for v2.

```csharp
IObservable<PlayerDied> deaths = rzeka.Scry<PlayerDied>();
```

---

### 🧬 Shuttle - Async Request/Response

> 📜 Shuttle is rzeka's pattern for triggering operations and awaiting their outcomes - save/load, network calls, path computation, anything with latency or a success/failure result. Pair it with [`Ask`](#ask) on the caller side.

For sync live state (player health, game settings, etc.), prefer [`[HasState]`](#hasstate) matter types instead.

Shuttle operates on two specialised Matter types - extend them to define your round-trip pair:

```csharp
class SaveGameRequest : Request { }

class SaveGameResponse : Response<SaveGameRequest>
{
    public SaveGameResponse(SaveGameRequest request, bool wasSuccessful)
        // The `: base(request, wasSuccessful)` constructor call is required.
        : base(request, wasSuccessful) { }
}
```

> 📜🧭 The suggested naming convention is to keep both types in the same `.cs` file suffixed with `RR`, like `SaveGameRR`.

**Usage**: Register a handler with `Shuttle`. Because Shuttle's primary use case crosses an async boundary, wrap the operation in `Observable.Create` and stamp the request as a circumstance on the response manually - the same rule as [Async Operations](#async-operations):

```csharp
Q += rzeka.Shuttle<SaveGameRequest, SaveGameResponse>(
    this,
    reqs => reqs.SelectMany(req =>
        Observable.Create<SaveGameResponse>(observer =>
        {
            _saveSystem.SaveAsync(success =>
            {
                // Async boundary - stamp manually while req is still in scope
                observer.OnNext(
                    new SaveGameResponse(req, success)
                        .WithCircumstances<SaveGameResponse>(req));
                observer.OnCompleted();
            });
            return Disposable.Empty;
        })
    )
);
```

---

#### Ask - Request Side

> 📜 Send a request into the river and receive an observable that emits only the response to *your specific* request - not responses to other concurrent requests of the same type.

When Ask(ing) you also have to manually stamp the request matter circumstances with `.WithCircumstances(...circumstances)`.

```csharp
// Inside a Loom: on level completion, save then show results
Q += rzeka.Loom<LevelCompletedEvent, ResultsScreenRequest>(
    this,
    levelCompletedEvent => levelCompletedEvent.SelectMany(evt =>
        rzeka.Ask<SaveGameRequest, SaveGameResponse>(
                this, 
                new SaveGameRequest().WithCircumstances(evt))
             .Select(save => new ResultsScreenRequest(evt.Score, save.WasSuccessful)))
);
```

> 📜🧨 **Avoid nesting Asks.** Multi-step chains written as nested `SelectMany` / `Zip` inside a single Loom are hard to read and fight rzeka's model. Decompose them into separate Looms and Shuttles instead - each step stays readable, owns one responsibility, and causality flows automatically through the river.

---

#### Multi-context Response using Scry

> 📜 When the response depends on more than just the request, pull the additional matter in via `Scry` inside the lambda.

```csharp
Q += rzeka.Shuttle<LoadSceneRequest, LoadSceneResponse>(
    this,
    // The request remains the trigger, Scry'd matter is read as ambient context
    reqs => reqs.WithLatestFrom(rzeka.Scry<GameState>(), rzeka.Scry<Settings>())
        .Select(ctx =>
        {
            var (req, state, settings) = ctx;
            // Stamp manually so the response carries [request, state, settings]
            // as circumstances.
            return new LoadSceneResponse(req, wasSuccessful: true)
                .WithCircumstances<LoadSceneResponse>(req, state, settings);
        })
);
```

**Stamping rule for Shuttle responses.** If you stamp circumstances manually on the response, **include the request yourself**. If you do *not* stamp anything manually, Shuttle automatically stamps `[request]` for you.
- The manual route is the only way to also record the additional context that shaped the response. 
- Forgetting the request in a manual stamp does not break `Ask` request/response casuality (but it does orphan the response from its triggering request in Eris matter graph view)

> 📜🧨 Do not use `.Do()` or `.Reacting()` for internal state mutations inside a Shuttle - this can lead to race conditions since the response stream is shared among multiple potential requesting agents.

## 🏹 Eris

> 📜 Eris is rzeka's internal debugger realm. It records every spell lifecycle event (created, has mana, no mana, forgotten), every matter emission (shaped, received) along with their Circumstances, and every message/exception - all timestamped and serialized.

Eris runs in core and is always active, even in release builds, so that diagnostic data is available for crash dumps ([WHDIG files](localing.com)).

### Live Debugger

Rzeka ships with a browser-based debugger (not included in builds) that connects to your game over WebSocket. It shows matter flow, messages and live spell status (TODO, not impelemnted yet.) in real time - no in-game UI needed.

**Setup:**

1. Add a reference to `Rzeka.Dev` in your game project (dev builds only) TODO: show people with no big experience with net how to do it in their csproj 
2. In your game initialization:

```csharp
var spring = new Spring(); // TODO hold on, current Spring class does not have enabledevserver method
spring.EnableDevServer(); // starts WebSocket on ws://127.0.0.1:9222

IRzeka rzeka = spring.Create("MyGame");
```

3. Start the Eris UI:

```csharp
cd rzeka/ui
npm install   # first time only
npm run dev
```

4. Open `http://localhost:5173` in any browser

The debugger auto-connects to the game's WebSocket server. If the game isn't running, it reconnects automatically every 3 seconds. All Rzeka instances created through the factory are picked up automatically.

The `Rzeka.Dev` package is the only part that adds an external dependency (Fleck for WebSocket). Core remains dependency-free beyond `System.Reactive`. Remove the `Rzeka.Dev` reference for release builds - Eris continues recording internally, the WebSocket server simply isn't started.

### Running the Demo

A demo test is included that spins up Rzeka with sample spells and emits matter on timers, useful for developing the UI or verifying the debug pipeline:

Terminal 1 - start the UI
- `cd rzeka/ui`
- `npm run dev`

Terminal 2 - run the demo (30 seconds)
- `cd rzeka/tests`
- `dotnet test --filter "FullyQualifiedName~DebugServerDemo" -- xUnit.MaxParallelThreads=1`

### Lifecycle & Mana

todo: add mana section

### Whisper

`IRzeka` exposes a `Whisper` method for emitting structured log messages into Eris from your game code. All whispers appear in the Eris UI alongside matter flow and spell lifecycle events.

Three severity levels are available via `RzekaMessageType`:
- `Hint` - informational
- `Hunch` - warning
- `Horror` - error

```csharp
// Simple info log (defaults to Hint)
rzeka.Whisper("Player respawned");

// With explicit severity
rzeka.Whisper("Save slot full", RzekaMessageType.Hunch);

// Exception - automatically Horror severity
rzeka.Whisper(exception);

// Exception with a message
rzeka.Whisper("Failed to load scene", exception);
```

You can optionally attach `IMatter` instances as circumstances, linking the message to the causal graph:

```csharp
rzeka.Whisper("Invalid state reached", RzekaMessageType.Horror, triggeringMatter);
```

Rzeka itself uses `Whisper` internally for thread violations and stream errors, so they appear in Eris alongside your own messages.

---

## 🪧 Attributes

### HasState

A very important rzeka attribute that you will be using for your matter types that are supposed to hold on to the last emission of this matter type and immediately provide it to its new subscribers.

The suggested naming convention for stateful matter types is adding *State* suffix to them.

```csharp
[HasState]
class PlayerInputState : Matter
{
    public InputState InputState { get; }
    public PlayerInputState(InputState state) => InputState = state;
}
```

#### Evolving State

> 📜 State evolves via a Loom that reads the current state, listens to an event that influences it, and emits the new state back into the same stream.

The signature is a reducer: `(state, event) → state`.

```csharp
Q += rzeka.Loom<PlayerHealthState, DamageReceived, PlayerHealthState>(
    this,
    (health, damage) => damage.WithLatestFrom(health)
        .Select(((DamageReceived d, PlayerHealthState s) pair) =>
            new PlayerHealthState(pair.s.Hp - pair.d.Amount))
);
```

> 📜🧨 **Event triggers state - not the other way around.** Use `WithLatestFrom` with the event as the source, not `CombineLatest`. With `CombineLatest`, every new state emission re-fires the Loom against the latest event and emits state again - an infinite self-feedback loop. Rzeka's stream overheat detector catches it at runtime, but treat that as a smoke alarm signalling a wrong combinator choice, not as a design.

> 📜🧭 **State matter is single-writer.** Rzeka enforces this at registration: attempting to register a second active writer for a `[HasState]` type throws `InvalidOperationException`. Pluck can still seed an initial value before a long-lived writer exists (its registration is disposed synchronously), but Pluck against a `[HasState]` type while a Loom or Strand already owns it will also throw. Dispose the existing writer first if you need to hand ownership over.

## 🧼 Extension Methods

### Reacting

`.Reacting()` is the explicit operator for intentional side effects inside Loom chains. It reads as what it is - "react to this matter" - rather than abusing Rx's `.Do()` (a debug tap) or hiding logic inside `.Select()`.

Two overloads:

- `.Reacting(Action<T>)` - run a side effect, pass matter through unchanged. Sugar over `.Do()`.
- `.Reacting(Func<T, TOut>)` - run a side effect and produce output matter in one step. Sugar over `.Select()`.

```csharp
// Side effect only - matter flows on unchanged
Q += rzeka.Loom<EnemyDefeated, XpGranted>(
    this,
    events => events
        .Reacting(e => _combatLog.Record(e))
        .Select(e => new XpGranted(e.XpReward))
);

// Side effect + transform in one step
Q += rzeka.Loom<DamageReceived, ScreenShakeRequested>(
    this,
    damage => damage
        .Where(dmg => dmg.Amount > 15f)
        .Reacting(e =>
        {
            _camera.Shake(e.Amount);
            return new ScreenShakeRequested(e.Amount);
        })
);
```
### WithLatestFrom / CombineLatest

Tuple-returning convenience overloads defined in `ScrollExtensions`, named after the standard Rx operators they wrap. Unlike the standard Rx versions, these take no result selector and always return a flat tuple - so if you see `.CombineLatest(other)` returning `(T1, T2)`, it's these overloads, not the standard Rx ones.

```csharp
// WithLatestFrom: source is the trigger, other is "the current value of X"
// Silently drops source emissions that arrive before other has ever emitted.
source.WithLatestFrom(other) // → IObservable<(T1, T2)>
source.WithLatestFrom(ctx1, ctx2) // → IObservable<(T1, T2, T3)>

// CombineLatest: emits whenever either stream fires, always pairing with the latest of the other
streamA.CombineLatest(streamB) // → IObservable<(T1, T2)>
streamA.CombineLatest(streamB, streamC) // → IObservable<(T1, T2, T3)>
```

### IsRespondingTo

Check whether a response corresponds to a specific request - `Ask` uses this internally to filter the response stream. Use it directly when you need a different lifecycle than Ask provides (e.g. observing every response to a long-lived request, or correlating inside a Weave you already own):

```csharp
var myRequest = new InventoryRequest();
Q += rzeka.Weave<InventoryResponse>(
    this,
    responses => responses
        .Where(r => r.IsRespondingTo(myRequest))
        .Subscribe(r => ShowInventory(r.Items))
);
rzeka.Pluck(this, myRequest);
```

Prefer `Ask` for the standard request/response round-trip - it bundles the Weave + Pluck pair and the `IsRespondingTo` filter for you.

---

## 🛟 Error Boundary

> 📜 Every publishing spell - Strand, Pluck, Loom, Shuttle - registers your observable behind a per-spell error boundary. `OnError` that reaches rzeka without being handled upstream is caught, whispered to Eris as a Horror message with the spell's title and owner, and the conjurer completes cleanly. The river survives; other sources keep flowing.

User-side error handling runs first - `.Catch`, `.Retry`, `.OnErrorResumeNext` work as usual. The boundary only sees errors that fall through them:

```csharp
// User handles upstream - boundary never sees the error
Q += rzeka.Strand(
    this,
    flakySource
        .Retry(3)
        .Catch<Reading, TimeoutException>(_ => Observable.Empty<Reading>())
);

// User doesn't handle - boundary catches, whispers to Eris with attribution,
// the river keeps running
Q += rzeka.Strand(this, flakySource);
```

### Crash-on-error (opt-in)

The boundary makes errors *visible* in Eris, not fatal. If you want unhandled source errors to crash the process - typical for dev builds - supply a callback when you create the river:

```csharp
IRzeka rzeka = new Spring().Create(
    "Nile",
    onUnhandledSourceError: (spell, ex) => throw ex
);
```

The callback receives the spell that produced the error (`spell.Title`, `spell.Who`, `spell.SpellSchool`, `spell.Guid`) and the exception. You can filter - e.g. crash only on `Shuttling` spells, log others. Throwing from the callback propagates the exception as `OnError` back into the river's source observer, which rethrows on the source thread.

The whisper to Eris always runs first, the callback is *additional* behavior, not a replacement.

The boundary applies only to publishing spells. `Weave` is a final terminal subscriber - if your subscribe function throws, that's your own `try`/`catch` responsibility.

---

## 🖇️ Async Operations

Rzeka is single-threaded, but your game will have async operations - resource loading, network calls, save/load, etc. The pattern for handling these is:

1. Wrap the async API in `Observable.Create` to bring it into Rx
2. Inside that wrapper, **manually attach circumstances** with `.WithCircumstances()` while you still have the triggering matter in scope
3. **Return to the main thread** with `.ObserveOn(mainThreadScheduler)` before the result re-enters the rzeka chain

Rzeka's automatic circumstance tracking cannot follow across an async boundary. Inside `Observable.Create`, the async callback fires later - potentially after other emissions have passed through the chain. By attaching circumstances manually at the point where you still hold the original trigger, you preserve the causal link.

The same boundary also breaks rzeka's single-thread requirement. The async callback fires on a background thread, so emitting from inside it sends downstream spells off-thread and trips Eris's off-thread guard. `.ObserveOn(scheduler)` shifts emissions back to the main thread before they re-enter the river - see [Hosting rzeka in Godot](#hosting-rzeka-in-godot) for where the scheduler comes from.

```csharp
// Wrap an async engine operation as an Observable
internal static class ResourceLoaderObservable
{
    public static IObservable<LoadSceneResponse> ObservableAsyncSceneLoad(
        this IObservable<LoadSceneRequest> source,
        SceneArchive sceneArchive,
        IScheduler mainThread)
    {
        return Observable.Create<LoadSceneResponse>(observer =>
        {
            return source.Subscribe(request =>
            {
                SceneDescriptor descriptor = sceneArchive
                    .GetSceneDescriptor(request.SceneType, request.SceneVersion);

                // Async boundary - this callback fires later, on whatever
                // thread the engine's loader returns on
                BeginAsyncSceneLoad(descriptor, handle =>
                {
                    LoadSceneResponse response = handle.Succeeded
                        ? new LoadSceneResponse(request, handle.Result)
                        : new LoadSceneResponse(request);

                    // Manually attach circumstances - we still have `request` in scope
                    response = response.WithCircumstances<LoadSceneResponse>(request);

                    observer.OnNext(response);
                });
            });
        })
        // Shift emissions back to the main thread before re-entering the river
        .ObserveOn(mainThread);
    }
}
```

Then use it inside a Loom normally, passing in the main-thread scheduler your hosting layer provides:

```csharp
Q += rzeka.Loom<LoadSceneRequest, LoadSceneResponse>(
    this,
    requests => requests.ObservableAsyncSceneLoad(sceneArchive, River.MainThread)
);
```

Because the output matter already has circumstances attached, Loom's automatic tracking steps aside and preserves them.

> 📜🧨 **Important:** Only use this pattern when crossing a genuine async boundary. For synchronous Loom chains, let Rzeka handle circumstances automatically - don't attach them manually "just in case", as that silently disables the automatic tracking for that emission.
