using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dss.Domain.Models
{
    public class AddDocumentResponse
    {
        public Guid CorrelationId { get; set; }
        public Uri? SasUrl { get; set; }

    }
}
