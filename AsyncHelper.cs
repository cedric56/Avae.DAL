//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Avae.DAL;

//public static class AsyncHelper
//{
//    private static readonly TaskFactory _myTaskFactory = new
//(CancellationToken.None,
//                  TaskCreationOptions.None,
//                  TaskContinuationOptions.None,
//                  TaskScheduler.Default);

//    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
//    {
//        return _myTaskFactory
//          .StartNew(func)
//          .Unwrap()
//          .GetAwaiter()
//          .GetResult();
//    }

//    public static void RunSync(Func<Task> func)
//    {
//        _myTaskFactory
//           .StartNew(func)
//           .Unwrap()
//           .GetAwaiter()
//           .GetResult();
//    }

//    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
//    {
//        using var timeoutCancellationTokenSource = new CancellationTokenSource();

//        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
//        if (completedTask == task)
//        {
//            timeoutCancellationTokenSource.Cancel();
//            return await task;  // Very important in order to propagate exceptions
//        }
//        else
//        {
//            throw new TimeoutException("The operation has timed out.");
//        }
//    }
//}
