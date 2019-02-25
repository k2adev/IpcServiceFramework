using System.IO;
using System.IO.Compression;

namespace K2adev.IpcServiceFramework.Services
{
    public static class GZipCompressor
    {
        public static byte[] Compress(byte[] data)
        {
            byte[] res = null;
            MemoryStream compressedStream = new MemoryStream();
            GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            res = compressedStream.ToArray();
            return res;
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] res = null;
            MemoryStream compressedStream = new MemoryStream(data);

            using (GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                using (MemoryStream resultStream = new MemoryStream())
                {
                    zipStream.CopyTo(resultStream);
                    res = resultStream.ToArray();
                }
            }

            return res;
        }
    }
}
