using Indiceringsmodule.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class ServiceReaderJPG : IDocumentReader
    {
        public FlowDocument LoadDocument(string path)
        {
            var doc = new FlowDocument();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.None
            };

            doc.Blocks.Add(new BlockUIContainer(image));

            return doc;
        }

        public KeyValuePair<string, Image> LoadImagePair(string path)
        {
            string name = Path.GetFileName(path);
                       
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform
            };

            var Pair = new KeyValuePair<string, Image>(name, image);
            return Pair;
        }

        public KeyValuePair<string, BitmapImage> LoadBitmapPair(string path)
        {
            string name = Path.GetFileName(path);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            var Pair = new KeyValuePair<string, BitmapImage>(name, bitmap);
            return Pair;
        }

        public Task SaveDocument(FlowDocument document, string selectedPath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
