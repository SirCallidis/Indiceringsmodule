using Indiceringsmodule.Common.DocumentObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common
{
    public class FactValidationFindings
    {
        public bool allGreen = false;
        public bool areThereDuplicates = false;
        public List<Hyperlink> duplicates = new List<Hyperlink>();
        public bool mismatch = false;   //both list have an equal length, but some items don't match up
        public List<Hyperlink> linksListExceptions = new List<Hyperlink>();
        public List<Hyperlink> dictExceptions = new List<Hyperlink>();
        public bool linkListHasMore = false;
        public bool dictKeysHasMore = false;
        public int differenceCount = 0;
        public Fact fact { get; private set; }

        public FactValidationFindings(Fact fact)
        {
            this.fact = fact;
        }
    }
}
