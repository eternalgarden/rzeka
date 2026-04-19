# TODO
- how to deal with whidg generation in the end, i guess rzeka users should be able to modify how the final report is generated, like lol without the ascii art for example 
- dev server instructions, installation instructons
- add at the end relative links to specific scripts, like Matter to IMatter implementation
- implement .Label()
- todo: do we really want to enforce the max 2 matter input in api methods? or we keep the 3 input matter overloads but suggest that this could be a code smell?
- todo: do we in the end keep naming for the two extension methods: CombineLatest and WithLatestFrom since they might be misleading for Rx users.
- todo: we need .Reacting() overloads for situations with more than one input matter type
- todo: since we have class Spring then do we plan on making SpringRiver internal? also update docs on initializing rzeka since it should be done through Spring

# rzeka

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)

**rzeka is a single-threaded reactive event bus.** rzeka ("river" in Polish) is a C# library built on Rx.NET. All data flows as typed streams - components publish into the river and subscribe to it without holding direct references to each other.

Instead of subscribing directly to a shared stream, [Rzeka API](link.com) provides a set of methods that define the means and ends of communication through the river. This allows for the extensive debugging and testing of what happens within the stream, i.e. tracking the entire chains of the event emissions.

All matter must be published and consumed on the same thread. This constraint is enforced at runtime and is by design - it guarantees that circumstance tracking, mana transitions, and spell lifecycle are always consistent without locks or synchronization. The documentation provides an example on how to handle async cases, see [Async Operations]().

It was created in order to be used in the Godot game engine for the [sanctuary](addlink.com), but it can be potentially used in other environments. Unfortunately it won't work in Unity (is that true, check?).

