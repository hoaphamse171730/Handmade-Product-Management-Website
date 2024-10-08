using Firebase.Storage;

namespace HandmadeProductManagement.Services.Service
{
    public class ManageFirebaseImageService
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

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                var storagePath = GetStoragePathFromUrl(fileUrl);
                Console.WriteLine($"Storage Path: {storagePath}");
                await new FirebaseStorage(_bucketName)
                    .Child(storagePath)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file from Firebase: {ex.Message}");
            }
        }

        private string GetStoragePathFromUrl(string fileUrl)
        {

            var uri = new Uri(fileUrl);
            var segments = uri.Segments;

            // Kiểm tra số lượng segments, dự kiến có ít nhất 3 segments
            if (segments.Length >= 3)
            {
                // Tạo storagePath từ segments từ thứ 3 trở đi
                // Chúng ta cần lấy lại đường dẫn từ segment thứ 3 trở đi
                string storagePath = string.Join("", segments.Skip(5)); // Bỏ qua những phần đầu

                // Xóa các tham số truy vấn, chỉ giữ lại đường dẫn chính
                int queryIndex = storagePath.IndexOf('?');
                if (queryIndex >= 0)
                {
                    storagePath = storagePath.Substring(0, queryIndex);
                }

                // Giải mã đường dẫn
                return Uri.UnescapeDataString(storagePath);
            }

            throw new ArgumentException("Invalid file URL format");
        }

    }
}
