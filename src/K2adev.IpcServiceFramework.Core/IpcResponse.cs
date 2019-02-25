using System;

namespace K2adev.IpcServiceFramework
{
    [Serializable]
    public class IpcResponse
    {
        private IpcResponse(bool succeed, dynamic data, string failure)
        {
            Succeed = succeed;
            Data = data;
            Failure = failure;
        }

        public static IpcResponse Fail(string failure)
        {
            return new IpcResponse(false, null, failure);
        }

        public static IpcResponse Success(dynamic data)
        {
            return new IpcResponse(true, data, null);
        }

        public bool Succeed { get; }
        public dynamic Data { get; }
        public string Failure { get; }
    }
}
