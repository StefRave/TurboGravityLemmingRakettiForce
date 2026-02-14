using System;
using System.Threading;
using System.Threading.Tasks;

namespace TurboPort
{
    public interface IDelayService
    {
        Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}