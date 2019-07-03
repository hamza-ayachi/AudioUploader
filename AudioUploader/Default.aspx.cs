﻿using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Linq;

// Remember: code behind is run at the server.

namespace AudioUploader
{
    public partial class _Default : System.Web.UI.Page
    {
        // accessor variables and methods for blob containers and queues
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudQueueService _queueStorageService = new CloudQueueService();

        private CloudBlobContainer getAudioGalleryContainer()
        {
            return _blobStorageService.getCloudBlobContainer();
        }

        private CloudQueue getAudioFileMakerQueue()
        {
            return _queueStorageService.getCloudQueue();
        }


        private string GetMimeType(string Filename)
        {
            try
            {
                string ext = Path.GetExtension(Filename).ToLowerInvariant();
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (key != null)
                {
                    string contentType = key.GetValue("Content Type") as String;
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
            }
            return "application/octet-stream";
        }

        // User clicked the "Submit" button
        protected void submitButton_Click(object sender, EventArgs e)
        {
            if (upload.HasFile)
            {
                // Get the file name specified by the user. 
                var ext = Path.GetExtension(upload.FileName);

                // Add more information to it so as to make it unique
                // within all the files in that blob container
                var name = string.Format("{0}{1}", Guid.NewGuid(), ext);

                // Upload photo to the cloud. Store it in a new 
                // blob in the specified blob container. 

                // Go to the container, instantiate a new blob
                // with the descriptive name
                String path = "audios/" + name;

                var blob = getAudioGalleryContainer().GetBlockBlobReference(path);

                // The blob properties object (the label on the bucket)
                // contains an entry for MIME type. Set that property.
                blob.Properties.ContentType = GetMimeType(upload.FileName);
                var title = upload.FileName.Replace(".mp3" , "" );
                blob.Metadata["Title"] = title;
        

                // Actually upload the data to the
                // newly instantiated blob
                blob.UploadFromStream(upload.FileContent);

                // Place a message in the queue to tell the worker
                // role that a new photo blob exists, which will 
                // cause it to create a thumbnail blob of that photo
                // for easier display. 
                getAudioFileMakerQueue().AddMessage(new CloudQueueMessage(System.Text.Encoding.UTF8.GetBytes(name)));

                System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", path));
            }
        }

        // rerun every timer click - set by timer control on aspx page to be every 1000ms
        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                // Look at blob container that contains the audioFiles
                // generated by the worker role. Perform a query
                // of the its contents and return the list of all of the
                // blobs whose name begins with the string "audioFiles". 
                // It returns an enumerator of their URLs. 
                // Place that enumerator into list view as its data source. 
                AudioFileDisplayControl.DataSource = from o in getAudioGalleryContainer().GetDirectoryReference("audioFiles").ListBlobs()
                                                     select new { Url = o.Uri , Title = getAudioTitle(o.Uri) };

                // Tell the list view to bind to its data source, thereby
                // showing 
                AudioFileDisplayControl.DataBind();
            }
            catch (Exception)
            {
            }
        }

        private object getAudioTitle(Uri uri)
        {
            CloudBlockBlob blob = new CloudBlockBlob(uri);
            blob.FetchAttributes();
            return blob.Metadata["Title"];
         
        }
    }
}