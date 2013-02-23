using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    public interface IFetchOfx
    {
        OfxAccount FetchOfx(DateTime fromDate, DateTime toDate);
        string Request { get; }
        string Response { get; }
    }
}
