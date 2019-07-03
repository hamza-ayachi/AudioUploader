using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;

namespace AudioUploader_WebJob
{
    public class Functions
    {
        // This class contains the application-specific WebJob code consisting of event-driven
        // methods executed when messages appear in queues with any supporting code.

        // Trigger method  - run when new message detected in queue. "audiomaker" is name of queue.
        // "audiogallery" is name of storage container; "audios" and "audioFiles" are folder names. 
        // "{queueTrigger}" is an inbuilt variable taking on value of contents of message automatically;
        // the other variables are valued automatically.
        public static void GenerateAudioFile(
        [QueueTrigger("audiomaker")] String blobInfo,
        [Blob("audiogallery/audios/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("audiogallery/audioFiles/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        {
            int duration = 20;
            //use log.WriteLine() rather than Console.WriteLine() for trace output
            logger.WriteLine("GenerateAudioFile() started...");
            logger.WriteLine("Input blob is: " + blobInfo);

            // Open streams to blobs for reading and writing as appropriate.
            // Pass references to application specific methods
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                CreateSample(input, output, duration);
                outputBlob.Properties.ContentType = "audio/mpeg3";
                outputBlob.Metadata["Title"]= inputBlob.Metadata["Title"];
            }
            logger.WriteLine("GenerateAudioFile() completed...");
        }

        // Create thumbnail - the detail is unimportant but notice formal parameter types.
        private static void CreateSample(Stream input, Stream output, int duration)
        {
            using (var reader = new Mp3FileReader(input, wave => new NLayer.NAudioSupport.Mp3FrameDecompressor(wave)))
            {
                Mp3Frame frame;
                frame = reader.ReadNextFrame();
                int frameTimeLength = (int)(frame.SampleCount / (double)frame.SampleRate * 1000.0);
                int framesRequired = (int)(duration / (double)frameTimeLength * 1000.0);

                int frameNumber = 0;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    frameNumber++;

                    if (frameNumber <= framesRequired)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else break;
                }
            }
        }

    }
}
