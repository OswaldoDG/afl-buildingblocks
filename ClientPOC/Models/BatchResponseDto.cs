using System;
using System.Collections.Generic;
using System.Text;

namespace ClientPOC.Models
{
    public class BatchResponseDto
    {
        /// <summary>
        /// Unique identifier for the batch request, used for tracking and correlation.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the batch operation.
        /// </summary>
        public BatchStatus BatchStatus { get; set; }

        /// <summary>
        /// Name of the batch job, used for logging and tracking purposes.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Destination folder where the batch files will be uploaded.
        /// </summary>
        required public string DestinationFolder { get; set; }

        /// <summary>
        /// Batch items to be processed.
        /// </summary>
        public List<BatchItemResponseDto> Items { get; set; } = [];
    }

    public enum BatchStatus
    {
        /// <summary>
        /// The batch is waiting to be processed.
        /// </summary>
        Pending,

        /// <summary>
        /// The batch is currently being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// The batch has been processed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The batch processing failed due to an error.
        /// </summary>
        Failed,
    }

    public enum FileStatus
    {
        /// <summary>
        /// The file is waiting to be uploaded.
        /// </summary>
        Uploading,

        /// <summary>
        /// The file is fully uploaded.
        /// </summary>
        Uploaded,

        /// <summary>
        /// The file processing failed due to an error.
        /// </summary>
        Failed,
    }

}
