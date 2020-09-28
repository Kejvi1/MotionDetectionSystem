using GleamTech.VideoUltimate;
using ImagesApi.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VisioForge.Shared.AForge.Vision.Motion;

namespace ImagesApi.Services
{
    public class FramesService
    {
        private VideoFrameReader videoFrameReader;
        private MotionDetector motionDetector;
        private VideoModel videoModel;
        public FramesService()
        {
            motionDetector = new MotionDetector(new TwoFramesDifferenceDetector(), new MotionAreaHighlighting());
            videoModel = new VideoModel();
        }
        public VideoModel GetVideoFrames(byte[] videoBytes)
        {
            Bitmap fr;
            videoModel.Images = new List<string>();
            //create the frame reader
            using (MemoryStream ms = new MemoryStream(videoBytes))
            using (videoFrameReader = new VideoFrameReader(ms))
            {
                //if video too big
                if(videoFrameReader.Duration.TotalSeconds > 30)
                {
                    return null;
                }
                else
                {
                    //calculating number of frames as fps/framerate*duration
                    videoModel.FrameRate = (int)Math.Ceiling(videoFrameReader.FrameRate);
                    //        var totalFrames = videoFrameReader.Duration.TotalSeconds * fps;
                    //looping through all the frames
                    foreach (var frame in videoFrameReader)
                    {
                        //for this frame
                        using (frame)
                        {
                            //detect if there is motion
                            fr = DetectMotion(motionDetector, frame);
                            //if there is motion get image bytes, convert it to base64 string and add it to list
                            if (fr != null)
                            {
                                var bytes = GetImageBytes(fr);
                                var base64Frame = Convert.ToBase64String(bytes);
                                videoModel.Images.Add(base64Frame);
                            }
                            //if there isn't motion get the byte array of frame, convert it to base 64 and add it to list
                            else
                            {
                                var bytes = GetImageBytes(frame);
                                var base64Frame = Convert.ToBase64String(bytes);
                                videoModel.Images.Add(base64Frame);
                            }
                        }
                    }
                }
                
                videoFrameReader.Dispose();
                return videoModel;
            }
        }    

        //detect if there is motion in frames
        private Bitmap DetectMotion(MotionDetector motionDetector, Bitmap videoFrame)
        {
            Bitmap frames = null;
            // process new video frame and check motion level
            if (motionDetector.ProcessFrame(videoFrame) > 0.02) frames = videoFrame;
            return frames;
        }

        //convert the bitmap file to byte array
        public byte[] GetImageBytes(Bitmap file)
        {
            using (var stream = new MemoryStream())
            {
                file.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
    }
}