# rzeka

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/EternalGarden.Rzeka?logo=nuget)](https://www.nuget.org/packages/EternalGarden.Rzeka)

**A reactive event bus for C# that tracks causality.**

rzeka ("_river_" in Polish) is a single-threaded event bus built on Rx.NET. Components publish typed events into the river and react to events flowing through it, without holding references to each other.

What makes rzeka different from a typical event bus or pub/sub library is **causality tracking**: every event automatically carries a record of the events that caused it. You can ask any event "_what caused you?_" and get the full chain back. Combined with Eris, rzeka's built-in debugger, this means you can step through the entire causal history of anything that happens in your system - live, in a browser, while the game runs.

rzeka is single-threaded by design. This is the constraint that makes everything else possible: it guarantees that circumstance tracking, mana transitions, and spell lifecycle are always consistent. Async operations are handled within defined boundaries - see [Async Operations](addlink).

**Status**: rzeka was originally built for [sanctuary](https://github.com/eternalgarden/sanctuary), a 3D journaling application shipped on Unity. It is currently being refactored alongside sanctuary's port to Godot. **The core API is stable**. The Godot integration and Eris UI are actively evolving.

## 🕯️ Grimoire

rzeka uses river, textile and magic vocabulary throughout:
- events are **Matter**
- transformations are **Spells**
- a Spell's required Matter types fulfillment status is called **Mana**
- Spell-defining methods (_Strand, Loom, Shuttle, Weave, Pluck_) follow textile-production vocabulary, inspired by [Zeros and Ones by Sadie Plant](https://www.goodreads.com/en/book/show/927879.Zeros_and_Ones))

**Eris**, the debugger, borrows her name from the Greek goddess of discord.

The metaphor is consistent and once you let it work its magic, the API becomes self-describing.

## Installation

Rzeka targets `net8.0` and depends only on `System.Reactive`.

```bash
dotnet add package EternalGarden.Rzeka
```

### Godot 4

Godot does not resolve transitive NuGet dependencies, so add System.Reactive explicitly:

```bash
dotnet add package System.Reactive
```

## 🌱 Getting Started

Create a single river at startup and share its IRzeka reference with the systems that need it:

```csharp
IRzeka rzeka = new Spring().Create("Nile");
```

📜🧨 You should have **one rzeka** per application - v1 is designed around single instance. Running multiple instances will break [circumstance chain tracking](#Circumstances).

Multi-rzeka topologies are considered for a v2 version. 

The name passed to Create serves currently a purely mythical role, it has no direct usage in rzeka codebase, but one should have the capacity to name the river that they live along.

### Hosting rzeka in Godot

The simplest pattern is a Autoload that owns the river instance:
```csharp
using Godot;
using Rzeka;

public partial class River : Node
{
    public static IRzeka Rzeka { get; private set; }

    public override void _Ready()
    {
        Rzeka = new Spring().Create("MyGame");
    }
}
```

Nodes access the river via `River.Rzeka`. In larger projects you may want to wire `IRzeka` through a DI container.

All Godot lifecycle callbacks (`_Ready`, `_Process`, `_PhysicsProcess`, etc.) run on the main thread, so Strand, Pluck, Loom, and Weave all work without any extra setup. 

Async operations crossing a thread boundary need manual circumstance handling - see [Async Operations](#async-operations).

For the live debugger during development, see [Eris](#eris).

Once you summoned your rzeka, you are ready to shape Matter.

## 🪽 Matter - Events

### Base

Matter is the base carrier of event data. Every matter has a `Guid` (its unique identity) and a list of `Circumstances` (a collection of matter that led to the emission of this one). 

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

📜🧨 Keep your Matter instances immutable - avoid mutable reference type fields. `WithCircumstances<T>()` works by cloning, Eris holds references to emitted matter for the causal graph. Mutating after emission corrupts that history and can produce subtle bugs when multiple subscribers hold the same instance.

### Circumstances

Circumstances attach context to a piece of matter. For example, a `DamageDealt` event could carry the `AttackEvent` that triggered it as a circumstance.

This allows you to track the causality chains through [Eris, rzeka's debugger](#Eris) or to check in your game logic if a given matter is caused by another (`damage_dealt.IsCircumstancedBy(wizard_fireball_attack)`). 

```csharp
var attack = new AttackEvent(attacker: "dragon");
var damage = new DamageDealt(amount: 40).WithCircumstances<DamageDealt>(attack);

// Later, check if a piece of matter is causally linked to something specific
bool causedByDragon = damage.IsCircumstancedBy(attack); // true
```

#### Automatic vs. Manual Circumstances

Rzeka tracks circumstances automatically inside **Loom** - when your spell produces output matter, the input matter that triggered it is attached as a circumstance. You don't need to do anything for this to work. This relies on the single-threaded guarantee: the causal link between input and output is always unambiguous.

If your output matter already has circumstances attached (via `.WithCircumstances()`), Rzeka will leave them alone and skip the automatic tracking. This means `.WithCircumstances()` inside a Loom lambda is an **active decision to override the default tracking**.

**Where automatic tracking works:**
- Synchronous Loom chains - the default, no action needed
- `Shuttle` responses - if the responder does not stamp manually, Shuttle attaches the triggering request as the circumstance for you. If the responder *does* stamp manually (e.g. to thread Scry'd context streams), Shuttle leaves your stamp alone - see the [Shuttle stamping rule](#shuttle---requestresponse).

**Where you must attach circumstances manually:**
- Inside `Pluck` and `Ask` calls - pre-stamp the matter via `.WithCircumstances<T>(trigger)` when you have the triggering matter in scope
- Inside async boundaries within Loom (see [Async Operations](#async-operations) below)

**Where circumstances are not touched:**
- `Strand` - it is used for root matter emissions (eg. caused by user input)

---

## 🧬 API

Initialize Rzeka with:

```csharp
IRzeka rzeka = new Spring().Create("MyGame");
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

### 🧬 Strand - publisher

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

### 🧬 Pluck - fire once publisher

Publish a single matter value into the river imperatively, without an ongoing stream. Pluck is fully visible to Eris — it appears as a `Plucking` spell with a `Created → Shaped → Forgotten` lifecycle attributed to the `who` you pass in.

```csharp
rzeka.Pluck(this, new GameStarted());
```

Pluck has no automatic upstream tracking — it does not know what caused it. When you *do* know (inside a Loom or Weave lambda where the triggering matter is in scope), pre-stamp the matter via `.WithCircumstances<T>()`:

```csharp
// Inside a Weave or other context where you have the triggering matter:
rzeka.Pluck(this, new GamePaused().WithCircumstances<GamePaused>(triggeringEvent));
```

---

### 🧬 Loom - transform

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
For neither Loom or Weave overloads with more than three input-matter types

---

### 🧬 Weave - subscriber

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

### 🧬 Scry - raw observable

TODO: Add Scry notes, they were incorrectly desciribng multi-rzeka situation which we suspended for v2.

```csharp
IObservable<PlayerDied> deaths = rzeka.Scry<PlayerDied>();
```

---

### 🧬 Shuttle - Request/Response

📜🐇 Shuttle is rzeka's request/response API. It operates on two specialised Matter types - extend them to define your round-trip pair:

```csharp
class InventoryRequest : Request { }

class InventoryResponse : Response<InventoryRequest>
{
    public IReadOnlyList<Item> Items { get; }
    public InventoryResponse(InventoryRequest request, IReadOnlyList<Item> items, bool wasSuccessful)
        : base(request, wasSuccessful) => Items = items;
}
```

The response carries a reference back to the original request so rzeka can route replies to the correct caller - you don't need to handle correlation, but the `: base(request, wasSuccessful)` constructor call is required.

📜🧭 The suggested convention is to keep both types in the same `.cs` file suffixed with `RR`, like `GetInventoryRR`.

Register a handler that answers incoming requests with `Shuttle`. Pair it with the [`Ask` extension method](#ask) on the requester side. Shuttle stays single-input by design - when a responder needs additional matter context, pull it through `Scry` inside the lambda (see "Multi-context responder" below).

```csharp
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

#### Multi-context response using Scry

When the response depends on more than just the request, pull the additional streams in via `Scry` inside the lambda. The request remains the trigger; everything else is read as ambient context.

```csharp
Q += rzeka.Shuttle<LoadSceneRequest, LoadSceneResponse>(
    this,
    reqs => reqs
        .WithLatestFrom(rzeka.Scry<GameState>(), rzeka.Scry<Settings>())
        .Select(ctx =>
        {
            var (req, state, settings) = ctx;
            // Stamp manually so the response carries [request, state, settings]
            // as circumstances.
            return new LoadSceneResponse(req, /* shape from state */, wasSuccessful: true)
                .WithCircumstances<LoadSceneResponse>(req, state, settings);
        })
);
```

**Stamping rule for Shuttle responses.** If you stamp circumstances manually on the response, **include the request yourself** — Shuttle leaves your manual stamp alone. If you do *not* stamp manually, Shuttle automatically stamps `[request]` for you. Either path works; the manual route is the only way to also record the additional context that shaped the response. Forgetting the request in a manual stamp does not break `Ask` correlation (that uses `response.Request.Guid` directly), but it does orphan the response from its triggering request in the Eris matter graph view.

> **Note:** Do not use `.Do()` or `.Reacting()` for internal state mutations inside a Shuttle - this can lead to race conditions since the response stream is shared among multiple potential requesting agents.

#### Ask - request side

📜🌱 Send a request into the river and receive an observable that emits only the response to *your specific* request - not responses to other concurrent requests of the same type. 

Ask is implemented over a one-shot Weave (for the response) plus a Pluck (for the request), so both halves of the round-trip are visible to Eris and attributed to the `who` you pass in. Cardinality is caller-controlled - use `.Take(1)` for a single-shot exchange or omit it to observe correlated responses as a stream.

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

##### Tracking circumstances when Ask(ing)

Inside a Loom, pre-stamp the request with the triggering matter so the full causal chain remains walkable in Eris:

```csharp
Q += rzeka.Loom<LoadSplashScreenRequest, LoadSplashScreenResponse>(
    this,
    splashReqs => splashReqs.SelectMany(splashReq =>
        rzeka.Ask<LoadSceneRequest, LoadSceneResponse>(
                this,
                new LoadSceneRequest(splashReq.SceneType)
                    .WithCircumstances<LoadSceneRequest>(splashReq))
             .Take(1)
             .Select(r => new LoadSplashScreenResponse(splashReq, r.WasSuccessful)))
);
```

The pre-stamped circumstances flow with the request as it enters the river; Shuttle's response then auto-stamps `[request]`, completing the chain.
TODO: shouldny this prestamping 

📜🧨 **Avoid nesting Asks.** Multi-step chains written as nested `SelectMany` / `Zip` inside a single Loom are hard to read and fight rzeka's model. Decompose them into separate Looms and Shuttles instead - each step stays readable, owns one responsibility, and causality flows automatically through the river.

---


## 🏹 Eris

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

## 🧼 Extension Methods

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

Check whether a response corresponds to a specific request — `Ask` uses this predicate internally to filter the response stream. Reach for it directly when you need a different lifecycle than Ask provides (e.g. observing every response to a long-lived request, or correlating inside a Weave you already own):

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

Prefer `Ask` for the standard request/response round-trip — it bundles the Weave + Pluck pair and the `IsRespondingTo` filter for you.

---

## 🖇️ Async Operations

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
