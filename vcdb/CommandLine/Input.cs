using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace vcdb.CommandLine
{
    public class Input : IInput
    {
        private readonly Options options;
        private readonly JsonSerializer jsonSerializer;
        private readonly IHashHelper hashHelper;
        private string inputCache;

        public Input(Options options, JsonSerializer jsonSerializer, IHashHelper hashHelper)
        {
            this.options = options;
            this.jsonSerializer = jsonSerializer;
            this.hashHelper = hashHelper;
        }

        public string GetHash(int hashSize)
        {
            if (inputCache == null)
                throw new InvalidOperationException("The input must be read first");

            return hashHelper.GetHash(inputCache, hashSize);
        }

        public async Task<T> Read<T>()
        {
            if (string.IsNullOrEmpty(options.InputFile))
            {
                if (Console.IsInputRedirected)
                    return await ReadFromStream<T>(Console.In);

                return default;
            }

            return await ReadFromStream<T>(new StreamReader(Path.Combine(options.WorkingDirectory, options.InputFile)));
        }

        public TextReader GetSiblingContent(string fileName)
        {
            if (Console.IsInputRedirected)
            {
                return ReadContentFromFileInWorkingDirectory(fileName)
                    ?? throw new FileNotFoundException($"Requested file could not be found in working directory ({options.WorkingDirectory})", fileName);
            }

            return ReadContentFromFileInSameDirectoryAsInputFile(fileName)
                ?? ReadContentFromFileInWorkingDirectory(fileName)
                ?? throw new FileNotFoundException($"Requested file could not be found as a sibling of the input json ({Path.GetFullPath(options.InputFile)}) or in the working directory ({options.WorkingDirectory})", fileName);
        }

        private TextReader ReadContentFromFileInSameDirectoryAsInputFile(string fileName)
        {
            var fullInputFilePath = Path.GetFullPath(options.InputFile);
            var inputPathDirectory = Path.GetDirectoryName(fullInputFilePath);
            return ReadContentFromFileIfExists(Path.Combine(inputPathDirectory, fileName));
        }

        private TextReader ReadContentFromFileInWorkingDirectory(string fileName)
        {
            return ReadContentFromFileIfExists(Path.Combine(options.WorkingDirectory, fileName));
        }

        private TextReader ReadContentFromFileIfExists(string filePath)
        {
            return File.Exists(filePath)
                ? new StreamReader(filePath)
                : null;
        }

        private Task<T> ReadFromStream<T>(TextReader inputStreamReader)
        {
            return Task.Run(() =>
            {
                inputCache = inputStreamReader.ReadToEnd();

                using (var jsonReader = new JsonTextReader(new StringReader(inputCache)))
                    return jsonSerializer.Deserialize<T>(jsonReader);
            });
        }
    }
}
