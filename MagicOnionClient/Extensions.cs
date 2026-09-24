using Grpc.Net.Client;
using GrpcWebSocketBridge.Client;
using MagicOnion;
using MagicOnion.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Avae.DAL;

public static class MagicOnionClientExtensions
{
    private static Func<Task>? _disconnect;

    public static Task<Func<Task>> AddStreamingHub<TObject>(
        this IDBMonitor<TObject> monitor,
        string url,
        IDBFactory dbFactory,
        HttpMessageHandler? httpMessageHandler = null,
        ILogger? logger = null)
        where TObject : class, new()
    {
        dbFactory.Monitors.Add(monitor);
        var channel = GetGrpcHandlerChannel(url, httpMessageHandler);
        return monitor.AddStreamingHub(channel, dbFactory, logger);
    }

    private static async Task<Func<Task>> AddStreamingHub<TObject>(
        this IDBMonitor<TObject> monitor, GrpcChannel channel, IDBFactory dbFactory, ILogger? logger = null)
        where TObject : class, new()
    {
        try
        {
            if (dbFactory.Sessions.TryGetValue(typeof(TObject), out _))
                return _disconnect ?? (() => Task.CompletedTask);

            var receiver = new RecordHubReceiver<TObject>(monitor, dbFactory);
            var hub = await StreamingHubClient.ConnectAsync<IRecordHub<TObject>, IRecordHubReceiver<TObject>>(channel, receiver);//, cancellationToken: cts.Token);
            var guid = await hub.AddReceiverAsync();
            dbFactory.Sessions.Add(typeof(TObject), guid.ToString());
            monitor.OnRecordChanged += OnRecordChanged;
            return _disconnect = async () =>
            {
                try
                {
                    await hub.RemoveAsync();               // 1. Business-logic call: tell server you're leaving
                    var disconnectTask = hub.WaitForDisconnectAsync(); // 2. Capture the task BEFORE disposing
                    await hub.DisposeAsync();              // 3. Actually triggers the disconnect
                    await disconnectTask;                  // 4. Confirm the disconnect
                }
                finally
                {
                    monitor.OnRecordChanged -= OnRecordChanged;
                }
            };

            void OnRecordChanged(object? sender, Record<TObject> e)
            {
                dbFactory.Sessions.TryGetValue(typeof(TObject), out var sessionId);
                e.Add(sessionId);
                hub.OnRecordChanged(e);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return () => Task.CompletedTask;
        }
    }

    public static IMagicService Create<IMagicService>(this IServiceProvider provider, string url) where IMagicService : IService<IMagicService>
    {
        var handler = new GrpcWebSocketBridgeHandler();
        if (!OperatingSystem.IsBrowser() && handler.InnerHandler is HttpClientHandler httpHandler)
            httpHandler.ServerCertificateCustomValidationCallback = ValidateCertificates2;
        var client = new HttpClient(handler);
        var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions()
        {
            HttpClient = client
        });
        return MagicOnionClient.Create<IMagicService>(channel);
    }

    private static GrpcChannel GetGrpcHandlerChannel(string url, HttpMessageHandler? httpMessageHandler = null)
    {
        if (httpMessageHandler != null)
            return GrpcChannel.ForAddress(url, new GrpcChannelOptions()
            {
                HttpHandler = httpMessageHandler
            });

        var handler = new GrpcWebSocketBridgeHandler();
        if (!OperatingSystem.IsBrowser() && handler.InnerHandler is HttpClientHandler httpHandler)
            httpHandler.ServerCertificateCustomValidationCallback = ValidateCertificates2;
        return GrpcChannel.ForAddress(url, new GrpcChannelOptions()
        {
            HttpHandler = handler
        });
    }

    private static bool ValidateCertificates2(HttpRequestMessage message, X509Certificate2? x509Certificate, X509Chain? x509Chain, SslPolicyErrors errors)
    {
        //TODO
        if (x509Certificate == null) return false;
        return true;
    }

    public static bool ValidateCertificates(object sender, X509Certificate? x509Certificate, X509Chain? x509Chain, SslPolicyErrors errors)
    {
        //TODO
        if (x509Certificate == null) return false;
        return true;
    }
}