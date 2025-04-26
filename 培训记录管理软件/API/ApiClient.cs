using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;
using TrainingRecordManager;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string _token;

    public ApiClient(string baseUrl = null)
    {   
        string apiUrl = baseUrl ?? ApiUrlManager.Instance.ApiUrl;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(apiUrl);
        // 设置默认请求头
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }



    private async Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, String json = null)
    {
        try
        {
            // 不再每次请求时修改BaseAddress，而是使用构造函数中设置的值
            var request = new HttpRequestMessage(method, endpoint);

            // 添加Authorization请求头
            if (!string.IsNullOrEmpty( TokenManager.Instance.GetToken()))
            {
                request.Headers.Add("Authorization", $"Bearer {TokenManager.Instance.GetToken()}");
            }

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"API请求失败: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";
                if (!string.IsNullOrEmpty(content))
                {
                    errorMessage += $"\n响应内容: {content}";
                }
                
                throw new HttpRequestException(errorMessage);
            }
            
            // 验证返回的内容是否为有效的JSON格式
            try
            {
                using (JsonDocument.Parse(content))
                {
                    return JsonSerializer.Deserialize<T>(content);
                }
            }
            catch (JsonException)
            {
                // 如果不是JSON格式，直接返回原始内容
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }
                return default(T);
            }
           
        }
        catch (Exception ex)
        {
           
            throw;
        }
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        return await SendRequestAsync<T>(HttpMethod.Get, endpoint);
    }

    public async Task<T> PostAsync<T>(string endpoint, string data)
    {
        return await SendRequestAsync<T>(HttpMethod.Post, endpoint, data);
    }

    public async Task<T> PutAsync<T>(string endpoint, string data)
    {
        return await SendRequestAsync<T>(HttpMethod.Put, endpoint, data);
    }

    public async Task<T> DeleteAsync<T>(string endpoint)
    {
        return await SendRequestAsync<T>(HttpMethod.Delete, endpoint);
    }
}