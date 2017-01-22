using System;
using System.Threading;
using System.Threading.Tasks;

namespace TurboPort.Test
{
    public class DelayServiceMock : IDelayService
    {
        private TaskCompletionSource<bool> completionSource;
        private TaskCompletionSource<bool> waitForDelayCompletionSource;

        public DelayServiceMock()
        {
            Initialize();
        }

        private void Initialize()
        {
            completionSource = new TaskCompletionSource<bool>();
            waitForDelayCompletionSource = new TaskCompletionSource<bool>();
        }

        public void Continue()
        {
            completionSource.SetResult(true);
            Initialize();
        }

        public Task WaitForDelayCall()
        {
            return waitForDelayCompletionSource.Task;
        }

        public Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => completionSource.SetCanceled());
            waitForDelayCompletionSource.SetResult(true);
            return completionSource.Task;
        }
    }
}