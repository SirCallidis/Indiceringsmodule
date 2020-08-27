using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    public class Subscription : IDisposable
    {
        private readonly Action RemoveMethod;

        public Subscription(Action removeMethod)
        {
            RemoveMethod = removeMethod;
        }
        
        public void Dispose()
        {
            RemoveMethod?.Invoke();
        }
    }
}
