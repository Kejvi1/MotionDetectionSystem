using System.Collections.Generic;

namespace ImagesApi.Models
{
    public class VideoModel
    {
        public List<string> Images { get; set; }
        public int FrameRate { get; set; }
    }
}