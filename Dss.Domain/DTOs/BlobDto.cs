using System;
namespace Dss.Domain.DTOs
{
    public class BlobDto
    {
        public string? Uri { get; set; }
        public string? Name { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public Stream Content { get; set; } = Stream.Null;
        public DateTimeOffset? LastModifiedDate { get; set; }
    }
}