using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace MotionDetection.Services
{
    public class CompressService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private Guid guid;
        public CompressService(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }
        public byte[] GetImageBytes(Bitmap file)
        {
            using (var stream = new MemoryStream())
            {
                file.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }

        public string CompressAndConvertVideo(string fileName)
        {
            guid = Guid.NewGuid();
            string outputFileName = guid + DateTime.Now.Day.ToString() + DateTime.Now.Minute +  DateTime.Now.Millisecond + ".mp4";
            string outfile = Path.Combine(webHostEnvironment.WebRootPath, "videos", outputFileName);
            string inputfile = Path.Combine(webHostEnvironment.WebRootPath, "videos", fileName);
            string ffmpeg = Path.Combine(webHostEnvironment.WebRootPath, "ffmpeg", "ffmpeg.exe");
                
            string args = "-y -i {0} -vcodec mpeg4 -qscale:v 1 {1}";

            var pinf = new ProcessStartInfo(ffmpeg, string.Format(args, inputfile, outfile));
            pinf.UseShellExecute = false;
            pinf.RedirectStandardInput = true;

            var proc = Process.Start(pinf);

            proc.WaitForExit();
            return outfile;           
        }

        public async Task<byte[]> LoadVideo(string path)
        {
            byte[] myVideoByteArray = await File.ReadAllBytesAsync(path);
            return myVideoByteArray;
        }
    }
}
