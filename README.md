# TODO
- how to deal with whidg generation in the end, i guess rzeka users should be able to modify how the final report is generated, like lol without the ascii art for example 
- dev server instructions, installation instructons

# Rzeka

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)

**Rzeka is a single-threaded reactive event bus.** All matter must be published and consumed on the same thread. This constraint is enforced at runtime and is by design — it guarantees that circumstance tracking, mana transitions, and spell lifecycle are always consistent without locks or synchronization.

Rzeka ("river" in Polish) is built on Rx.NET. All data flows as typed streams — components publish into the river and subscribe to it without holding direct references to each other.

## Installation

_TODO: add Unity Package Manager / Godot installation instructions._

Mention that it works best with DI?

## Events

Events carried through Rzeka are called **Matter**.

### Matter

The base carrier of event data. Every matter has a `Guid` (its identity) and a `Circumstances` list (contextual metadata — other matter instances that provide context for this one). 

Guid and Circumstances attachment is handled by rzeka automatically so it can be later tracked in your integration tests and observed in the provided [Eris debugger tool](#Eris).

Matter instances are compared by `Guid`, so two separate instances of the same type are never equal.

Extend `Matter` to define your own event types:

```c
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

A pair of Matter specialisations for request/response patterns. The response carries a reference back to the original request, so the requester can verify the response is for their specific request.

You are required to implement the `.base()` constructor call.

This is what [Shuttle API method](#Shuttle) described below uses automatically under the hood.

```c
class InventoryRequest : Request { }

class InventoryResponse : Response<InventoryRequest>
{
    public IReadOnlyList<Item> Items { get; }
    public InventoryResponse(InventoryRequest request, IReadOnlyList<Item> items, bool wasSuccessful)
        : base(request, wasSuccessful) => Items = items;
}
```

### Circumstances

Circumstances attach context to a piece of matter. For example, a `DamageDealt` event could carry the `AttackEvent` that triggered it as a circumstance, allowing you to track the causality chains through [Eris, rzeka's debugger](#Eris)

```c
var attack = new AttackEvent(attacker: "dragon");
var damage = new DamageDealt(amount: 40).WithCircumstances<DamageDealt>(attack);

// Later, check if a piece of matter is causally linked to something specific
bool causedByDragon = damage.IsCircumstancedBy(attack); // true
```

##### Automatic vs Manual Circumstances

Rzeka tracks circumstances automatically inside **Loom** and **Interlace** - when your spell produces output matter, the input matter that triggered it is attached as a circumstance. You don't need to do anything for this to work. This relies on the single-threaded guarantee: the causal link between input and output is always unambiguous.

If your output matter already has circumstances attached (via `.WithCircumstances()`), Rzeka will leave them alone and skip the automatic tracking. This means `.WithCircumstances()` inside a Loom or Interlace lambda is an **active decision to override the default tracking**.

TODO, IMPLEMENT (Eris): Display information in Eris whether matteroccurence has manually assigned circumstances.

**Where automatic tracking works:**
- Synchronous Loom/Interlace chains — the default, no action needed

**Where you must attach circumstances manually:**
- Inside `Pluck` calls — Pluck has no automatic tracking
- Inside async boundaries within Loom (see [Async Operations](#async-operations) below)
- On matter passed to external systems outside Rzeka (What do you mean by that Calude?)

**Where circumstances are not touched:**
- `Strand` — passes matter through to the Library as-is

---

## API

Initialize Rzeka with:

```c
// TODO shouldn't we actually make springriver constructor internal and force the user to initialize it through the factory.
IRzeka rzeka = new SpringRiver("MyGame");

// TODO Add notes on child rivers and consider implementing at least a rzkea factory which would inform Eris about all existing rivers, also potentially implement an 
```

All rzeka API methods accept a `who` object (the registering owner, used for diagnostics) and return `IDisposable` to unregister. Observables and lambda functions you pass into them are called *spells*.

A common pattern is to collect them into a composite disposable:

```c
CollectibleDisposable Q = new();

Q += rzeka.Loom<PlayerInputState, PlayerMovementState>(...)

// on destroy / cleanup:
Q.Dispose();
```

`CollectibleDisposable` is a wrapper around [CompositeDisposable][https://learn.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh228980(v=vs.103)], it overloads `+` operator allowing you to neatly add your rzeka subscriptions to it. It does not implement `.Clear()` method of the `CompositeDisposable` because that would likely lead you to accidental memory leaks.

Loom, Interlace and Weave API methods below have overloads that allow two input matter types. If you need more, pull additional streams manually via `Scry`:

```c
Q += rzeka.Loom<A, B, Out>(
    this,
    (a, b) =>
        a.WithLatestFrom(b, rzeka.Scry<C>())
         .Select((aVal, bVal, cVal) => new Out(...))
);
```

---

### Strand — Publisher

Registers a source `IObservable<T>` into the river. Any Loom or Weave that listens to `T` will receive these values.

```c
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

### Pluck — Fire Once

Publish a single matter value into the river imperatively, without an ongoing stream.

```c
rzeka.Pluck(this, new GameStarted());
```
Pluck has no automatic circumstance tracking — it doesn't know what caused it. If causality matters, attach circumstances manually with `.WithCircumstances()` before plucking:

<!-- EDIT: you had a note here asking for an Interlace + Pluck + WithCircumstances example — write one from a real game scenario when ready -->

```csharp
// Inside a Weave or other context where you have the triggering matter:
rzeka.Pluck(this, new GamePaused().WithCircumstances<GamePaused>(triggeringEvent));
```

---

### Loom — Pure Transform

Listens to one or more streams and produces a new stream. No side effects — use it when you're just mapping or combining data.

Loom automatically attaches the triggering input matter as a circumstance on the output. You do not need to call `.WithCircumstances()` manually — and if you do, it will override the automatic tracking (see [Automatic vs Manual Circumstances](#automatic-vs-manual-circumstances)).

**Single input:**
```c
// Transform health change events into UI update events
Q += rzeka.Loom<HealthChanged, HealthBarUpdateRequested>(
    this,
    health => health.Select(e => new HealthBarUpdateRequested(e.NewValue, e.MaxValue))
);
```

**Two inputs — combine latest:**
```c
// Produce a rendering update whenever position OR animation state changes
Q += rzeka.Loom<PositionChanged, AnimationStateChanged, RenderUpdateRequested>(
    this,
    (positions, animations) =>
        positions
            .CombineWith(animations)
            .Select(((PositionChanged pos, AnimationStateChanged anim) pair) =>
                new RenderUpdateRequested(pair.pos.Position, pair.anim.Clip))
);
```

**Three inputs:**
```c
Q += rzeka.Loom<InputEvent, PhysicsState, GameState, MovementCommand>(
    this,
    (inputs, physics, game) =>
        inputs
            .WithContext(physics)
            .WithContext(game)
            .Select(/* ... */)
);
```

---

### Interlace — Reacting Transform

Like Loom, but receives a `LoomContext` that lets you report side effects to the Eris debugger. Use this instead of Loom when the transform causes a visible component reaction (e.g. playing a sound, triggering an animation), so it shows up in the diagnostics stream.

Same automatic circumstance tracking as Loom applies — see [Automatic vs Manual Circumstances](#automatic-vs-manual-circumstances).

```csharp
// When the player takes damage, shake the camera and emit a screen effect event
Q += rzeka.Interlace<DamageReceived, ScreenShakeRequested>(
    this,
    (damage, ctx) =>
        damage.Reacting(ctx, e =>
        {
            _camera.Shake(e.Amount); // side effect, tracked by ctx
            return new ScreenShakeRequested(e.Amount);
        })
);
```

The `.Reacting(ctx, reaction)` extension collapses `.Do()` + `.Select()` into one step and publishes a `ReactingOccurence` to Eris so the effect is visible in the debugger.

---

### Weave — Subscriber

Terminal subscriber — consumes streams and produces nothing. Use for final effects: rendering, audio, persistence, etc.

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

### Shuttle — Request/Response Provider

Like Loom, but works exclusively with `IRequest` / `IResponse<T>` types. Register a handler that answers incoming requests. Pair it with [`Ask`](#ask) on the requester side.

```c
// Simple sync query
Q += rzeka.Shuttle<PlayerStatsRequest, PlayerStatsResponse>(
    this,
    reqs => reqs.Select(req =>
        new PlayerStatsResponse(req, _player.Health, wasSuccessful: true))
);

// Async: each request triggers an async operation
Q += rzeka.Shuttle<SaveGameRequest, SaveGameResponse>(
    this,
    reqs => reqs.SelectMany(req =>
        _saveSystem.SaveAsync()
                   .Select(success => new SaveGameResponse(req, success)))
);
```

> **Note:** Do not use `.Do()` or `.Reacting()` for internal state mutations inside a Shuttle — this can lead to race conditions since the response stream is shared.

---

### Scry — Raw Observable

Returns the raw `IObservable<T>` for a matter type, without registering a spell. Primarily used to bridge between multiple Rzeka instances.

```csharp
IObservable<PlayerDied> deaths = rzeka.Scry<PlayerDied>();

// Bridge: re-strand events from one river into another
Q += otherRzeka.Strand(this, rzeka.Scry<GameStateChanged>());
```

---

## Eris

Eris is Rzeka's internal diagnostics bus. It records every spell lifecycle event (created, has mana, no mana, forgotten), every matter emission (shaped, received), and every message/exception — all timestamped and serialized. This runs in core and is always active, even in release builds, so that diagnostic data is available for crash dumps (WHDIG files).

<!-- EDIT: expand on WHDIG format and how to load dumps when that's finalized -->

### Live Debugger

Rzeka ships with a browser-based debugger that connects to your game over WebSocket. It shows live spell states, matter flow, and messages in real time — no in-game UI needed.

**Setup:**

1. Add a reference to `Rzeka.Dev` in your game project (dev builds only) TODO: show people with no big experience with net how to do it in their csproj 
2. In your game initialization:

```c
var spring = new Spring();
spring.EnableDevServer(); // starts WebSocket on ws://127.0.0.1:9222

IRzeka rzeka = spring.Create("MyGame");
```

3. Start the Eris UI:

```
cd rzeka/ui
npm install   # first time only
npm run dev
```

4. Open `http://localhost:5173` in any browser

The debugger auto-connects to the game's WebSocket server. If the game isn't running, it reconnects automatically every 3 seconds. All Rzeka instances created through the factory are picked up automatically.

The `Rzeka.Dev` package is the only part that adds an external dependency (Fleck for WebSocket). Core remains dependency-free beyond `System.Reactive`. Remove the `Rzeka.Dev` reference for release builds — Eris continues recording internally, the WebSocket server simply isn't started.

### Running the Demo

A demo test is included that spins up Rzeka with sample spells and emits matter on timers, useful for developing the UI or verifying the debug pipeline:

Terminal 1 — start the UI
- `cd rzeka/ui`
- `npm run dev`

Terminal 2 — run the demo (30 seconds)
- `cd rzeka/tests`
- `dotnet test --filter "FullyQualifiedName~DebugServerDemo" -- xUnit.MaxParallelThreads=1`

<!-- EDIT: add a screenshot of the Eris UI showing the demo output -->

## Attributes

### HasState

A very important rzeka attribute that you will be using for your matter types that are supposed to hold on to the last emission of this matter type and immediately provide it to its new subscribers.

The suggested naming convention for stateful matter types is adding *State* suffix to them.

```c
[HasState]
class PlayerInputState : Matter
{
    public InputState InputState { get; }
    public PlayerInputState(InputState state) => InputState = state;
}
```

## Extension Methods

### Ask

Send a request into the river and receive an observable that emits only the response to *your specific* request — not responses to other concurrent requests of the same type.

```csharp
// Inside a Loom: on level completion, save then show results
Q += rzeka.Loom<LevelCompletedEvent, ResultsScreenRequest>(
    this,
    events => events.SelectMany(evt =>
        rzeka.Ask<SaveGameRequest, SaveGameResponse>(this, new SaveGameRequest())
             .Take(1)
             .Select(save => new ResultsScreenRequest(evt.Score, save.WasSuccessful)))
);
```

#### Nested Asks — Sequential

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

#### Nested Asks — Independent

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

`.Reacting(action)` is a side-effect operator for use inside Loom chains. It wraps `.Do()` to make intentional reactions explicit and readable.

```csharp
Q += rzeka.Loom<EnemyDefeated, XpGranted>(
    this,
    events => events
        .Reacting(e => _combatLog.Record(e))   // side effect
        .Select(e => new XpGranted(e.XpReward))
);
```

`.Reacting(ctx, reaction)` is the Interlace variant — see [Interlace](#interlace--reacting-transform).

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
