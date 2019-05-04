using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastCopy
{
    class FileProp
    {
        public DateTime LastUpdate { get; set; }
        public long FileSize { get; set; }
        public UInt32 Hash { get; set; }
    }
}
