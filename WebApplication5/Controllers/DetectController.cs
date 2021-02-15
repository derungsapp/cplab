using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetectController : ControllerBase
    {
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly IStorageService _storageService;

        public DetectController(IImageRecognitionService imageRecognitionService, IStorageService storageService)
        {
            _imageRecognitionService = imageRecognitionService;
            _storageService = storageService;
        }

        [HttpPost]
        public async Task<Result> Post()
        {
            var file = Request.Form.Files.First();
            var label = await _imageRecognitionService.DetectLabel(file);
            var id = Guid.NewGuid().ToString();
            await _storageService.UploadFileAsync(file, "test-bucket-robin", id, label);

            return new Result { Label = label, FileName = id, Confidence = 0.95 };
        }
    }
}
