using System.Security.Cryptography;
using System.Text;

namespace K2adev.IpcServiceFramework
{
    public class IpcServiceOptions
    {
        public bool GZipCompressionEnabled { get; set; } = false;
        public bool Aes256EncryptionEnabled { get; set; } = false;
        public byte[] Aes256Password { get; private set; }
        public string Aes256Passwod
        {
            set
            {
                var mySHA256 = SHA256.Create();
                Aes256Password = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
        public int ThreadCount { get; set; } = 4;
    }
}
