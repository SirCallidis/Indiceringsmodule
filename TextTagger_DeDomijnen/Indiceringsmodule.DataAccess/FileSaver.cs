using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class FileSaver : Observable
    {
        private readonly EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        public FileSaver(EventAggregator ea)
        {
            Ea = ea;
            Subscriptions.Add(Ea.Subscribe<PublishDocumentEventModel>(m => OpenSaveFileDialog(m.Data)));
        }

        public void SaveFile()
        {
            Ea.Publish(new RequestDocumentForSavingEventModel() { });
        }

        private List<string> SerializeFactMembers(Dictionary<Hyperlink, FactMember> factMembers)
        {
            var jfactMembers = new List<string>();
            foreach (var factMember in factMembers)
            {
                var serializedFact = XamlWriter.Save(factMember.Value);
                var jFactDoc = JsonConvert.SerializeObject(serializedFact, Formatting.Indented);
                jfactMembers.Add(jFactDoc);
            }
            return jfactMembers;
        }

        private List<string> SerializeFactDocs(ObservableCollection<Fact> totalFacts)
        {
            var jFactDocs = new List<string>();
            foreach (var factDoc in totalFacts)
            {
                var serializedFact = XamlWriter.Save(factDoc);
                var jFactDoc = JsonConvert.SerializeObject(serializedFact, Formatting.Indented);
                jFactDocs.Add(jFactDoc);
                //check if hyperlinks are saved??
            }
            return jFactDocs;
        }

        private string SerializeTranscriptionDoc(FlowDocument transcriptionDocument)
        {
            //try
            //{
            //    var xmlWriterSettings = new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true };
            //    string path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "config.xml");
            //    using (XmlWriter xmlWriter = XmlWriter.Create(path, xmlWriterSettings))
            //    {
            //        FHConfig obj = new FHConfig();
            //        XamlServices.Save(xmlWriter, obj);
            //    }
            //}
            //catch (Exception exep) { MessageBox.Show("Saving UI parameters: " + exep.Message); }

            //var xmlWriterSettings = new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true };
            //var path = @"C:/";
            //var xmlWriter = XmlWriter.Create(path, xmlWriterSettings);
            //XamlWriter.Save(transcriptionDocument, xmlWriter);


            //var serializedDocument = XamlWriter.Save(transcriptionDocument);
            //var jTranscriptionDoc = JsonConvert.SerializeObject(serializedDocument, Formatting.Indented);
            //return jTranscriptionDoc;

            var serializedDocument = XamlWriter.Save(transcriptionDocument);
            return serializedDocument;
        }

        private List<ImagePacket> SerializeImages(ObservableDictionary<string, BitmapImage> images)
        {
            var imagePacketList = new List<ImagePacket>();
            foreach (var pair in images)
            {
                byte[] imgToBytes = ImagePacket.ImageToBytes(pair.Value);
                ImagePacket packet = new ImagePacket(imgToBytes);
                imagePacketList.Add(packet);
            }
            return imagePacketList;
        }

        /// <summary>
        ///
        /// </summary>
        private void OpenSaveFileDialog(DocumentObject docOb)
        {          
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Json files (*.json)|*.json",
                DefaultExt = ".json"
            };

            var path = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = new DirectoryInfo(path).Parent.Parent.FullName;
            var targetDirectory = rootPath + @"\SavedFiles";

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            saveFileDialog.InitialDirectory = targetDirectory;

            if (saveFileDialog.ShowDialog() == true)
            {
                var selectedPath = saveFileDialog.FileName;
                //Task.Run(() => SaveFileToJson(selectedPath, docOb));
                SaveFileToJson(selectedPath, docOb);
                ValuesChanged = false;
            }
            else
            {
                MessageBox.Show("Error while saving");
            }
        }

        private void SaveFileToJson(string path, DocumentObject docOb)
        {
            var serializeSettings = true;
            var serializeImages =false;
            var serializeTranscriptionDoc = true;
            var serializeFactDocs = false;
            var serializeFactMembers = false;

            var jDocOb = new Dictionary<string, object>();
            jDocOb.Add("TITLE", Path.GetFileName(path));
            
            if (serializeSettings)
            {
                jDocOb.Add("SETTINGS", docOb.Settings);
            }
            if (serializeImages)
            {
                //TODO serialize images
                jDocOb.Add("IMAGES", docOb.Images);
            }
            if (serializeTranscriptionDoc)
            {
                var blocks = docOb.TranscriptionDocument.Blocks;
                var allParas = false;
                var transcriptionText = string.Empty;
                foreach (var block in blocks)
                {
                    if (block.GetType() == typeof(Paragraph))
                    {
                        allParas = true;
                    }
                    else
                    {
                        allParas = false;
                        break;
                    }
                    if (allParas)
                    {
                        var range = new TextRange(block.ContentStart, block.ContentEnd);
                        transcriptionText = range.Text;
                    }
                }
                jDocOb.Add("TRANSCRIPT", transcriptionText);
            }
            if (serializeFactDocs)
            {
                jDocOb.Add("FACTS", docOb.TotalFacts); //below: needs work, make dict out of totals.
                foreach (var fact in docOb.TotalFacts)
                {
                    var x = fact.TotalFactMembers;
                }
            }
            if (serializeFactMembers)
            {
                //handled in serializeFactDocs?
            }

            
            var keyStrings = new List<string>();

            var facts = docOb.TotalFacts;
            foreach (var fact in facts)
            {
                var totalFactMembers = fact.TotalFactMembers;
                foreach (var key in totalFactMembers.Keys)
                {
                    var keyString = fact.GetTextFromHyperlink(key);
                    keyStrings.Add(keyString);
                }
            }
            jDocOb.Add("KEYS", keyStrings);

            jDocOb.Add("TEST DOC", new List<string>
            {
                "Sentence first part ",
                keyStrings[1],
                " last part of sentence."
            });

            JsonSerializer sr = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
            };
            using (StreamWriter sw = new StreamWriter(path))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    sr.Serialize(writer, jDocOb);
                }
            }

            //custom serialization:
            var strW = new StringWriter();
            var wr = new JsonTextWriter(strW)
            {
                Formatting = Formatting.Indented
            };
            wr.WriteStartObject();
            wr.WritePropertyName("NAME");
            wr.WriteValue("Bob Testington");
            wr.WritePropertyName("ARRAY");
            wr.WriteStartArray();
            wr.WriteValue("Item1");
            wr.WriteValue("Item1");
            wr.WriteEndArray();
            wr.WritePropertyName("OBJECT2");
            wr.WriteStartObject();
            wr.WritePropertyName("sub");
            wr.WriteValue(1);
            wr.WriteEndObject();
            wr.WriteEndObject();
            wr.Flush();

            var jsonText = strW.GetStringBuilder().ToString();
            using (StreamWriter sw = new StreamWriter(path+".json"))
            {
                strW.Write(jsonText);
            }
            

            //var jDocSet = JsonConvert.SerializeObject(docOb.Settings);
            //var imagePacketList = new List<ImagePacket>();
            //var jTranscriptionDoc = "";
            //var jFactDocs = new List<string>();
            //var jFactMembers = new List<string>();

            ////serialize list of images --- https://stackoverflow.com/questions/58922730/serializing-bitmap-using-json
            //if (serializeImages)
            //{
            //    if (docOb.Images.Count > 0)
            //    {
            //        imagePacketList = SerializeImages(docOb.Images);
            //    }                
            //}

            ////serialize transcription doc
            //if (serializeTranscriptionDoc)
            //{
            //    if (docOb.TranscriptionDocument == null) throw new NullReferenceException("Document cannot be null");
            //    //jTranscriptionDoc = await Task.Run(() => SerializeTranscriptionDoc(docOb.TranscriptionDocument));
            //    //jTranscriptionDoc = SerializeTranscriptionDoc(docOb.TranscriptionDocument); //may remove this method: oneliner!
            //    jTranscriptionDoc = XamlWriter.Save(docOb.TranscriptionDocument);
            //}

            ////serialize facts - special care on hyperlinks
            //if (serializeFactDocs)
            //{
            //    jFactDocs = SerializeFactDocs(docOb.TotalFacts);
            //}

            ////serialize factmembers
            //if (serializeFactMembers)
            //{
            //    foreach (var fact in docOb.TotalFacts)
            //    {
            //        jFactDocs = SerializeFactMembers(fact.TotalFactMembers);
            //    }
            //}

            //create one large string file for saving
            //var jDocOb = jDocSet + jTranscriptionDoc;

            //using (StreamWriter sw = File.CreateText(newPath))
            //{
            //    JsonSerializer sr = new JsonSerializer
            //    {
            //        Formatting = Formatting.Indented,
            //        PreserveReferencesHandling = PreserveReferencesHandling.All,
            //    };
            //    sr.Serialize(sw, jDocSet);
            //}

            //save the above into json file
            //File.WriteAllText(path, jTranscriptionDoc);
            //var reader = new ServiceReaderJSON();
            //await reader.SaveDocument(docOb.TranscriptionDocument, path);
        }
    }
}
