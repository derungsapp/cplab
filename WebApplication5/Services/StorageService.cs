using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace WebApplication5.Services
{
    public class StorageService : IStorageService
    {
        private readonly AmazonS3Client _amazonS3Client;

        public StorageService(AmazonS3Client amazonS3Client)
        {
            _amazonS3Client = amazonS3Client;
        }

        public Task UploadFileAsync(MemoryStream memoryStream, string bucketName, string fileName, string folder)
        {
            throw new NotImplementedException();
        }

        public async Task UploadFileAsync(IFormFile formFile, string bucketName, string fileName, string folder)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = $"{folder}/{fileName}",
                BucketName = bucketName,
                CannedACL = S3CannedACL.PublicRead,

            };

            var fileTransferUtility = new TransferUtility(_amazonS3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);
        }
    }
}
