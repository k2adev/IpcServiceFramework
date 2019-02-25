using System.Threading;
using System.Threading.Tasks;

namespace K2adev.IpcServiceFramework
{
    public interface IIpcServiceHost
    {
        void Run();

        Task RunAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}