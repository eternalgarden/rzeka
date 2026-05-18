using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        public static IDisposable EnableDevServer(this Spring spring, int port = 9222)
        {
            var disposables = new CompositeDisposable();
            Eris? currentEris = null;
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

                        // Defer subscription so it doesn't fire replay synchronously inside OnOpen
                        if (currentEris is not null)
                        {
                            var eris = currentEris;
                            Task.Run(() => connectionSubscriptions.Add(SubscribeEris(eris, socket)));
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
                currentEris = river.Eris;
                foreach (var (socket, subscriptions) in activeConnections)
                {
                    subscriptions.Add(SubscribeEris(river.Eris, socket));
                }
            }));

            disposables.Add(spring.OnDisposed.Subscribe(_ => currentEris = null));

            return disposables;
        }

        static IDisposable SubscribeEris(Eris eris, IWebSocketConnection socket)
        {
            var disposables = new CompositeDisposable();

            disposables.Add(eris.SerializableSpellOccurences.Subscribe(occ =>
                Send(eris, socket, occ)));

            disposables.Add(eris.SerializableMatterOccurences.Subscribe(occ =>
                Send(eris, socket, occ)));

            disposables.Add(eris.SerializableMessageOccurences.Subscribe(occ =>
                Send(eris, socket, occ)));

            return disposables;
        }

        static void Send<T>(Eris eris, IWebSocketConnection socket, T occurence)
        {
            string json;
            try
            {
                json = JsonSerializer.Serialize(occurence, occurence!.GetType(), SerializerOptions);
            }
            catch (Exception ex)
            {
                // Surface the failure through Eris so the dev UI sees it instead of silently
                // dropping the occurrence. Skip republishing if a Message itself failed -
                // that would recurse forever on the same broken payload.
                if (occurence is not SerializableMessageOccurence)
                {
                    eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Failed to serialize {DescribeOccurence(occurence!)} - {ex.Message}. Check for non-serializable properties on matter (engine-native handles, IO objects, throwing getters). Please mark them with [JsonIgnore] from System.Text.Json.Serialization namespace.",
                        Exception = ex,
                        Circumstances = Array.Empty<Guid>(),
                    });
                }
                Console.Error.WriteLine($"[Eris] Send error: {ex.Message}");
                return;
            }

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
    }
}
