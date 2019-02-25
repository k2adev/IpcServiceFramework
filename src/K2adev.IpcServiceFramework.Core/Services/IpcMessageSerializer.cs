using K2adev.IpcServiceFramework.Services;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace K2adev.IpcServiceFramework
{
    public class IpcMessageSerializer : IIpcMessageSerializer
    {
        public IpcServiceOptions Options { get; set; }

        public IpcRequest DeserializeRequest(byte[] binary)
        {
            return Deserialize<IpcRequest>(binary);
        }

        public IpcResponse DeserializeResponse(byte[] binary)
        {
            return Deserialize<IpcResponse>(binary);
        }

        public byte[] SerializeRequest(IpcRequest request)
        {
            return Serialize(request);
        }

        public byte[] SerializeResponse(IpcResponse response)
        {
            return Serialize(response);
        }

        private byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            byte[] result;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    result = ms.ToArray();
                }

                if (Options.GZipCompressionEnabled)
                    result = GZipCompressor.Compress(result);

                if (Options.Aes256EncryptionEnabled)
                    result = AesEncryption.Encrypt(result, "test");

                return result;
            }
            catch
            {
                return null;
            }
        }

        private dynamic Deserialize<T>(byte[] binary)
        {
            try
            {
                byte[] request = (Options.Aes256EncryptionEnabled)
                                     ? AesEncryption.Decrypt(binary, "test")
                                     : binary;

                request = (Options.GZipCompressionEnabled)
                                    ? GZipCompressor.Decompress(request)
                                    : request;

                

                BinaryFormatter binForm = new BinaryFormatter();
                using (MemoryStream memStream = new MemoryStream())
                {
                    memStream.Write(request, 0, request.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    return binForm.Deserialize(memStream);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}