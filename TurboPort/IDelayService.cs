using System;
using System.Threading;
using System.Threading.Tasks;

namespace TurboPort.Test
{
    public interface IDelayService
    {
        Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}