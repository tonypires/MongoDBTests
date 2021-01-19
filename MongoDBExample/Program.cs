using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBExample
{
    class Program
    {
        private static readonly MongoClient Client = new MongoClient("mongodb://localhost:27017");

        static void Main(string[] args)
        {
            BsonClassMap.RegisterClassMap<File>();

            // Connect + Get DB
            var database = Client.GetDatabase("foo");
            var collection = database.GetCollection<BsonDocument>("bar");

            // Get + Save Bson Test Data
            var testBson = Seed.GetTestBsonDocument();
            collection.InsertOne(testBson);

            // Get + Save Test C# Poco
            var testPoco = Seed.GetTestPoco();
            var pocoBsonConverted = testPoco.ToBsonDocument();
            collection.InsertOneAsync(pocoBsonConverted);

            // Get + Save Test C# Complex Poco
            var complexPoco = Seed.GetComplexTestPoco();
            var complexPocoConverted = complexPoco.ToBsonDocument();
            collection.InsertOne(complexPocoConverted);
        }

        class Seed
        {
            public static BsonDocument GetTestBsonDocument()
            {
                return new BsonDocument
                {
                    { "name", "MongoDB" },
                    { "type", "Database" },
                    { "count", 1 },
                    { "info", new BsonDocument
                        {
                            { "x", 203 },
                            { "y", 102 }
                        }
                    }
                };
            }

            public static File GetTestPoco()
            {
                return new File
                {
                    Label = "This is a test",
                    IsActive = true,
                    Version = 1
                };
            }

            public static File GetComplexTestPoco()
            {
                return new File
                {
                    Label = "This is a test for a complex object",
                    IsActive = true,
                    Version = 1,
                    Items = GenerateItems()
                };
            }

            private static IList<FileItem> GenerateItems()
            {
                var items = new List<FileItem>();

                var item1 = new FileItem
                {
                    Label = "Item1"
                };
                var childItem = new FileItem
                {
                    Label = "ChildItem1"
                };
                var item1Node = new FileNode
                {
                    Items = new List<FileItem>() { item1 },
                    Parent = null
                };
                var childNode = new FileNode
                {
                    Items = new List<FileItem> { childItem },
                    Parent = item1Node
                };

                item1.Node = item1Node;
                childItem.Node = childNode;

                items.Add(item1);
                items.Add(childItem);

                return items;
            }
        }

        private class File
        {
            public string Label { get; set; }
            public int Version { get; set; }
            public bool IsActive { get; set; }
            public ICollection<FileItem> Items { get; set; }
        }

        class FileNode
        {
            public FileNode Parent { get; set; }
            public ICollection<FileItem> Items { get; set; }
        }

        class FileItem
        {
            public string Label { get; set; }
            public FileNode Node { get; set; }
        }

        static void BulkWriteExample(IMongoCollection<BsonDocument> collection)
        {
            var models = new WriteModel<BsonDocument>[]
            {  
                new InsertOneModel<BsonDocument>(new BsonDocument("_id", 4)),
                new InsertOneModel<BsonDocument>(new BsonDocument("_id", 5)),
                new InsertOneModel<BsonDocument>(new BsonDocument("_id", 6)),
                new UpdateOneModel<BsonDocument>(
                    new BsonDocument("_id", 1),
                    new BsonDocument("$set", new BsonDocument("x", 2))),
                new DeleteOneModel<BsonDocument>(new BsonDocument("_id", 3)),
                new ReplaceOneModel<BsonDocument>(
                    new BsonDocument("_id", 3),
                    new BsonDocument("_id", 3).Add("x", 4))
            };

            // 1. Ordered bulk operation - order of operation is guaranteed
            collection.BulkWrite(models);

            // 2. Unordered bulk operation - no guarantee of order of operation
            collection.BulkWrite(models, new BulkWriteOptions { IsOrdered = false });

            // 1. Ordered bulk operation - order of operation is guaranteed
            //await collection.BulkWriteAsync(models);

            // 2. Unordered bulk operation - no guarantee of order of operation
            //await collection.BulkWriteAsync(models, new BulkWriteOptions { IsOrdered = false });
        }
    }
}
