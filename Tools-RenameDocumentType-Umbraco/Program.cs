using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Umbraco.Doctype.Renamer
{
    internal class Program
    {
        private const string BASE_DIRECTORY = "BaseDirectory";
        private const string ORIGINAL_DOC_TYPE_NAME = "OriginalDocTypeName";
        private const string ORIGINAL_DOC_TYPE_ALIAS = "OriginalDocTypeAlias";
        private const string NEW_DOC_TYPE_NAME = "NewDocTypeName";
        private const string USYNC_DIRECTORY = "uSyncDirectory";
        private const string IGNORE_CASE = "IgnoreCase";
        private const string CULTURE_INFO = "CultureInfo";

        static void Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json");

            var config = configuration.Build();

            var baseDirectory = GetConfigValueOrUserInput(config, BASE_DIRECTORY);

            var originalDocTypeName = GetConfigValueOrUserInput(config, ORIGINAL_DOC_TYPE_NAME);
            var originalDocTypeAlias = config[ORIGINAL_DOC_TYPE_ALIAS];
            var originalDocTypeFileName = originalDocTypeName.Replace(" ", string.Empty);

            if (bool.TryParse(config[IGNORE_CASE], out var ignoreCase) == false)
            {
                Console.WriteLine("No value for 'IgnoreCase' found in appsettings.json, defaulting to true");
                ignoreCase = true;
            }


            var cultureInfo = new CultureInfo(config[CULTURE_INFO] ?? "en-GB");

            if (string.IsNullOrEmpty(originalDocTypeAlias))
            {
                originalDocTypeAlias = GenerateAlias(originalDocTypeName);
            }

            var newDocTypeName = GetConfigValueOrUserInput(config, NEW_DOC_TYPE_NAME);
            var newDocTypeAlias = GenerateAlias(newDocTypeName);
            var newDocTypeFileNamePrefix = GenerateFileNamePrefix(newDocTypeName);

            var uSyncDirectory = GetConfigValueOrUserInput(config, USYNC_DIRECTORY);

            var files = Directory.GetFiles(baseDirectory, $"*{originalDocTypeFileName}*", SearchOption.AllDirectories);

            Console.WriteLine($"{files.Length} found");

            foreach (var file in files)
            {
                Console.WriteLine($"{file} will be updated");
                var fileContent = File.ReadAllText(file);

                fileContent = fileContent.Replace(originalDocTypeAlias, newDocTypeFileNamePrefix, ignoreCase, cultureInfo);

                if (file.Contains(".generated."))
                {
                    fileContent = fileContent.Replace(originalDocTypeAlias, newDocTypeAlias, ignoreCase, cultureInfo);
                }


                File.WriteAllText(file, fileContent);

                var newFileName = file.Replace(originalDocTypeFileName, newDocTypeFileNamePrefix, ignoreCase, cultureInfo);
                Console.WriteLine($"Renaming {file} to {newFileName}");
                File.Move(file, newFileName);
            }

            var uSyncContentTypeFiles = Directory.GetFiles($"{baseDirectory}{uSyncDirectory}/ContentTypes", "*", SearchOption.AllDirectories);
            Console.WriteLine($"{uSyncContentTypeFiles.Length} content types found, searching for {originalDocTypeName}");
            ReplaceForUSyncFiles(uSyncContentTypeFiles, originalDocTypeAlias, originalDocTypeName, newDocTypeAlias, newDocTypeName);

            var uSyncContentFiles = Directory.GetFiles($"{baseDirectory}{uSyncDirectory}/Content", "*", SearchOption.AllDirectories);
            Console.WriteLine($"{uSyncContentFiles.Length} content files found, searching for content that uses the {originalDocTypeName} document type");
            ReplaceForUSyncFiles(uSyncContentFiles, originalDocTypeAlias, originalDocTypeName, newDocTypeAlias, newDocTypeName);


        }

        private static void ReplaceForUSyncFiles(IEnumerable<string> files, string originalDocTypeAlias, string originalDocTypeName, string newDocTypeAlias, string newDocTypeName)
        {
            foreach (var file in files)
            {
                var fileContent = File.ReadAllText(file);

                if (fileContent.Contains(originalDocTypeName) || fileContent.Contains(originalDocTypeAlias))
                {
                    fileContent = fileContent
                            .Replace(originalDocTypeAlias, newDocTypeAlias)
                            .Replace(originalDocTypeName, newDocTypeName);

                    File.WriteAllText(file, fileContent);
                }
            }
        }

        private static string GenerateAlias(string docTypeName)
        {
            return char.ToLower(docTypeName[0]) + docTypeName.Substring(1).Replace(" ", string.Empty);
        }

        private static string GenerateFileNamePrefix(string docTypeName)
        {
            return docTypeName.Replace(" ", string.Empty);
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
