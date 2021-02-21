using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class ImagePacket
    {
        public string Hash { get; set; } = string.Empty;
        public int Length { get; set; } = 0;
        public string Image { get; set; } = string.Empty;
        public ImagePacket() { }

        public ImagePacket(byte[] img_sources)
        {
            Hash = StringHash(img_sources);
            Length = img_sources.Length;
            Image = EncodeBytes(img_sources);
        }

        public byte[] GetRawData()
        {
            byte[] data = DecodeBytes(Image);

            if (data.Length != Length) throw new Exception("Error data len");
            if (!StringHash(data).Equals(Hash)) throw new Exception("Error hash");

            return data;
        }

        /// <summary>
        /// Conver Image to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(BitmapImage value)
        {
            ImageConverter converter = new ImageConverter();
            byte[] arr = (byte[])converter.ConvertTo(value, typeof(byte[]));
            return arr;
        }

        /// <summary>
        /// Conver byte array to Image
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Image BytesToImage(byte[] value)
        {
            using (var ms = new MemoryStream(value))
            {
                return System.Drawing.Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Convert bytes to base64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeBytes(byte[] value) => Convert.ToBase64String(value);
        
        /// <summary>
        /// convert base64 to bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>      
        public static byte[] DecodeBytes(string value) => Convert.FromBase64String(value);
        
        /// <summary>
        /// get MD5 hash from byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StringHash(byte[] value)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(value);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
