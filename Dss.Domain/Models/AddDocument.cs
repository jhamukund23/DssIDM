﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dss.Domain.Models
{
    public class AddDocument
    {
        public Guid CorrelationId { get; set; }
        public Guid? DocId { get; set; }
        public Uri TempBlobURL { get; set; }
        public string? PermanentURL { get; set; }
        public string? FileName { get; set; }
    }
}
