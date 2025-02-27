using System;

namespace TrainingRecordManager
{
    public class ApiUrlManager
    {
        private static readonly ApiUrlManager _instance = new ApiUrlManager();
        private string _apiUrl = "http://192.168.0.107:5115"; // 默认API地址

        private ApiUrlManager() { }

        public static ApiUrlManager Instance
        {
            get { return _instance; }
        }

        public string ApiUrl
        {
            get { return _apiUrl; }
            set { _apiUrl = value; }
        }
        public void SaveApiUrl(string apiUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                throw new ArgumentException("API地址不能为空");

            _apiUrl = apiUrl;
        }
    }
}