using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Infrastructure.Config
{
    public class O365AuditLogs
    {
        public string BaseAddress { get; set; }
        public string Resource { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}
