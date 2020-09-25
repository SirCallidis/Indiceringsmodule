using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.Common.EventModels
{
    public class LoadedKVPairStringBitmapimageEventModel
    {
        public KeyValuePair<string, BitmapImage> Data { get; set; }
    }
}
