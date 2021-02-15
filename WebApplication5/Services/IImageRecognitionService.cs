using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApplication5.Services
{
    public interface IImageRecognitionService
    {
        Task<string> DetectLabel(IFormFile formFile);
    }
}
