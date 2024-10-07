using Firebase.Storage;

namespace HandmadeProductManagement.Services.Service
{
    public class UploadImageService
    {
        private readonly string _bucketName = "handmade-product-bde7a.appspot.com";

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; 

                string[] data = fileName.Split('.');
                string mediaType = data.Length > 1 ? data[1] : "jpg"; 
                fileName = Guid.NewGuid().ToString() + "." + mediaType; 

                var task = new FirebaseStorage(_bucketName)
                    .Child($"product-Image/{fileName}")
                    .PutAsync(memoryStream);

                var downloadUrl = await task; 

                return downloadUrl; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to Firebase: {ex.Message}");
            }
        }

    }
}
