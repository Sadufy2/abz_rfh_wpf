using FirmwareUploader.Models;
using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace FirmwareUploader.Services
{
    public class LogService
    {
        readonly HttpClient _client;
        int port = 5000;
        public LogService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<List<DroneLog>> GetLogInfo(string ipAddress)
        {
            string url = $"http://{ipAddress}:{port}/getLogInfo";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            List<DroneLog> logs = JsonConvert.DeserializeObject<List<DroneLog>>(responseBody);
            return logs;
        }

        public async Task<string> DownloadLog(string ipAddress, int id)
        {
            string url = $"http://{ipAddress}:{port}/log/?id={id}";
            try
            {
                return await GetLog(url, id.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }
        public async Task<string> DownloadLog(string ipAddress, List<int> ids)
        {
            var queryString = string.Join("&", ids.Select(id => $"id={id}"));
            var url = $"http://{ipAddress}:{port}/log?{queryString}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync();
            var filePath = Path.Combine(GetDefaultDownloadFolder(), "logs.zip");
            int i = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(GetDefaultDownloadFolder(), $"logs_{i++}.zip");
            }
            await System.IO.File.WriteAllBytesAsync(filePath, content);

            return $"Logs downloaded successfully to {filePath}";
        }

        public async Task<string> DownloadLastLog(string ipAddress)
        {
            string url = $"http://{ipAddress}:{port}/getLastLog";
            try
            {
                return await GetLog(url, "last");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }

        async Task<string> GetLog(string url, string id)
        {
            string downloadFolder = GetDefaultDownloadFolder();

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contentStream = await response.Content.ReadAsStreamAsync();
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }
            string filePath = Path.Combine(downloadFolder, $"log_{id}.bin");

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            return $"File downloaded successfully to {filePath}";
        }
        static string GetDefaultDownloadFolder()
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadFolderPath = Path.Combine(userProfilePath, "Downloads");
            if (Directory.Exists(downloadFolderPath))
            {
                return downloadFolderPath;
            }
            else
            {
                throw new DirectoryNotFoundException("The Downloads folder could not be found.");
            }
        }
    }
}
