using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace HandmadeProductManagementAPI.Extensions
{
    public static class FirebaseServiceExtensions
    {
        public static void AddFireBaseServices(this IServiceCollection services)
        {
            try
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("handmade-product-bde7a-firebase-adminsdk-g3y5r-fd05db8476.json")
                });
                Console.WriteLine("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Firebase: {ex.Message}");
            }
        }
    }
}

