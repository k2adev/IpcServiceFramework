namespace K2adev.IpcServiceFramework
{
    public interface IIpcMessageSerializer
    {
        IpcServiceOptions Options { get; set; }

        byte[] SerializeRequest(IpcRequest request);
        IpcResponse DeserializeResponse(byte[] binary);
        IpcRequest DeserializeRequest(byte[] binary);
        byte[] SerializeResponse(IpcResponse response);
    }
}
