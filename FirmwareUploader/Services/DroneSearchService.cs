using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUploader
{
    public class DroneSearchService
    {
        ConcurrentBag<ScannedDrone> scannedDrones;
        public DroneSearchService()
        {
            scannedDrones = new ConcurrentBag<ScannedDrone>();
        }

        public async Task<List<ScannedDrone>> StartScanning()
        {
            string sendUrl = "http://192.168.1.{0}:5000";
            string content = "droneCheck";

            scannedDrones.Clear();
            Task[] tasks = new Task[30];
            for (int i = 0; i < tasks.Length; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() => SendMessagesAsync(sendUrl, threadId, content));
            }
            await Task.WhenAll(tasks);
            return scannedDrones.ToList();
        }

        async Task SendMessagesAsync(string urlFormat, int threadIdx, string content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(1);
                string url = string.Format(urlFormat, threadIdx.ToString());

                StringContent httpContent = new StringContent(content, Encoding.UTF8, "text/plain");
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent.StartsWith("droneName="))
                        {
                            string droneName = responseContent.Substring("droneName=".Length);
                            scannedDrones.Add(new ScannedDrone("192.168.1." + threadIdx.ToString(), droneName));
                        }
                        else
                        {
                            throw new Exception(responseContent);
                        }
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
