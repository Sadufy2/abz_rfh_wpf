using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUploader.Services
{
    public class ConfigService
    {
        string URL { get; set; }
        public ConfigService(string ipAddress)
        {
            URL = $"http://{ipAddress}:5000/config";
        }

        public async Task<string> GetConfig()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(URL);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return responseContent;
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
                catch (Exception e) 
                {
                    return e.Message;
                }
            }
        }
        public async Task<string> PutConfig(string config)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(config, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync(URL, content);
                    response.EnsureSuccessStatusCode();

                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                catch (Exception e) {
                    return e.Message;
                }
            }
        }
    }
}
