using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using Fleck;

namespace Rzeka.Dev
{
    public static class RzekaDevExtensions
    {
        public static IDisposable EnableDevServer(this Spring spring, int port = 9222)
        {
            var disposables = new CompositeDisposable();
            var connections = new List<IWebSocketConnection>();

            var server = new WebSocketServer($"ws://127.0.0.1:{port}");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    connections.Add(socket);
                    Console.WriteLine($"Eris debugger connected ({connections.Count} client(s))");
                };

                socket.OnClose = () =>
                {
                    connections.Remove(socket);
                    Console.WriteLine($"Eris debugger disconnected ({connections.Count} client(s))");
                };
            });

            disposables.Add(Disposable.Create(() =>
            {
                server.Dispose();
                connections.Clear();
            }));

            foreach (Eris eris in spring.AllErises)
            {
                disposables.Add(SubscribeEris(eris, connections));
            }

            // Subscribe to future Eris instances as they're created
            disposables.Add(spring.OnInstanceCreated.Subscribe(rzeka =>
            {
                disposables.Add(SubscribeEris(rzeka.Eris, connections));
            }));

            return disposables;
        }

        static IDisposable SubscribeEris(Eris eris, List<IWebSocketConnection> connections)
        {
            var disposables = new CompositeDisposable();

            disposables.Add(eris.SerializableSpellOccurences.Subscribe(occ =>
                Broadcast(connections, occ)));

            disposables.Add(eris.SerializableMatterOccurences.Subscribe(occ =>
                Broadcast(connections, occ)));

            disposables.Add(eris.SerializableMessageOccurences.Subscribe(occ =>
                Broadcast(connections, occ)));

            return disposables;
        }

        static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        static void Broadcast<T>(List<IWebSocketConnection> connections, T occurence)
        {
            if (connections.Count == 0) return;

            string json = JsonSerializer.Serialize(occurence, occurence!.GetType(), SerializerOptions);

            foreach (var connection in connections)
            {
                connection.Send(json);
            }
        }
    }
}
