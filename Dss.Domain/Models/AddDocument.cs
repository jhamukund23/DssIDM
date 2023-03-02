using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dss.Domain.Models
{
    public class AddDocument
    {
        public Guid CorrelationId { get; set; }
        public string FileName { get; set; }
        public string FileSise { get; set; }

    }
}
