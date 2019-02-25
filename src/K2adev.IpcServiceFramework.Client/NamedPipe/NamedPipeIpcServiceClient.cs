using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace K2adev.IpcServiceFramework.NamedPipe
{
    internal class NamedPipeIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly string _pipeName;

        public NamedPipeIpcServiceClient(IIpcMessageSerializer serializer, string pipeName, IpcServiceOptions options)
            : base(serializer, options)
        {
            _pipeName = pipeName;
        }

        protected override async Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None);
            await stream.ConnectAsync(cancellationToken);
            return stream;
        }
    }
}
