using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace Avae.DAL;

public class FiveSecondsReconnectPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}
