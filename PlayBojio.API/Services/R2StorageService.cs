using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace PlayBojio.API.Services;

public interface IR2StorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
}

public class R2StorageService : IR2StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public R2StorageService(IConfiguration configuration)
    {
        var accessKey = configuration["R2:AccessKeyId"];
        var secretKey = configuration["R2:SecretAccessKey"];
        var accountId = configuration["R2:AccountId"];
        _bucketName = configuration["R2:BucketName"] ?? throw new InvalidOperationException("R2:BucketName is required");
        _publicUrl = configuration["R2:PublicUrl"] ?? $"https://{_bucketName}.{accountId}.r2.cloudflarestorage.com";

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(accountId))
        {
            throw new InvalidOperationException("R2 credentials are not configured properly");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = false,
            UseHttp = false
        };

        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var key = $"uploads/{DateTime.UtcNow:yyyy/MM}/{uniqueFileName}";

            // Read stream into memory to avoid R2 streaming signature issues
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = memoryStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead,
                DisablePayloadSigning = true // Fix for R2 compatibility
            };

            await _s3Client.PutObjectAsync(request);

            // Return public URL
            return $"{_publicUrl}/{key}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file to R2: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Extract key from URL
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