TODO: Suggest that it can be used in cojunction with with [R3](https://github.com/Cysharp/R3). Verify if true.

This version of rzeka was refactored and improved from the old Unity-focused implementation with help of an LLM.

## Installation

_TODO: add Unity Package Manager / Godot installation instructions (fill it be able to use it in unity?)._

TODO: Mention that it works best with DI?

## Events

Events carried through Rzeka are called **Matter**.

### Matter

The base carrier of event data. Every matter has a `Guid` (its identity) and a `Circumstances` list (contextual metadata - other matter instances that provide context for this one). 

Guid and Circumstances attachment is handled by rzeka automatically so it can be later tracked in your integration tests and observed through the provided [Eris debugger tool](#Eris).

Matter instances are compared by `Guid`, so two separate instances of the same type are never equal.

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

Avoid using reference types in your Matter instances.

Keep your matter instances immutable.

### Request / Response

A specialised pair of Matter for request/response patterns. The response carries a reference back to the original request, so the requester can verify the response is for their specific request.

You are required to implement the `.base()` constructor call.

This is what [Shuttle API method](#Shuttle) operates on.

```csharp
class InventoryRequest : Request { }

class InventoryResponse : Response<InventoryRequest>
{
    public IReadOnlyList<Item> Items { get; }
    public InventoryResponse(InventoryRequest request, IReadOnlyList<Item> items, bool wasSuccessful)
        : base(request, wasSuccessful) => Items = items;
}
```

TODO: Example of checking manually if response you receive is specific to your request. Does that even make sense? Shouldnt RR be always used with Shuttle?
```csharp
// claude please help me with the example
```

The suggested filenaming convention (a good place for a snippet) is to keep both Request and its Response in the same `.cs` file suffixed with `RR`, like `InventoryRR`.

### Circumstances

Circumstances attach context to a piece of matter. For example, a `DamageDealt` event could carry the `AttackEvent` that triggered it as a circumstance, allowing you to track the causality chains through [Eris, rzeka's debugger](#Eris)

```csharp
var attack = new AttackEvent(attacker: "dragon");
var damage = new DamageDealt(amount: 40).WithCircumstances<DamageDealt>(attack);

// Later, check if a piece of matter is causally linked to something specific
bool causedByDragon = damage.IsCircumstancedBy(attack); // true
```

##### Automatic vs. Manual Circumstances

Rzeka tracks circumstances automatically inside **Loom** - when your spell produces output matter, the input matter that triggered it is attached as a circumstance. You don't need to do anything for this to work. This relies on the single-threaded guarantee: the causal link between input and output is always unambiguous.

If your output matter already has circumstances attached (via `.WithCircumstances()`), Rzeka will leave them alone and skip the automatic tracking. This means `.WithCircumstances()` inside a Loom lambda is an **active decision to override the default tracking**.

TODO, IMPLEMENT (Eris): Display information in Eris whether matteroccurence has manually assigned circumstances.

**Where automatic tracking works:**
- Synchronous Loom chains — the default, no action needed

**Where you must attach circumstances manually:**
- Inside `Pluck` calls — Pluck has no automatic tracking
- Inside async boundaries within Loom (see [Async Operations](#async-operations) below)
- On matter passed to external systems outside Rzeka (What do you mean by that Calude?)

**Where circumstances are not touched:**
- `Strand` — passes matter through to the Library as-is

---

## API

Initialize Rzeka with:

```csharp
// TODO shouldn't we actually make springriver constructor internal and force the user to initialize it through the factory.

IRzeka rzeka = new SpringRiver("MyGame");

// TODO Add notes on child rivers and consider implementing at least a rzkea factory which would inform Eris about all existing rivers, also potentially implement an 
```

All rzeka API methods accept a `who` object (the registering owner, used for diagnostics) and return `IDisposable` to unregister. Observables and lambda functions you pass into them are called *spells*.

A common pattern is to collect them into a composite disposable:

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

### Strand - pure giver / publisher

Registers a source `IObservable<T>` into the river. Any Loom or Weave that listens to `T` will receive these values.

```csharp
// Expose a button's click stream as PlayerJumpRequested events
Q += rzeka.Strand(
    this,
    jumpButton.OnClickAsObservable().Select(_ => new PlayerJumpRequested())
);

// Expose a timer tick as a recurring event
Q += rzeka.Strand(
    this,
    Observable.Interval(TimeSpan.FromSeconds(1)).Select(_ => new GameClockTick())
);
```

---

### Pluck - fire once publisher

Publish a single matter value into the river imperatively, without an ongoing stream.

```csharp
rzeka.Pluck(this, new GameStarted());
```
Pluck has no automatic circumstance tracking — it doesn't know what caused it. If causality matters, attach circumstances manually with `.WithCircumstances()` before plucking:

```csharp
// Inside a Weave or other context where you have the triggering matter:
rzeka.Pluck(this, new GamePaused().WithCircumstances<GamePaused>(triggeringEvent));
```

---

### Loom - transform

Listens to one or more streams and produces a new stream. Use it for mapping, combining, or reacting to matter. Intentional side effects belong inside `.Reacting()` (see [Reacting](#reacting)), not bare `.Do()` or `.Select(...)` with imperative bodies.

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
// Produce a rendering update whenever position OR animation state changes
Q += rzeka.Loom<PositionChanged, AnimationStateChanged, RenderUpdateRequested>(
    this,
    (positions, animations) =>
        positions
            .CombineWith(animations) // or .WithLatestFrom(...), see extension methods
            .Select(((PositionChanged pos, AnimationStateChanged anim) pair) =>
                new RenderUpdateRequested(pair.pos.Position, pair.anim.Clip))
);
```

// todo: remove if we don't keep threee input api methods
**Three inputs:**
```csharp
Q += rzeka.Loom<InputEvent, PhysicsState, GameState, MovementCommand>(
    this,
    (inputs, physics, game) =>
        inputs
            .WithContext(physics) // todo: this uses the old naming for CombineWith
            .WithContext(game)
            .Select(/* ... */)
);
```

---

### Weave - pure taker / subscriber

Final subscriber - consumes streams and produces nothing. Use for final effects: rendering, audio, persistence, etc. Other publishing/transforming rzeka methods will already work on their own even if the final `.Weave()` subscriber is not active yet (todo: is this worth mentioning?).

**Single stream:**
```csharp
Q += rzeka.Weave<PlayerDied>(
    this,
    deaths => deaths.Subscribe(e => ShowDeathScreen(e.Cause))
);
```

**Two streams — react when both have values:**
```csharp
Q += rzeka.Weave<EquipmentChanged, PlayerStats>(
    this,
    (equipment, stats) =>
        equipment
            .WithContext(stats)
            .Subscribe(((EquipmentChanged eq, PlayerStats s) pair) =>
                UpdateEquipmentUI(pair.eq, pair.s))
);
```

**Raw observer:**
```csharp
Q += rzeka.Weave<GameClockTick>(this, _clockDisplay);
```

---

### Shuttle - Request/Response

Like Loom, but works exclusively with `IRequest` / `IResponse<T>` types. Register a handler that answers incoming requests. Pair it with [`Ask` extension method](#ask) on the requester side.

```c
// Simple sync query
Q += rzeka.Shuttle<PlayerStatsRequest, PlayerStatsResponse>(
    this,
    reqs => reqs.Select(req =>
        new PlayerStatsResponse(req, _player.Health, wasSuccessful: true))
);

// Async: each request triggers an async operation
// TODO: since we mentioned rzeka operates on single-thread, is this a correct example with respec to how we describe dealing with async situations?
Q += rzeka.Shuttle<SaveGameRequest, SaveGameResponse>(
    this,
    reqs => reqs.SelectMany(req =>
        _saveSystem.SaveAsync()
                   .Select(success => new SaveGameResponse(req, success)))
);
```

> **Note:** Do not use `.Do()` or `.Reacting()` for internal state mutations inside a Shuttle - this can lead to race conditions since the response stream is shared among multiple potential requesting agents.

---

### Scry - raw observable

Returns the raw `IObservable<T>` for a matter type, without registering a spell. Primarily used to bridge between multiple Rzeka instances. (todo: add: since it is breaking circumstance tracking? or will we handle the cross-rzeka-instance matter ttracking?)

```csharp
IObservable<PlayerDied> deaths = rzeka.Scry<PlayerDied>();

// Bridge: re-strand events from one river into another
Q += otherRzeka.Strand(this, rzeka.Scry<GameStateChanged>());
```

---

## Eris

Eris is rzeka's internal debugger realm. It records every spell lifecycle event (created, has mana, no mana, forgotten (todo: write notes on spell lifecycle events)), every matter emission (shaped, received), and every message/exception - all timestamped and serialized. This runs in core and is always active, even in release builds, so that diagnostic data is available for crash dumps ([WHDIG files](localing.com)).

### Live Debugger

Rzeka ships with a browser-based debugger that connects to your game over WebSocket. It shows matter flow, messages and live spell status (TODO, not impelemnted yet.) in real time - no in-game UI needed.

**Setup:**

1. Add a reference to `Rzeka.Dev` in your game project (dev builds only) TODO: show people with no big experience with net how to do it in their csproj 
2. In your game initialization:

```csharp
var spring = new Spring();
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

The `Rzeka.Dev` package is the only part that adds an external dependency (Fleck for WebSocket). Core remains dependency-free beyond `System.Reactive`. Remove the `Rzeka.Dev` reference for release builds — Eris continues recording internally, the WebSocket server simply isn't started.

### Running the Demo

A demo test is included that spins up Rzeka with sample spells and emits matter on timers, useful for developing the UI or verifying the debug pipeline:

Terminal 1 - start the UI
- `cd rzeka/ui`
- `npm run dev`

Terminal 2 - run the demo (30 seconds)
- `cd rzeka/tests`
- `dotnet test --filter "FullyQualifiedName~DebugServerDemo" -- xUnit.MaxParallelThreads=1`

<!-- EDIT: add a screenshot of the Eris UI showing the demo output -->

TODO: at the very end check if demo is working still

### Speak

todo: write a note on using .Speak to manually log messages and errors to Eris

### WHDIG

<!-- EDIT: expand on WHDIG format and how to load dumps when that's finalized -->

## Attributes

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

## Extension Methods

### Ask

Send a request into the river and receive an observable that emits only the response to *your specific* request - not responses to other concurrent requests of the same type.

```csharp
// Inside a Loom: on level completion, save then show results
Q += rzeka.Loom<LevelCompletedEvent, ResultsScreenRequest>(
    this,
    levelCompletedEvent => levelCompletedEvent.SelectMany(evt =>
        rzeka.Ask<SaveGameRequest, SaveGameResponse>(this, new SaveGameRequest())
             .Take(1)
             .Select(save => new ResultsScreenRequest(evt.Score, save.WasSuccessful)))
);
```

#### Nested Asks - Sequential

Each ask waits for its response before the next one fires.

```csharp
Q += rzeka.Loom<StartupRequest, UIReadyEvent>(
    this,
    reqs => reqs.SelectMany(req =>
        rzeka.Ask<LoadConfigRequest, LoadConfigResponse>(this, new LoadConfigRequest())
             .Take(1)
             .SelectMany(config =>
                 rzeka.Ask<LoadPlayerRequest, LoadPlayerResponse>(
                         this,
                         new LoadPlayerRequest(config.Profile))
                      .Take(1)
                      .Select(player => new UIReadyEvent(player.Data))))
);
```

#### Nested Asks - Independent

Both asks fire at the same time; the Loom waits for both before producing output.

```csharp
Q += rzeka.Loom<StartupRequest, UIReadyEvent>(
    this,
    reqs => reqs.SelectMany(req =>
        rzeka.Ask<LoadConfigRequest, LoadConfigResponse>(this, new LoadConfigRequest())
             .Take(1)
             .Zip(
                 rzeka.Ask<LoadPlayerRequest, LoadPlayerResponse>(this, new LoadPlayerRequest()),
                 (config, player) => new UIReadyEvent(config.Settings, player.Data)))
);
```

---

### Reacting

`.Reacting()` is the explicit operator for intentional side effects inside Loom chains. It reads as what it is — "react to this matter" — rather than abusing Rx's `.Do()` (a debug tap) or hiding logic inside `.Select()`.

Two overloads:

- `.Reacting(Action<T>)` — run a side effect, pass matter through unchanged. Sugar over `.Do()`.
- `.Reacting(Func<T, TOut>)` — run a side effect and produce output matter in one step. Sugar over `.Select()`.

```csharp
// Side effect only — matter flows on unchanged
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


---

### WithLatestFrom / CombineLatest

Tuple-returning convenience overloads defined in `ScrollExtensions`, named after the standard Rx operators they wrap. Unlike the standard Rx versions, these take no result selector and always return a flat tuple — so if you see `.CombineLatest(other)` returning `(T1, T2)`, it's these overloads, not the standard Rx ones.

```csharp
// WithLatestFrom: source is the trigger, other is "the current value of X"
// Silently drops source emissions that arrive before other has ever emitted.
source.WithLatestFrom(other) // → IObservable<(T1, T2)>
source.WithLatestFrom(ctx1, ctx2) // → IObservable<(T1, T2, T3)>

// CombineLatest: emits whenever either stream fires, always pairing with the latest of the other
streamA.CombineLatest(streamB) // → IObservable<(T1, T2)>
streamA.CombineLatest(streamB, streamC) // → IObservable<(T1, T2, T3)>
```

---

### Crossing

Strips circumstances from matter as it passes through a chain — useful when forwarding events across system boundaries where the original context is not relevant.

```csharp
Q += rzeka.Loom<ExternalInputEvent, InputCommand>(
    this,
    events => events.Crossing().Select(e => new InputCommand(e.Key))
);
```

---

### IsRespondingTo

Check whether a response corresponds to a specific request. Used internally by `Ask`, but can also be used manually inside Weave chains.

```csharp
var myRequest = new InventoryRequest();
rzeka.Scry<InventoryResponse>()
     .Where(r => r.IsRespondingTo(myRequest))
     .Take(1)
     .Subscribe(r => ShowInventory(r.Items));

rzeka.Pluck(this, myRequest);
```

---

## Async Operations

Rzeka is single-threaded, but your game will have async operations — resource loading, network calls, save/load, etc. The pattern for handling these is:

1. Wrap the async API in `Observable.Create` to bring it into Rx
2. Inside that wrapper, **manually attach circumstances** with `.WithCircumstances()` while you still have the triggering matter in scope
3. The result re-enters the synchronous Rx chain as a normal emission

Rzeka's automatic circumstance tracking cannot follow across an async boundary. Inside `Observable.Create`, the async callback fires later — potentially after other emissions have passed through the chain. By attaching circumstances manually at the point where you still hold the original trigger, you preserve the causal link.

<!-- EDIT: the example below is adapted from your Unity scene loader (SceneLoaderObservable.cs) — rewrite it to use Godot equivalents when the port is ready, but the pattern is identical -->

```csharp
// Wrap an async engine operation as an Observable
internal static class ResourceLoaderObservable
{
    public static IObservable<LoadSceneResponse> ObservableAsyncSceneLoad(
        this IObservable<LoadSceneRequest> source,
        SceneArchive sceneArchive)
    {
        return Observable.Create<LoadSceneResponse>(observer =>
        {
            return source.Subscribe(request =>
            {
                SceneDescriptor descriptor = sceneArchive
                    .GetSceneDescriptor(request.SceneType, request.SceneVersion);

                // Async boundary — this callback fires later
                BeginAsyncSceneLoad(descriptor, handle =>
                {
                    LoadSceneResponse response = handle.Succeeded
                        ? new LoadSceneResponse(request, handle.Result)
                        : new LoadSceneResponse(request);

                    // Manually attach circumstances — we still have `request` in scope
                    response = response.WithCircumstances<LoadSceneResponse>(request);

                    observer.OnNext(response);
                });
            });
        });
    }
}
```

Then use it inside a Loom normally:

```csharp
Q += rzeka.Loom<LoadSceneRequest, LoadSceneResponse>(
    this,
    requests => requests.ObservableAsyncSceneLoad(sceneArchive)
);
```

Because the output matter already has circumstances attached, Loom's automatic tracking steps aside and preserves them.

> **Important:** Only use this pattern when crossing a genuine async boundary. For synchronous Loom chains, let Rzeka handle circumstances automatically — don't attach them manually "just in case", as that silently disables the automatic tracking for that emission.
