using System;
using System.Threading;
using System.Threading.Tasks;

namespace TurboPort
{
    public class DelayService : IDelayService
    {
        public Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            return Task.Delay(timeSpan, cancellationToken);
        }
    }
}