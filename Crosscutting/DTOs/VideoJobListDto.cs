using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crosscutting.DTOs
{
    public class VideoJobListDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public string? OutputFile { get; set; }
    }
}
