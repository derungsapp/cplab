using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using WebApplication5.Controllers;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AmazonDynamoDBClient _client;

        public StatisticsService(AmazonDynamoDBClient client)
        {
            _client = client;
        }

        public async Task RateAsync(string label, bool correct, string user)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                {"id", new AttributeValue(Guid.NewGuid().ToString())},
                {"label", new AttributeValue(label)},
                {"user", new AttributeValue(user)},
                {"correct", new AttributeValue(correct.ToString())}
            };

            await _client.PutItemAsync("statistics", item);
        }

        public async Task<Statistics> GetStatisticsAsync(string user = null)
        {
            var scanRequest = await _client.ScanAsync(new ScanRequest("statistics"));

            return new Statistics
            {
                UserStats =
                    new List<LabelStats>
                    {
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Human", user), CorrectRecords = CountCorrectRecords(scanRequest, "Human", user), Label = "Human"},
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Animal", user), CorrectRecords = CountCorrectRecords(scanRequest, "Animal", user), Label = "Animal"},
                        new LabelStats { TotalRecords = CountRecords(scanRequest, "Outdoors", user), CorrectRecords = CountCorrectRecords(scanRequest, "Outdoors", user), Label = "Outdoors" },
                        new LabelStats { TotalRecords = CountRecords(scanRequest, "Car", user), CorrectRecords = CountCorrectRecords(scanRequest, "Car", user), Label = "Car" },
                        new LabelStats { TotalRecords = CountRecords(scanRequest, "Undefined", user), CorrectRecords = CountCorrectRecords(scanRequest, "Undefined", user), Label = "Undefined" }
                    },
                GlobalStats = new List<LabelStats>
                {
                    new LabelStats {TotalRecords = CountRecords(scanRequest, "Human"), CorrectRecords = CountCorrectRecords(scanRequest, "Human"), Label = "Human"},
                    new LabelStats {TotalRecords = CountRecords(scanRequest, "Animal"), CorrectRecords = CountCorrectRecords(scanRequest, "Animal"), Label = "Animal"},
                    new LabelStats {TotalRecords = CountRecords(scanRequest, "Outdoors"), CorrectRecords = CountCorrectRecords(scanRequest, "Outdoors"), Label = "Outdoors"},
                    new LabelStats {TotalRecords = CountRecords(scanRequest, "Car"), CorrectRecords = CountCorrectRecords(scanRequest, "Car"), Label = "Car"},
                    new LabelStats {TotalRecords = CountRecords(scanRequest, "Undefined"), CorrectRecords = CountCorrectRecords(scanRequest, "Undefined"), Label = "Undefined"}
                }
            };
        }

        public int CountRecords(ScanResponse scanResponse, string label, string user = null)
        {
            var data = user == null ? scanResponse.Items.ToList() : scanResponse.Items.Where(p => p.Any(s => s.Key.ToString() == "user" && s.Value.S.ToString() == user)).ToList();
            return data.Count(p => p.Any(s => s.Key.ToString() == "label" && s.Value.S.ToString() == label));
        }

        public int CountCorrectRecords(ScanResponse scanResponse, string label, string user = null)
        {
            var data = user == null ? scanResponse.Items.ToList() : scanResponse.Items.Where(p => p.Any(s => s.Key.ToString() == "user" && s.Value.S.ToString() == user)).ToList();
            return data.Where(p => p.Any(s => s.Key.ToString() == "label" && s.Value.S.ToString() == label)).Count(p => p.Any(s => s.Key.ToString() == "correct" && s.Value.S.ToString() == "True"));
        }

    }
}
