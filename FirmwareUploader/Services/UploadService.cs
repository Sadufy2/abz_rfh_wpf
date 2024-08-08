using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace FirmwareUploader.Services
{
    public static class UploadService
    {
        public static async Task UploadFileAsync(string filePath, string ipAddress)
        {
            string uploadUrl = $"http://{ipAddress}:5000/uploadFirmware";

            using (HttpClient client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
                HttpResponseMessage response = await client.PostAsync(uploadUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString());
                }
            }
        }
    }
}
