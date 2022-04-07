using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace efmigrate
{
    class Program
    {

        private static AmazonDynamoDBClient _client;
        private static DynamoDBContext _context;

        static void Main(string[] args)
        {
            _client = new AmazonDynamoDBClient();
            _context = new DynamoDBContext(_client);
            var search = LoadSubjectsAsync().Result;
            var allSubjects = search.GetRemainingAsync().Result;

            allSubjects.ForEach(s => Console.WriteLine(s.FilePrefix));
        }

        static async Task<AsyncSearch<Subject>> LoadSubjectsAsync()
        {
            var c = new ScanCondition("FilePrefix", ScanOperator.IsNotNull);
            var conditions = new ScanCondition[] { c };
            return await Task.FromResult(_context.ScanAsync<Subject>(conditions));
        }
    }
}
