# 💦📜🏹 rzeka

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![CI](https://github.com/eternalgarden/rzeka/actions/workflows/ci.yml/badge.svg)](https://github.com/eternalgarden/rzeka/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/eternalgarden/rzeka/branch/main/graph/badge.svg)](https://codecov.io/gh/eternalgarden/rzeka)
[![NuGet](https://img.shields.io/nuget/v/EternalGarden.Rzeka?logo=nuget)](https://www.nuget.org/packages/EternalGarden.Rzeka)
<!-- [![Downloads](https://img.shields.io/nuget/dt/EternalGarden.Rzeka?label=downloads)](https://www.nuget.org/packages/EternalGarden.Rzeka) -->

**A reactive event bus for C# that tracks causality.**

rzeka ("[_river_](https://en.wikipedia.org/wiki/Nile)" in Polish) is a single-threaded event bus built on Rx.NET. Components [yeet](https://youtu.be/S2t59dPf9K0?si=srvWNs02TmMQwvzH) typed events into the river and act on the events flowing through it, without holding references to each other.

What makes rzeka different from a typical event bus is **causality tracking**: every event carries a record of the events that caused it. 

You can ask any event – _o little rabbit! where did you come from?_ – and get its full casual chain – _what fox's chasing glare or carrots allure_. Thanks to Eris (rzeka's built-in debugger) you can read the entire story of what's happening in your system - real-time, in a browser, while your game runs (both in a game engine and in a build).

> 📐📜 **Important**: This README is a glimpse into rzeka, not the manual. [Full documentation in Wiki](https://github.com/eternalgarden/rzeka/wiki) – API reference, threading model, Eris, attributes & error handling.

<https://github.com/user-attachments/assets/de5e608b-5123-47dd-99c4-996be5ff5259>

> ⚗️ **rzeka is single-threaded by design**. This is the constraint that makes everything else possible: it guarantees that circumstance tracking, mana transitions, and spell lifecycle are always consistent. Async operations are handled within defined boundaries.

**Status**: rzeka was originally built for [sanctuary](https://github.com/eternalgarden/sanctuary), a 3D journaling software shipped on Unity. It is currently being refactored alongside sanctuary's port to Godot. **The core API is stable**. Eris UI and Godot integration are actively evolving.

## 🪞 The metaphor

rzeka uses a river, textile and magic themed naming system: events are **Matter**, transformations are **Spells**, and a spell's ingredient-readiness is its **Mana**. The spell-defining methods (_Strand, Loom, Shuttle, Weave, Pluck_) follow textile-production vocabulary. **Eris**, the debugger, borrows her name from the Greek goddess of discord.

The metaphor is consistent, and once you let it work its magic the API becomes self-describing. The full vocabulary lives in the **[Grimoire](https://github.com/eternalgarden/rzeka/wiki/Grimoire)**.

![](https://github.com/eternalgarden/rzeka/blob/main/docs/Daisuke_Igarashi_Witches.png)
> A page from [Witches by Daisuke Igarashi](https://en.wikipedia.org/wiki/Witches_(manga))

## 💾 Installation

rzeka targets `net8.0` and depends only on `System.Reactive`.

```python
dotnet add package EternalGarden.Rzeka
```

For the optional browser-based Eris debugger (`EternalGarden.Rzeka.Dev`) and Godot's transitive-dependency quirks, go to the **[Installation](https://github.com/eternalgarden/rzeka/wiki/Installation)** page.

## 🌱 A taste

Summon one river at startup, then let typed events flow into it and react to them - no component holds a reference to any other. Here is a complete vertical slice: a witch is hexed, her ward weakens, its glow answers on screen.

```csharp
using Rzeka;

// Events are Matter. Extend Matter to shape your own.
class HexCast : Matter
{
    public int Potency { get; }
    public HexCast(int potency) => Potency = potency;
}

// [HasState] matter remembers its last value and replays it to new subscribers.
[HasState]
class WardState : Matter
{
    public int Strength { get; }
    public WardState(int strength) => Strength = strength;
}

// Summon the river once, share its IRzeka reference with the systems that need it.
// mainThread is an IScheduler for your engine's main thread - see Getting Started.
IRzeka rzeka = new Spring().Create("Styx", mainThread);

// Collect your spells so you can let them all go together later.
CollectibleDisposable Q = new();

// Seed the ward at full strength.
rzeka.Pluck(this, new WardState(100));

// Strand: let every hex the enemy hurls flow into the river.
// OnHexCastAsObservable() is something using Observable.FromEvent() or similar
Q += rzeka.Strand(
    this,
    enemy.OnHexCastAsObservable().Select(hex => new HexCast(hex.Potency))
);

// Loom: weave each hex against the standing ward, conjuring its next state.
// The HexCast matter is the trigger - WithLatestFromMatter fires off the hexes, not the ward updates.
Q += rzeka.Loom<WardState, HexCast, WardState>(
    this,
    (ward, hexes) => hexes.WithLatestFromMatter(ward)
        .Select(t => new WardState(t.Item2.Strength - t.Item1.Potency))
);

// Weave: the path ending spell - read the ward, let its glow update in game.
Q += rzeka.Weave<WardState>(
    this,
    ward => ward.Subscribe(w => wardGlow.SetIntensity(w.Strength))
);
```

Every `WardState` here automatically remembers the `HexCast` that diminished it as a circumstance, so Eris can later show the whole causal chain. That automatic causality tracking is the core functionality of rzeka - the **[API](https://github.com/eternalgarden/rzeka/wiki/API)** and **[Matter](https://github.com/eternalgarden/rzeka/wiki/Matter)** pages cover it fully.

![](https://media.githubusercontent.com/media/eternalgarden/rzeka/refs/heads/main/docs/code_aqua.png)
> 📜🧚🏻‍♀️ rzeka code will make the characteristic waterfall 2D structures that go deep into your indentation while remaining very clear and readable. This depth might not be for everyone though. Personally I really prefer that to a 1D top-down wall of code-text, maybe you might like it too! _Screenshot info_: nvim, theme [Aquavium](https://github.com/T-b-t-nchos/Aquavium.nvim), semitransparent background, CSharpier formatter.

> 📜🌱 Want a runnable project instead? See [little-river](https://github.com/eternalgarden/little-river), a tiny example Godot game built on rzeka.

## 🏹 Eris, the debugger

rzeka ships with a browser-based debugger that connects to your running game over WebSocket and shows matter flow and messages in real time – you don't need to build your own in-game UI! Eris records internally even in release builds (a feature to dump crash logs is underway). The WebSocket server is added only in dev builds via the `EternalGarden.Rzeka.Dev` package so you can easily strip it from your release builds.

Setup, the live UI, the demo, and structured logging via `Whisper` are all covered on the **[Eris](https://github.com/eternalgarden/rzeka/wiki/Eris)** page.
<!-- 
## 📚 Documentation

The wiki is the source of truth. Start at the **[Home](https://github.com/eternalgarden/rzeka/wiki)** page, or jump straight to a topic:

- [🪞 Grimoire](https://github.com/eternalgarden/rzeka/wiki/Grimoire) - the full naming metaphor and vocabulary
- [💾 Installation](https://github.com/eternalgarden/rzeka/wiki/Installation) - core, Eris dev server, and Godot-specific setup
- [🌱 Getting Started](https://github.com/eternalgarden/rzeka/wiki/Getting-Started) - creating the river and hosting it in Godot
- [🪽 Matter](https://github.com/eternalgarden/rzeka/wiki/Matter) - defining events and working with circumstances
- [🧬 API](https://github.com/eternalgarden/rzeka/wiki/API) - Strand, Pluck, Loom, Weave, Scry, Shuttle, circumstance stamping rules
- [👻 Mana & Lifecycle](https://github.com/eternalgarden/rzeka/wiki/Mana-and-Lifecycle) - how spells gate on their ingredients
- [🏹 Eris](https://github.com/eternalgarden/rzeka/wiki/Eris) - the debugger, the live UI, and `Whisper` logging
- [🪧 Attributes](https://github.com/eternalgarden/rzeka/wiki/Attributes) - `[HasState]` and evolving stateful matter
- [🧩 Extension Methods](https://github.com/eternalgarden/rzeka/wiki/Extension-Methods) - `Reacting`, the tuple combinators, `IsRespondingTo`
- [🛟 Error Boundary](https://github.com/eternalgarden/rzeka/wiki/Error-Boundary) - how unhandled source errors are caught and surfaced
- [🪃 Async Operations](https://github.com/eternalgarden/rzeka/wiki/Async-Operations) - crossing async boundaries while preserving causality
<br><br>
-->
## 📜 License

See [LICENSE](LICENSE).
