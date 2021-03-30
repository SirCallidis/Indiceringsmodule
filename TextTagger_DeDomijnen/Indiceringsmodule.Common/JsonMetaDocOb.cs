using Indiceringsmodule.Common.DocumentObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    public class JsonMetaDocOb
    {
        public Settings Settings;
        public List<string> FactMembers;
        public List<string> Transcription;
        public List<object> Facts;

        public JsonMetaDocOb()
        {
            InitializeFields();
        }

        private void InitializeFields()
        {
            Settings = new Settings();
            FactMembers = new List<string>();
            Transcription = new List<string>();
            Facts = new List<object>();
        }
    }
}
