using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Device;

namespace Ru1t3rl.StateRecorder.Utilities
{
    public class DataReader : BaseDataHandler
    {
        private string _filePath;

        public DataReader(string filePath)
        {
            _filePath = filePath;
            ValidateFile(ref _filePath);
        }

        public async Task<string> ReadAll()
        {
            var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(fileStream);

            var content = await reader.ReadToEndAsync();

            reader.Dispose();
            reader.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
            fileStream.Close();

            return content;
        }

        public async Task<T> ReadFromJsonAsync<T>()
        {
            var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(fileStream);

            var content = await reader.ReadToEndAsync();
            T result = JsonConvert.DeserializeObject<T>(content);

            reader.Dispose();
            reader.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
            fileStream.Close();

            return result;
        }

        public static async Task<T> ReadFromJsonAsync<T>(string filePath)
        {
            ValidateFile(ref filePath);

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(fileStream);

            var content = await reader.ReadToEndAsync();
            T result = JsonConvert.DeserializeObject<T>(content);

            reader.Dispose();
            reader.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
            fileStream.Close();

            return result;
        }
    }
}