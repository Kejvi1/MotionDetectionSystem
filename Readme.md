# MotionDetection
Check if there is motion in video using C#.

# Libraries used:
Gleamtech VideoUltimate -> to get the frames of the video
AForge -> to detect if there is motion in these frames by using "two frames difference algorithm" which checks if there is difference in pixels between two sequential frames
Google Drive Api v3 -> to get the publicly shared files in google drive.
SharpAvi -> to create the new avi video with the frames where motion is detected
Ffmpeg -> to compress the video and convert it to mp4 because the newly rendered is bigger than the old one (you couldn't set the bitrate of avi video because the library didn't have a method to do so, you could only resize the frames to make the video somewhat smaller)

*All of the above libraries are open-source and free to use
*Input your credentials inside Credentials folder. You can download them from the "Google Api Console".

# Logic of the program
1. First the user inputs the url of his publicly shared google drive video. The program checks if the url is that of google drive or not and extracts only the file id of the shared file which is in the url (eg: https://drive.google.com/d/ABCDEFGHIJ/view...). It's the one between /d and /view.

2. The file id is sent to the web api via http client to the method that extracts the byte array of the shared file.

3. After the byte array is extracted, Gleamtech offers a way to extract the frames of this byte array via their library. You can also extract different metadata about the video like framerate, duration, bitrate, height, width etc. Because the byte array and the frames are loaded in memory, there is a limitation about the video duration. I check the video duration and if the video is longer than 30 sec, the programs cancels the frame extraction and returns a message to the user.

4. During the frame extraction, we check to see if there is motion in the video. AForge has a couple of different algorithms you can use. It also highlights the motion in the video. The algorithm I chose to use was the "two frames difference algorithm".

5. All the extracted frames are added to the list and returned to the user.

6. The frames are used to create a new video of ".avi" format via the SharpAvi library. The library used frames that were read from bottom to top to create the video and had 4 codecs you could choose from. I chose to use the uncompressed one that wanted the frames to be read from bottom to top. You could also choose the bits per pixel you wanted per frame. Anything lower than bpp32 was unwatchable. Also, because the frames were read from bottom to top, the rendered video would be upside down. So the frames were first converted to bitmap, flipped, resized to the width and height that we set the video to be created to and converted to byte array. The framerate of the new video was set to be the same of the old one. (Reference: https://stackoverflow.com/questions/50001791/using-sharpavi-to-convert-images-to-video)

7. The new rendered video will be bigger in size than the old one. So i chose to use ffmpeg to resize the video using mmpeg4 virtual codec and setting the bitrate (qscale) to 1 (it's between 1-31 where 1 is the highest and 31 the lowest). You could also manipulate the audio of the video. (Reference: https://trac.ffmpeg.org/wiki/Encode/MPEG-4). For example a 30-40 video would be rendered to 100mb. The compressed version would be just 7-10kb.

8. Both the rendered video and the compressed video are first saved locally. When the compressed version is rendered, the original one is deleted. Than this version is converted to byte array and sent to the client for download. When the client downloads the video, the compressed one is deleted as well.
