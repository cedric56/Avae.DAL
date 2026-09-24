using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avae.DAL;

public class ConnectionTracker<TObject> : IDisposable where TObject : class, new()
{
    readonly HashSet<string> connections = new();
    readonly object gate = new();
    readonly IHubContext<SignalRHub<TObject>> hubContext;

    readonly IDBMonitor<TObject> monitor;

    readonly ILogger? logger;

    public ConnectionTracker(
        IHubContext<SignalRHub<TObject>> hubContext,
        IDBMonitor<TObject> monitor,
        IDBFactory factory,
        ILogger? logger = null)
    {
        this.logger = logger;
        this.monitor = monitor;
        this.hubContext = hubContext;
        factory.Monitors.Add(monitor);
        monitor.OnRecordChanged += OnRecordChanged; // subscribed exactly ONCE, ever
    }

    public void Add(string connectionId)
    {
        lock (gate) connections.Add(connectionId);
        logger?.LogInformation($"Customer connected: {connectionId}");
    }

    public void Remove(string connectionId)
    {
        lock (gate) connections.Remove(connectionId);
        logger?.LogInformation($"Customer disconnected: {connectionId}");
    }

    public async void OnRecordChanged(object? sender, Record<TObject> e)
    {
        var excludedIds = e.Connections ?? [];

        List<string> notified;
        lock (gate)
        {
            notified = connections.Where(id => !excludedIds.Contains(id)).ToList();
        }

        foreach (var id in notified)
            logger?.LogInformation($"Connected : {id}");

        await hubContext.Clients.AllExcept(excludedIds).SendAsync(SignalRExtensions.DBMessage, e);
    }

    public void Dispose()
    {
        monitor.OnRecordChanged -= OnRecordChanged;
    }
}
