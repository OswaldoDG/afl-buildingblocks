using System;
using System.Collections.Generic;
using System.Text;

namespace ClientPOC.Models
{
    public class BatchRequestDto
    {
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
        public List<BatchItemRequestDto> Items { get; set; } = [];
    }
}
