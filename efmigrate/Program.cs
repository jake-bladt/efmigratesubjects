using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text;

namespace efmigrate
{
    class Program
    {

        private static AmazonDynamoDBClient _client;
        private static DynamoDBContext _context;

        static void Main(string[] args)
        {
            var appConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var yearbookPath = appConfig.GetRequiredSection("yearbookPath").Get<String>();
            var subjectRoot = appConfig.GetRequiredSection("subjectRoot").Get<String>();

            _client = new AmazonDynamoDBClient();
            _context = new DynamoDBContext(_client);
            var search = LoadSubjectsAsync().Result;
            
            var allDbSubjects = search.GetRemainingAsync().Result;
            var allFileSubjects = GetAllFileSystemSubjects(yearbookPath, subjectRoot);

            allFileSubjects.ForEach(s => {
                if(!allDbSubjects.Any(d => d.FilePrefix == s.FilePrefix))
                {
                    Console.WriteLine($"Writing {s.DisplayName} to database.");
                    var wRet = _context.SaveAsync<Subject>(s);
                }
            });
        }

        static async Task<AsyncSearch<Subject>> LoadSubjectsAsync()
        {
            var c = new ScanCondition("FilePrefix", ScanOperator.IsNotNull);
            var conditions = new ScanCondition[] { c };
            return await Task.FromResult(_context.ScanAsync<Subject>(conditions));
        }

        static List<Subject> GetAllFileSystemSubjects(string path, string subjectRoot)
        {
            var ret = new List<Subject>();

            var dir = new DirectoryInfo(path);
            var allJpgs = dir.GetFiles("*.jpg");

            allJpgs.ToList<FileInfo>().ForEach(fi =>
            {
                var prefix = fi.Name.Replace(".jpg", String.Empty);
                var dName = PrefixToDisplayName(prefix);
                var subjectPath = Path.Combine(subjectRoot, prefix);
                var imgCount = GetImageCountFromFilesystem(subjectPath);

                var subject = new Subject { 
                    FilePrefix = prefix, 
                    DisplayName = dName,
                    ImageCount = imgCount
                };
                ret.Add(subject);
            });

            return ret;

        }

        static string PrefixToDisplayName(string prefix)
        {
            var ret = new StringBuilder();
            var capNext = true;
            var prefixArray = prefix.ToArray<char>();

            for(int i = 0; i < prefix.Length; i++)
            {
                var ch = prefixArray[i];
                switch(ch)
                {
                    case '.':
                        ret.Append(" ");
                        capNext = true;
                        break;
                    case '-':
                        ret.Append(ch);
                        capNext = true;
                        break;
                    default:
                        var cs = capNext ? ch.ToString().ToUpper() : ch.ToString();
                        ret.Append(cs);
                        capNext = false;
                        break;
                }

            }

            return ret.ToString();
        }

        static int GetImageCountFromFilesystem(string subjectPath)
        {
            if (Directory.Exists(subjectPath))
            {
                var di = new DirectoryInfo(subjectPath);
                var imgs = di.GetFiles(".jpg");
                return imgs.Length;
            }
            else
            {
                return -1;
            }
        }

    }
}
