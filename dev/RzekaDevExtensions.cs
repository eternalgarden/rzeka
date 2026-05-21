using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Fleck;
using Rzeka.Serialization;

namespace Rzeka.Dev
{
    public static class RzekaDevExtensions
    {
        static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(),
                new TypeJsonConverter(),
                // Delegates IMatter serialization to the concrete runtime type so user-defined
                // properties (ScenePath, GameTitle, …) appear in the output. Must come before
                // CircumstancesJsonConverter — once we dispatch to the concrete type the
                // [JsonConverter] attribute on Matter.Circumstances fires for the list.
                new MatterJsonConverter(),
                // MatterJsonConverter dispatches to the concrete type, so the [JsonConverter]
                // attribute on Matter.Circumstances now fires and handles this. Kept as a
                // fallback for any IReadOnlyList<IMatter> that reaches the serializer outside
                // that path.
                new CircumstancesJsonConverter(),
            }
        };

        public static IDisposable EnableDevServer(this Spring spring, int port = 10470)
        {
            var disposables = new CompositeDisposable();
            WireStreams? currentWires = null;
            var activeConnections = new List<(IWebSocketConnection socket, CompositeDisposable subscriptions)>();

            var server = new WebSocketServer($"ws://127.0.0.1:{port}");

            server.Start(socket =>
            {
                var connectionSubscriptions = new CompositeDisposable();

                socket.OnOpen = () =>
                {
                    try
                    {
                        activeConnections.Add((socket, connectionSubscriptions));
                        Console.WriteLine("[Eris] Debugger connected");

                        // Defer subscription so the replay doesn't fire synchronously inside OnOpen
                        if (currentWires is not null)
                        {
                            var wires = currentWires;
                            Task.Run(() => connectionSubscriptions.Add(SubscribeSocket(wires, socket)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[Eris] OnOpen error: {ex.Message}");
                    }
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("[Eris] Debugger disconnected");
                    activeConnections.RemoveAll(c => c.socket == socket);
                    connectionSubscriptions.Dispose();
                };

                socket.OnError = ex =>
                {
                    Console.Error.WriteLine($"[Eris] WebSocket error: {ex.Message}");
                };
            });

            disposables.Add(Disposable.Create(() =>
            {
                server.Dispose();
                activeConnections.Clear();
            }));

            disposables.Add(spring.OnCreated.Subscribe(river =>
            {
                // Dispose the previous river's wire streams if a new river is appearing without
                // an explicit disposal of the old. Otherwise serialization keeps running against
                // a dead Eris and the streams leak.
                currentWires?.Dispose();

                var wires = new WireStreams(river.Eris);
                currentWires = wires;

                foreach (var (socket, subscriptions) in activeConnections)
                {
                    subscriptions.Add(SubscribeSocket(wires, socket));
                }
            }));

            disposables.Add(spring.OnDisposed.Subscribe(_ =>
            {
                currentWires?.Dispose();
                currentWires = null;
            }));

            return disposables;
        }

        static IDisposable SubscribeSocket(WireStreams wires, IWebSocketConnection socket)
        {
            // Session marker first, then the replay+live stream. Pushed per-subscription
            // (not into the replay buffer itself) so it reaches every fresh socket — including
            // connects more than a minute after game start, when the marker would otherwise
            // have aged out of the 1-minute window.
            SendRaw(socket, wires.SessionStartedJson);
            return wires.Occurrences.Subscribe(json => SendRaw(socket, json));
        }

        static void SendRaw(IWebSocketConnection socket, string json)
        {
            try
            {
                socket.Send(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Eris] Socket send error: {ex.Message}");
            }
        }

        static string DescribeOccurence(object occurence) => occurence switch
        {
            SerializableShapedMatter sm => $"Shaped {sm.matterType.Name} (matter {sm.matter.Guid})",
            SerializableReceivedMatter rm => $"Received matter {rm.receivedMatterGuid}",
            _ => occurence.GetType().Name,
        };

        // Per-Eris pre-serialized wire stream. Serializes each occurrence exactly once
        // and multicasts the resulting JSON to every connected socket, so serialization
        // failures publish their Horror once, not once per connected viewer.
        //
        // All three categories funnel into a single ReplaySubject so replay preserves
        // the original chronological interleave — a message emitted before a matter
        // replays before the matter, instead of all spells then all matters then all
        // messages (which is what three separate buckets produce).
        sealed class WireStreams : IDisposable
        {
            readonly ReplaySubject<string> _occurrences = new(TimeSpan.FromMinutes(1));
            readonly CompositeDisposable _sources = new();

            public IObservable<string> Occurrences => _occurrences.AsObservable();

            // Session marker carrying a fresh guid per river. Browser tracks the last
            // seen guid and clears its occurrence list when this changes — i.e. when the
            // game was stopped and restarted while the UI tab stayed open.
            public string SessionStartedJson { get; }

            public WireStreams(Eris eris)
            {
                SessionStartedJson = JsonSerializer.Serialize(
                    new
                    {
                        type = "session-started",
                        sessionGuid = Guid.NewGuid(),
                        riverName = eris.Name,
                        timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    },
                    SerializerOptions
                );

                _sources.Add(eris.SerializableSpellOccurences.Subscribe(occ =>
                    Serialize(eris, occ, _occurrences)));
                _sources.Add(eris.SerializableMatterOccurences.Subscribe(occ =>
                    Serialize(eris, occ, _occurrences)));
                _sources.Add(eris.SerializableMessageOccurences.Subscribe(occ =>
                    Serialize(eris, occ, _occurrences)));
            }

            static void Serialize<T>(Eris eris, T occurence, ReplaySubject<string> sink)
            {
                string json;
                try
                {
                    json = JsonSerializer.Serialize(occurence, occurence!.GetType(), SerializerOptions);
                }
                catch (Exception ex)
                {
                    // Skip republishing if a Message itself failed - that would recurse forever
                    // on the same broken payload.
                    if (occurence is not SerializableMessageOccurence)
                    {
                        eris.PublishMessage(new MessageOccurence
                        {
                            Guid = Guid.NewGuid(),
                            Timestamp = DateTimeOffset.Now,
                            RzekaMessageType = RzekaMessageType.Horror,
                            Message = $"Failed to serialize {DescribeOccurence(occurence!)}: {ex.Message}. Eris loses this matter - downstream Received occurrences and any circumstance references will display as MISSING and causality breaks. Mark engine-native handles, IO objects, and throwing getters with [JsonIgnore] (System.Text.Json.Serialization). The rest of the matter still serializes.",
                            Exception = ex,
                            Circumstances = Array.Empty<Guid>(),
                        });
                    }
                    Console.Error.WriteLine($"[Eris] Serialize error: {ex.Message}");
                    return;
                }

                sink.OnNext(json);
            }

            public void Dispose()
            {
                _sources.Dispose();
                _occurrences.OnCompleted();
            }
        }
    }
}
