using System;

namespace TrainingRecordManager
{
    public class TokenManager
    {
        private static TokenManager _instance;
        private static readonly object _lock = new object();
        private string _token;

        private TokenManager() { }

        public static TokenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TokenManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token不能为空");
            }
            _token = token;
        }

        public string GetToken()
        {
            return _token;
        }

        public bool HasToken()
        {
            return !string.IsNullOrEmpty(_token);
        }

        public void ClearToken()
        {
            _token = null;
        }
    }
}