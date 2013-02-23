using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("envelope_template_items")]
    public class EnvelopeTemplateItem : DataModel
    {
        public int envelope_template_id { get; set; }
        public int envelope_id { get; set; }
        public decimal amount { get; set; }
    }
}
