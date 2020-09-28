using ImagesApi.Services;
using System.Web.Http;

namespace ImagesApi.Controllers
{
    [RoutePrefix("api/Frames")]
    public class FramesController : ApiController
    {
        private readonly FramesService framesService;
        private readonly DriveServices driveServices;
        public FramesController()
        {
            framesService = new FramesService();
            driveServices = new DriveServices();
        }

        [Route("getVideoFrames")]
        [HttpPost]
        public IHttpActionResult GetFrames([FromBody] string video)
        {
            //get byte array of video
            var videoBytes = driveServices.GetDriveVideoBytes(video);
            if (videoBytes != null)
            {
                //get frames
                var frames = framesService.GetVideoFrames(videoBytes);
                if (frames != null) return Ok(frames);
                else return BadRequest("Video too big");
            }
            else return BadRequest("Drive file is not shared or is not a video");
        }
    }
}
