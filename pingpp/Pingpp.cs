using System;
using Newtonsoft.Json;
using System.IO;
using Pingpp.Exception;

namespace Pingpp
{
    public abstract partial class Pingpp
    {
        public string ApiVersion { get; set; }
        public string AcceptLanguage { get; set; } = "zh-CN";
        public string ApiBase { get; set; } = "https://api.pingxx.com";
        public string Version { get; set; } = "1.4.5";
        public bool BadGateWayMatch { get; set; } = true;
        public int MaxNetworkRetries { get; set; } = 1;
        protected int MaxRetry { get; set; } = 0;
        public int DefaultTimeout { get; set; } = 80000;
        public int DefaultReadAndWriteTimeout { get; set; } = 20000;
        public string ApiKey { get; set; }
        public byte[] PrivateKey { get; private set; }

        public void SetMaxNetworkRetries(int maxNetworkRetries)
        {
            MaxNetworkRetries = maxNetworkRetries;
        }

        public void SetBadGateWayMatch(bool badGateWayMatch)
        {
            BadGateWayMatch = badGateWayMatch;
        }

        public void SetApiBase(string newApiBase)
        {
            ApiBase = newApiBase;
        }
        internal string GetApiKey()
        {
            return ApiKey;
        }

        public void SetApiKey(string newApiKey)
        {
            ApiKey = newApiKey;
        }

        internal byte[] GetPrivateKey()
        {
            return PrivateKey;
        }

        public void SetPrivateKey(string newPrivateKey)
        {
            PrivateKey = FormatPrivateKey(newPrivateKey);
        }

        public void SetPrivateKeyPath(string newPrivateKeyPath)
        {
            try
            {
                var privateKeyFile = new FileStream(newPrivateKeyPath, FileMode.Open);
                PrivateKey = FormatPrivateKey((new StreamReader(privateKeyFile)).ReadToEnd());
                privateKeyFile.Close();
            }
            catch (IOException ex)
            {
                throw new PingppException("Private key read error. " + ex);
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        private byte[] FormatPrivateKey(string privateKeyContent)
        {
            privateKeyContent = privateKeyContent.Replace("\r", "").Replace("\n", "");
            if (privateKeyContent.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                privateKeyContent = privateKeyContent.Substring(31);
            }
            if (privateKeyContent.EndsWith("-----END RSA PRIVATE KEY-----"))
            {
                privateKeyContent = privateKeyContent.Substring(0, privateKeyContent.Length - 29);
            }
            var privateKeyData = Convert.FromBase64String(privateKeyContent);
            if (privateKeyData.Length < 162)
            {
                throw new PingppException("Private key content is incorrect.");
            }

            return privateKeyData;
        }
    }
}
