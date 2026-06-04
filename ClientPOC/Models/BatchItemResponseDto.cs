using System;
using System.Collections.Generic;
using System.Text;

namespace ClientPOC.Models
{
    public class BatchItemResponseDto
    {
        /// <summary>
        /// Unique identifier for the batch item.
        /// </summary>
        required public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the batch item at the client side.
        /// When converted to BatchItemEntity, this value will be used as the Id for the entity, ensuring consistency between client and server representations.
        /// If no item id id is provided then the filename will be used in place.
        /// </summary>
        public string? ItemId { get; set; }

        /// <summary>
        /// URL for uploading the file associated with this batch item. This URL is typically a pre-signed URL that allows the client to upload the file
        /// directly to a storage service without needing to go through the server, improving efficiency and scalability.
        /// </summary>
        public string? UploadUrl { get; set; }
    }

}
