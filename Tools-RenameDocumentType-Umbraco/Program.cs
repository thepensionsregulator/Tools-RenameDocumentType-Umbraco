using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Configuration.Json;

namespace ToolsRenameDocumentTypeUmbraco
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json");

            var config = configuration.Build();

            var baseDirectory = GetConfigValueOrUserInput(config, "BaseDirectory");

            var originalDocTypeName = GetConfigValueOrUserInput(config, "OriginalDocTypeName");
            var originalDocTypeAlias = config["OriginalDocTypeAlias"];
            var originalDocTypeFileName = originalDocTypeName.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(originalDocTypeAlias))
            {
                originalDocTypeAlias = GenerateAlias(originalDocTypeName);
            }

            var newDocTypeName = GetConfigValueOrUserInput(config, "NewDocTypeName");
            var newDocTypeAlias = GenerateAlias(newDocTypeName);

            var uSyncDirectory = GetConfigValueOrUserInput(config, "uSyncDirectory");

            var files = Directory.GetFiles(baseDirectory, $"*{originalDocTypeFileName}*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileContent = File.ReadAllText(file);
                fileContent = fileContent.Replace(originalDocTypeFileName, newDocTypeName);

                if (file.Contains(".generated."))
                {
                    fileContent = fileContent.Replace(originalDocTypeAlias, newDocTypeAlias);
                }

                File.WriteAllText(file, fileContent);
                var newFileName = file.Replace(originalDocTypeFileName, newDocTypeName);
                File.Move(file, newFileName);
            }


            var uSyncContentTypeFiles = Directory.GetFiles($"{baseDirectory}{uSyncDirectory}/ContentTypes", "*", SearchOption.AllDirectories);

            Console.WriteLine($"{uSyncContentTypeFiles.Length} content types found, searching for {originalDocTypeName}");

            foreach (var file in uSyncContentTypeFiles)
            {
                var fileContent = File.ReadAllText(file);

                if (fileContent.Contains(originalDocTypeName))
                {
                    Console.WriteLine($"Found: {file}");
                    fileContent = fileContent
                        .Replace(originalDocTypeAlias, newDocTypeAlias)
                        .Replace(originalDocTypeName, newDocTypeName);

                    File.WriteAllText(file, fileContent);
                }
            }

            var uSyncContentFiles = Directory.GetFiles($"{baseDirectory}{uSyncDirectory}/Content", "*", SearchOption.AllDirectories);

            Console.WriteLine($"{uSyncContentFiles.Length} content files found, searching for content types that use {originalDocTypeName}");

            foreach(var file in uSyncContentFiles)
            {
                var fileContent = File.ReadAllText(file);

                if (fileContent.Contains($"<ContentType>{originalDocTypeAlias}</ContentType>"))
                {
                    fileContent = fileContent.Replace(originalDocTypeAlias, newDocTypeAlias);

                    File.WriteAllText(file, fileContent);
                }
            }
        }



        private static string GenerateAlias(string docTypeName)
        {
            return char.ToLower(docTypeName[0]) + docTypeName.Substring(1).Replace(" ", string.Empty);
        }

        private static string GetConfigValueOrUserInput(IConfigurationRoot config, string key)
        {
            var value = config[key];

            while (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"No value for {key} was found in appsettings.json, enter the value for {key}:");
                value = Console.ReadLine();
            }

            return value;
        }
    }
}
