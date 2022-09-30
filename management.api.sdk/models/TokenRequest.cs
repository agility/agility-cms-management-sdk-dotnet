using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agility.models
{
    public class TokenRequest
    {
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
        public string? audience { get; set; }
        public string? grant_type { get; set; }
    }
}
