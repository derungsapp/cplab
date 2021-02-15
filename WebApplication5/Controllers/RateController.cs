using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication5.Controllers
{
    public class LabelStats
    {
        public string Label { get; set; }
        public int TotalRecords { get; set; }
        public int CorrectRecords { get; set; }
        public int WrongRecords => TotalRecords - CorrectRecords;
    }

    public class Statistic
    {
        public List<LabelStats> UserStats { get; set; }
        public List<LabelStats> GlobalStats { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RateController : ControllerBase
    {

        // POST api/<RateController>
        [HttpPost]
        public async Task<Statistic> Post()
        {
            var userName = User?.Claims?.FirstOrDefault(p => p.Type == "cognito:username")?.Value;
            var correct = Convert.ToBoolean(Request.Form["correct"]);
            var label = Request.Form["label"].ToString();
            var credentials = new BasicAWSCredentials("AKIAIAV7QE63K7EHOVBQ", "wEoegCfvNQ0i5vF73PRxWaiqssYp5nlZclya9/JU");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast2
            };

            using var dynamoClient = new AmazonDynamoDBClient(credentials, config);

            var item = new Dictionary<string, AttributeValue>
            {
                {"id", new AttributeValue(Guid.NewGuid().ToString())},
                {"label", new AttributeValue(label)},
                {"user", new AttributeValue(userName)},
                {"correct", new AttributeValue(correct.ToString())}
            };

            await dynamoClient.PutItemAsync("statistics", item);

            var scanRequest = await dynamoClient.ScanAsync(new ScanRequest("statistics"));
            
            return new Statistic
            {
                UserStats =
                    new List<LabelStats>
                    {
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Human", userName), CorrectRecords = CountCorrectRecords(scanRequest, "Human", userName), Label = "Human"},
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Animal", userName), CorrectRecords = CountCorrectRecords(scanRequest, "Animal", userName), Label = "Animal"},
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Outdoors", userName), CorrectRecords = CountCorrectRecords(scanRequest, "Outdoors", userName), Label = "Outdoors"},
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Car", userName), CorrectRecords = CountCorrectRecords(scanRequest, "Car", userName), Label = "Car"},
                        new LabelStats {TotalRecords = CountRecords(scanRequest, "Undefined", userName), CorrectRecords = CountCorrectRecords(scanRequest, "Undefined", userName), Label = "Undefined"}
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
