using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FilterPlay.Utilities
{
    public static class CloudBlobUtility
    {
        public const int CacheControlMaxExpiration = Int32.MaxValue; // about 68 years
        public const int CacheControlOneWeekExpiration = 7 * 24 * 60 * 60; // 1 week
        public const int CacheControlOneHourExpiration = 60 * 60; // 1 hr

        /// <summary>
        ///   Iterates through each blob in the specified container and adds the
        ///   Cache-Control and ContentType headers
        /// </summary>
        public static void EnsureStaticFileHeaders(
            CloudBlobContainer container,
            int cacheControlMaxAgeSeconds)
        {
            string cacheControlHeader = "public, max-age=" + cacheControlMaxAgeSeconds.ToString();

            var blobInfos = container.ListBlobs(
                new BlobRequestOptions() { UseFlatBlobListing = true });
            Parallel.ForEach(blobInfos, (blobInfo) =>
            {
                // get the blob properties and set headers if necessary
                CloudBlob blob = container.GetBlobReference(blobInfo.Uri.ToString());
                blob.FetchAttributes();
                var properties = blob.Properties;

                bool wasModified = false;

                // see if a content type is defined for the extension
                string extension = Path.GetExtension(blobInfo.Uri.LocalPath);
                string contentType = GetContentType(extension);
                if (String.IsNullOrEmpty(contentType))
                {
                    Trace.TraceWarning("Content type not found for extension:" + extension);
                }
                else
                {
                    if (properties.ContentType != contentType)
                    {
                        properties.ContentType = contentType;
                        wasModified = true;
                    }
                }

                if (properties.CacheControl != cacheControlHeader)
                {
                    properties.CacheControl = cacheControlHeader;
                    wasModified = true;
                }

                if (wasModified)
                {
                    blob.SetProperties();
                }
            });
        }

        /// <summary>
        ///   Finds all js and css files in a container and creates a gzip compressed
        ///   copy of the file with ".gzip" appended to the existing blob name
        /// </summary>
        public static void EnsureGzipFiles(
            CloudBlobContainer container,
            int cacheControlMaxAgeSeconds)
        {
            string cacheControlHeader = "public, max-age=" + cacheControlMaxAgeSeconds.ToString();

            var blobInfos = container.ListBlobs(
                new BlobRequestOptions() { UseFlatBlobListing = true });
            Parallel.ForEach(blobInfos, (blobInfo) =>
            {
                string blobUrl = blobInfo.Uri.ToString();
                CloudBlob blob = container.GetBlobReference(blobUrl);

                // only create gzip copies for css and js files
                string extension = Path.GetExtension(blobInfo.Uri.LocalPath);
                if (extension != ".css" && extension != ".js")
                    return;

                // see if the gzip version already exists
                string gzipUrl = blobUrl + ".gzip";
                CloudBlob gzipBlob = container.GetBlobReference(gzipUrl);
                if (gzipBlob.Exists())
                    return;

                // create a gzip version of the file
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // push the original blob into the gzip stream
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    using (BlobStream blobStream = blob.OpenRead())
                    {
                        blobStream.CopyTo(gzipStream);
                    }

                    // the gzipStream MUST be closed before its safe to read from the memory stream
                    byte[] compressedBytes = memoryStream.ToArray();

                    // upload the compressed bytes to the new blob
                    gzipBlob.UploadByteArray(compressedBytes);

                    // set the blob headers
                    gzipBlob.Properties.CacheControl = cacheControlHeader;
                    gzipBlob.Properties.ContentType = GetContentType(extension);
                    gzipBlob.Properties.ContentEncoding = "gzip";
                    gzipBlob.SetProperties();
                }
            });
        }

        /// <summary>
        ///   Gets the content type for the specified extension
        /// </summary>
        private static string GetContentType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                case ".css":
                    return "text/css";
                case ".js":
                    return "text/javascript";
                case ".svg":
                    return "image/svg+xml";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".eot":
                    return "application/vnd.ms-fontobject";
                case ".woff":
                    return "application/x-woff";
                case ".ttf":
                    return "font/ttf";
                case ".otf":
                    return "font/otf";
            }

            return null;
        }

        public static bool Exists(this CloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public static bool Exists(this CloudBlobContainer container)
        {
            try
            {
                container.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
