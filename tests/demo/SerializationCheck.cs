using System.Text.Json;
using System.Reactive.Linq;
using Rzeka;
using Rzeka.Serialization;

namespace Rzeka.Tests.Demo
{
    public class SerializationCheck
    {
        static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(),
                new TypeJsonConverter()
            },
            WriteIndented = true
        };

        [Fact]
        public void PrintSerializedOccurences()
        {
            var spring = new Spring();
            var rzeka = spring.Create("Test", System.Reactive.Concurrency.ImmediateScheduler.Instance) as SpringRiver;

            // Capture serialized output
            var spellJsons = new List<string>();
            var matterJsons = new List<string>();

            rzeka!.Eris.SerializableSpellOccurences.Subscribe(occ =>
            {
                string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
                spellJsons.Add(json);
            });

            rzeka.Eris.SerializableMatterOccurences.Subscribe(occ =>
            {
                string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
                matterJsons.Add(json);
            });

            // Create a strand to trigger spell + matter occurences
            var Q = new CollectibleDisposable();
            Q += rzeka.Strand(this, Observable.Return(new GameStarted()));

            Q.Dispose();

            Console.WriteLine("=== SPELL OCCURENCES ===");
            foreach (var j in spellJsons) Console.WriteLine(j);

            Console.WriteLine("\n=== MATTER OCCURENCES ===");
            foreach (var j in matterJsons) Console.WriteLine(j);
        }
    }
}
