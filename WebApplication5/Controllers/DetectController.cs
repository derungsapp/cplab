using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace WebApplication5.Controllers
{

    public class Result
    {
        public string Label { get; set; }
        public double Confidence { get; set; }
        public string FileName { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DetectController : ControllerBase
    {

        public List<string> AllowedLabels = new List<string>() { "Animal", "Car", "Human", "Outdoors" };

        [HttpPost]
        public async Task<Result> Post()
        {
            var file = Request.Form.Files.First();
            var folder = await DetectImageLabel(file);
            var id = Guid.NewGuid().ToString();
            await UploadImage(file, folder, id);

            return new Result() { Label = folder, FileName = id, Confidence = 0.95 };

        }


        public async Task UploadImage(IFormFile file, string folder, string id)
        {
            var credentials = new BasicAWSCredentials("AKIAIPUTIIF6B7ECFVAA", "WzYtZCvQzSisqJLK2MeLuonyStekec962UjWKWAP");
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
            };
            using var client = new AmazonS3Client(credentials, config);

            await using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = $"{folder}/{id}",
                BucketName = "test-bucket-robin",
                CannedACL = S3CannedACL.PublicRead,

            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);

        }

        public async Task<string> DetectImageLabel(IFormFile formFile)
        {
            var credentials = new BasicAWSCredentials("AKIAIPUTIIF6B7ECFVAA", "WzYtZCvQzSisqJLK2MeLuonyStekec962UjWKWAP");
            var config = new AmazonRekognitionConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
            };

            using var rekognitionClient = new AmazonRekognitionClient(credentials, config);
            await using var newMemoryStream = new MemoryStream();
            await formFile.CopyToAsync(newMemoryStream);

            var request = new DetectLabelsRequest
            {
                Image = new Image { Bytes = newMemoryStream },

            };


            var response = await rekognitionClient.DetectLabelsAsync(request);

            var filteredLabels = new List<Label>();

            foreach (var label in response.Labels)
            {
                if (AllowedLabels.Contains(label.Name))
                {
                    filteredLabels.Add(label);
                }
            }

            var finalLabel = new Label { Name = "undefined" };

            if (filteredLabels.Any())
            {
                finalLabel = filteredLabels.OrderByDescending(p => p.Confidence).First();
            }

            Console.WriteLine($"Detected {finalLabel.Name} with confidence {finalLabel.Confidence}");

            return finalLabel.Name;
        }

    }
}
