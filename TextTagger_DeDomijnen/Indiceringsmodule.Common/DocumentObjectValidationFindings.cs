using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    public class DocumentObjectValidationFindings
    {
        public bool allGreen = false;
        public List<FactValidationFindings> nonGreenFactsFindings;

        public DocumentObjectValidationFindings()
        {
            nonGreenFactsFindings = new List<FactValidationFindings>();
        }
    }
}
