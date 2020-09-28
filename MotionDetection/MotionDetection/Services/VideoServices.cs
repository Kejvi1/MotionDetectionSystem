using Microsoft.AspNetCore.Hosting;
using MotionDetection.Models;
using Newtonsoft.Json;
using SharpAvi;
using SharpAvi.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MotionDetection.Services
{
    public class VideoServices
    {
        private readonly AviVideoServices aviVideoServices;
        private readonly CompressService compressService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private Guid guid;
        private IHttpClientFactory httpClientFactory;
        public VideoServices(AviVideoServices aviVideoServices, IWebHostEnvironment webHostEnvironment, CompressService compressService, IHttpClientFactory httpClientFactory)
        {
            this.aviVideoServices = aviVideoServices;
            this.webHostEnvironment = webHostEnvironment;
            this.compressService = compressService;
            this.httpClientFactory = httpClientFactory;
        }

        //get the video frames
        public async Task<Tuple<string, VideoModel>> GetVideoFrames(string video)
        {
            VideoModel videoModel = new VideoModel();
            //first get the video frames
            HttpClient client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:58492/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage message = await client.PostAsJsonAsync("api/Frames/getVideoFrames", video);
            if (message.IsSuccessStatusCode)
            {
                string result = message.Content.ReadAsStringAsync().Result;
                videoModel = JsonConvert.DeserializeObject<VideoModel>(result);
                Tuple<string, VideoModel> tuple = new Tuple<string, VideoModel>("ok", videoModel);
                return tuple;
            }
            else
            {
                string result = message.Content.ReadAsStringAsync().Result;
                Tuple<string, VideoModel> tuple = new Tuple<string, VideoModel>(result, null);
                return tuple;
            }
        }

        //create the video locally
        public async Task<string> CreateVideoAsync(List<byte[]> images, int frameRate, int width, int height)
        {
            guid = Guid.NewGuid();
            string fileName = guid.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Millisecond + ".avi";
            string compressedVideoName = null;
            string path = Path.Combine(webHostEnvironment.WebRootPath, "videos", fileName);
            try
            {
                var writer = new AviWriter(path)
                {
                    FramesPerSecond = frameRate,
                    EmitIndex1 = true
                };
                var stream = writer.AddVideoStream();
                stream.Width = width;
                stream.Height = height;
                stream.Codec = KnownFourCCs.Codecs.Uncompressed;
                stream.BitsPerPixel = BitsPerPixel.Bpp32;

                foreach (var image in images)
                {
                    //convert the byte array of the frame to bitmap and flip it upside down
                    var bm = aviVideoServices.ToBitmap(image);

                    //reduce the frames size to match the video size
                    var rbm = aviVideoServices.ReduceBitmap(bm, width, height);

                    //convert the bitmap to byte array
                    byte[] fr = aviVideoServices.BitmapToByteArray(rbm);

                    //write the frame to the video
                    await stream.WriteFrameAsync(true, fr, 0, fr.Length);
                }

                writer.Close();

                //compress the video
                compressedVideoName = compressService.CompressAndConvertVideo(fileName);
                return compressedVideoName;

            }

            finally
            {
                File.Delete(path);
            }
            
        }

        public List<byte[]> CreateListOfFrames(List<string> frames)
        {
            List<byte[]> images = new List<byte[]>();

            foreach (var frame in frames)
            {
                //convert from base64
                var fr = Convert.FromBase64String(frame);
                images.Add(fr);
            }
            return images;
        }

        //public void DownloadFrames(List<byte[]> images)
        //{
        //    int i = 0;
        //    foreach (var image in images)
        //    {
        //        using (MemoryStream mStream = new MemoryStream(image))
        //        {
        //            while (i < 10000)
        //            {
        //                var img = Image.FromStream(mStream);
        //                img.Save(@"C:\Users\kejvi\Desktop\Video1Frames\frame" + i + ".jpg", ImageFormat.Jpeg);
        //                i++;
        //                break;
        //            }

        //        }
        //    }
        //}
    }
}
