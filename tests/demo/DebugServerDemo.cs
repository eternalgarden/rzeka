using System.Reactive.Linq;
using Rzeka;
using Rzeka.Dev;

namespace Rzeka.Tests.Demo
{
    /// <summary>
    /// Integration demo that spins up Rzeka with the debug server,
    /// registers some spells, and emits matter on a timer.
    ///
    /// Run with: dotnet test --filter "FullyQualifiedName~DebugServerDemo" -- xUnit.MaxParallelThreads=1
    /// Then open the Eris UI (cd ui && npm run dev) and watch live data flow in.
    /// </summary>
    public class DebugServerDemo
    {
        [Fact]
        public async Task RunDemo()
        {
            // --- Setup ---
            var spring = new Spring();
            using var debugServer = spring.EnableDevServer();
            var rzeka = spring.Create("Demo");

            var Q = new CollectibleDisposable();
            int health = 100;

            // --- Strand: one-off game start event ---
            Q += rzeka.Strand(this, Observable.Return(new GameStarted()));

            // --- Strand: emit PlayerMoved every 500ms ---
            var random = new Random(42);
            Q += rzeka.Strand(
                this,
                Observable
                    .Interval(TimeSpan.FromMilliseconds(500))
                    .Select(_ => new PlayerMoved(
                        (float)(random.NextDouble() * 100),
                        (float)(random.NextDouble() * 100)))
            );

            // --- Strand: emit DamageTaken every 2s ---
            var sources = new[] { "goblin", "trap", "fire", "falling" };
            Q += rzeka.Strand(
                this,
                Observable
                    .Interval(TimeSpan.FromSeconds(2))
                    .Select(i => new DamageTaken(
                        random.Next(5, 25),
                        sources[i % sources.Length]))
            );

            // --- Loom: DamageTaken -> HealthState ---
            Q += rzeka.Loom<DamageTaken, HealthState>(
                this,
                damage => damage.Select(d =>
                {
                    health = Math.Max(0, health - d.Amount);
                    return new HealthState(health, 100);
                })
            );

            // --- Loom: HealthState -> PlayerDied (when health reaches 0) ---
            Q += rzeka.Loom<HealthState, PlayerDied>(
                this,
                hp => hp
                    .Where(h => h.Current <= 0)
                    .Select(_ => new PlayerDied("accumulated damage"))
            );

            // --- Weave: log deaths ---
            Q += rzeka.Weave<PlayerDied>(
                this,
                deaths => deaths.Subscribe(d =>
                    Console.WriteLine($"Player died: {d.Cause}"))
            );

            // --- Let it run ---
            Console.WriteLine("Debug server running on ws://127.0.0.1:9222");
            Console.WriteLine("Open Eris UI (cd ui && npm run dev) to see live data.");
            Console.WriteLine("Waiting 30 seconds...");

            await Task.Delay(TimeSpan.FromSeconds(30));

            // --- Cleanup ---
            Q.Dispose();
            Console.WriteLine("Demo complete.");
        }
    }
}
