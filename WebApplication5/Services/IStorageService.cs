using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApplication5.Services
{
    public interface IStorageService
    {
        Task UploadFileAsync(MemoryStream memoryStream, string bucketName, string fileName, string folder);
        Task UploadFileAsync(IFormFile formFile, string bucketName, string fileName, string folder);

    }
}
