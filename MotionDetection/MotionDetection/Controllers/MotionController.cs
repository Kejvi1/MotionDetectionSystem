using Microsoft.AspNetCore.Mvc;
using MotionDetection.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace MotionDetection.Controllers
{
    public class MotionController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly CompressService compressService;
        private readonly VideoServices videoServices;

        public MotionController(IHttpClientFactory httpClientFactory, CompressService compressService, VideoServices videoServices)
        {
            this.httpClientFactory = httpClientFactory;
            this.compressService = compressService;
            this.videoServices = videoServices;
        }


        [HttpGet]
        public IActionResult DetectFrames()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DetectFrames(string video)
        {
            if(video.Contains("drive.google.com"))
            {
                int slash = video.LastIndexOf('/');
                string fileId = video.Substring(32, slash - 32);
                //get frames
                var are_extracted = await videoServices.GetVideoFrames(fileId);
                if (are_extracted.Item1 == "ok")
                {
                    //get the frames of the video
                    var images = videoServices.CreateListOfFrames(are_extracted.Item2.Images);
                    //create the video
                    var videoPath = await videoServices.CreateVideoAsync(images, are_extracted.Item2.FrameRate, 320, 240);
                    ViewBag.Result = videoPath;
                    ViewBag.Success = "Motion detected and new video created! Click below to download!";
                }
                else ViewBag.Error = are_extracted.Item1; //display error
            }          
            else
            {
                ViewBag.Error = "Url is not of google drive!";
            }

            return View();
        
        }

        [HttpGet]
        public IActionResult Download(string videoPath)
        {
            try
            {
                //load the video bytes into memory
                var compressedVideoBytes = compressService.LoadVideo(videoPath).Result;
                return File(compressedVideoBytes, "video/mp4", "googledrivevideo.mp4");
            }
            finally
            {
                System.IO.File.Delete(videoPath);
            }

        }

    }
}
