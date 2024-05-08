﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RevueTestExam.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string ? Msg { get; set; }

        [JsonPropertyName("revueId")]

        public string ? RevueId { get; set; }
    }
}
