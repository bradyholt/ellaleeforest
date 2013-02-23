using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    public enum TransactionImportsStatusEnum : int
    {
        NOT_IMPORTED = 1,
        IMPORTED = 2,
        ERROR = 3,
        IN_PROCESS = 4
    }
}
