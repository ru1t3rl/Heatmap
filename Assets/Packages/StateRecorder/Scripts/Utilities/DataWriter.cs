using System;
using System.IO;
using System.Threading.Tasks;
using Ru1t3rl.StateRecorder.Utilities.Json;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Utilities
{
    public class DataWriter : BaseDataHandler
    {
        private readonly string filePath;

        public DataWriter(string filePath)
        {
            this.filePath = filePath;
            ValidateFile(ref this.filePath, true);
            Debug.Log("Writing to file at path " + this.filePath);
        }

        public async Task OverwriteFileContent<T>(T value, params JsonConverter[] jsonConverts)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            using StreamWriter writer = new StreamWriter(fileStream);

            try
            {
                var json = JsonConvert.SerializeObject(value, Formatting.Indented, jsonConverts);
                await writer.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(DataWriter)}] {ex.Message}");
            }


            await writer.FlushAsync();
            writer.Close();
            await writer.DisposeAsync();
            fileStream.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
        }

        public async Task AppendToFile<T>(T value)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            using StreamWriter writer = new StreamWriter(fileStream);

            await writer.WriteAsync(JsonConvert.SerializeObject(value));

            await writer.FlushAsync();
            writer.Close();
            await writer.DisposeAsync();
            fileStream.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
        }

        public static async Task OverwriteFileContent<T>(string filePath, T value)
        {
            ValidateFile(ref filePath);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            using StreamWriter writer = new StreamWriter(fileStream);

            await writer.WriteAsync(JsonConvert.SerializeObject(value));

            await writer.FlushAsync();
            await writer.DisposeAsync();
            writer.Close();
            fileStream.Close();
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
        }
    }
}