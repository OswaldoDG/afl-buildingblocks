namespace ClientPOC.services;

using ClientPOC.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;


public class CloudService(IConfigurationRoot configuration, HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string descopeCredentials = configuration["AppSettings:DescopeCredentials"];

    public async Task<string> DescopeLogin(string url, object data)
    {

        try
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + descopeCredentials);

            var response = await _httpClient.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                string sessionJwt = JsonSerializer.Deserialize<JsonElement>(responseContent).GetProperty("sessionJwt").GetString();
                return sessionJwt.TrimStart('{').TrimEnd('}');

            }
            else
            {
                throw new Exception($"Error en la respuesta del servidor: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al realizar la solicitud POST a {url}", ex);
        }
    }



    public async Task<BatchResponseDto> CreateBatch(string url, string jwt , object data)
    {

        try
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}" );

            var response = await _httpClient.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var batchResponse = Newtonsoft.Json.JsonConvert.DeserializeObject <BatchResponseDto>(responseContent);
                return batchResponse;

            }
            else
            {
                throw new Exception($"Error en la respuesta del servidor: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al realizar la solicitud POST a {url}", ex);
        }
    }


    public async Task CompleteBatch(string url, BatchCompleteRequestDto batchComplete , string jwt)
    {

        try
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

            var response = await _httpClient.PostAsJsonAsync(url, batchComplete);
            response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error en la respuesta del servidor: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al realizar la solicitud POST a {url}", ex);
        }
    }

    public async Task UploadToGoogle(BatchResponseDto batchResponse, BatchRequestDto batchRequest, int bathcSize)
    {
        DateTime now = DateTime.Now;
        this._httpClient.DefaultRequestHeaders.Remove("Authorization");
        List<Task<HttpResponseMessage>> puts = [];

        int batch = 1;
        foreach (var item in batchResponse.Items)
        {
            var r = batchRequest.Items.FirstOrDefault(f => f.ItemId == item.ItemId);

            //using var content = new ByteArrayContent(File.ReadAllBytes(r.FileName));
            //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //await _httpClient.PutAsync(item.UploadUrl, content);

            if (r != null && File.Exists(r.FileName))
            {
                var content = new ByteArrayContent(File.ReadAllBytes(r.FileName));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                puts.Add(_httpClient.PutAsync(item.UploadUrl, content));
            }

            if (puts.Count >= bathcSize)
            {
                HttpResponseMessage[] responses = await Task.WhenAll(puts);
                puts.Clear();


                FileService.LogTimeDiff($"Upload {batch}", now);
                now = DateTime.Now;
                batch++;
            }
        }

        if (puts.Count > 0)
        {
            HttpResponseMessage[] responses = await Task.WhenAll(puts);
            puts.Clear();
            FileService.LogTimeDiff($"Upload {batch}", now);
        }
    }
}
