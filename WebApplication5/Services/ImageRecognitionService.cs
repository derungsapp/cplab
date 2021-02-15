using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;

namespace WebApplication5.Services
{
    public class ImageRecognitionService : IImageRecognitionService
    {
        private readonly AmazonRekognitionClient _amazonRekognitionClient;

        public ImageRecognitionService(AmazonRekognitionClient amazonRekognitionClient)
        {
            _amazonRekognitionClient = amazonRekognitionClient;
        }

        public List<string> AllowedLabels = new List<string>() { "Animal", "Car", "Human", "Outdoors", "Undefined" };

        public async Task<string> DetectLabel(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            var request = new DetectLabelsRequest
            {
                Image = new Image { Bytes = memoryStream },
            };

            var response = await _amazonRekognitionClient.DetectLabelsAsync(request);

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
