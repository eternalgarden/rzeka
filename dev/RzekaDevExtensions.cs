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
                new TypeJsonConverter()
            }
        };

        public static IDisposable EnableDevServer(this Spring spring, int port = 9222)
        {
            var disposables = new CompositeDisposable();
            var erises = new List<Eris>();
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
                        Task.Run(() =>
                        {
                            foreach (Eris eris in erises)
                            {
                                connectionSubscriptions.Add(SubscribeEris(eris, socket));
                            }
                        });
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

            // Track existing + future Eris instances — subscribe all active connections to each.
            disposables.Add(spring.Watch().Subscribe(river =>
            {
                erises.Add(river.Eris);

                foreach (var (socket, subscriptions) in activeConnections)
                {
                    subscriptions.Add(SubscribeEris(river.Eris, socket));
                }
            }));

            disposables.Add(spring.OnDisposed.Subscribe(river =>
            {
                erises.Remove(river.Eris);
            }));

            return disposables;
        }

        static IDisposable SubscribeEris(Eris eris, IWebSocketConnection socket)
        {
            var disposables = new CompositeDisposable();

            disposables.Add(eris.SerializableSpellOccurences.Subscribe(occ =>
                Send(socket, occ)));

            disposables.Add(eris.SerializableMatterOccurences.Subscribe(occ =>
                Send(socket, occ)));

            disposables.Add(eris.SerializableMessageOccurences.Subscribe(occ =>
                Send(socket, occ)));

            return disposables;
        }

        static void Send<T>(IWebSocketConnection socket, T occurence)
        {
            try
            {
                string json = JsonSerializer.Serialize(occurence, occurence!.GetType(), SerializerOptions);
                socket.Send(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Eris] Send error: {ex.Message}");
            }
        }
    }
}
