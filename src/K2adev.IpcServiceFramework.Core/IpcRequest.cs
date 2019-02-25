using System;

namespace K2adev.IpcServiceFramework
{
    [Serializable]
    public class IpcRequest
    {
        public string MethodName { get; set; }
        public dynamic[] Parameters { get; set; }
    }
}