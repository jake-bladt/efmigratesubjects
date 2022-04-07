using System;
using System.Collections.Generic;
using System.Text;

using Amazon.DynamoDBv2.DataModel;

namespace efmigrate
{
    [DynamoDBTable("GallerySubjects")]
    public class Subject
    {
        [DynamoDBHashKey]
        public string FilePrefix { get; set; }
        [DynamoDBProperty]
        public string DisplayName { get; set; }
        [DynamoDBProperty]
        public int ImageCount { get; set; }
    }
}
