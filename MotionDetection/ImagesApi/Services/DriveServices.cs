using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace ImagesApi.Services
{
    public class DriveServices
    {
        public byte[] GetDriveVideoBytes(string url)
        {
            try
            {
                string[] Scopes = { DriveService.Scope.DriveReadonly };
                UserCredential credential;
                var credentialsPath = @"C:\Users\kejvi\Desktop\MotionDetection\ImagesApi\Credentials\credentials.json";
                using (var stream =
                    new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MotionDemo"
                });


                var request = service.Files.Get(url);
                var ms = new MemoryStream();

                request.Download(ms);
                return ms.ToArray();
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}