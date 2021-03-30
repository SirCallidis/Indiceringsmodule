using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class FileSaver : Observable
    {
        #region Fields & Properties

        private readonly EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        //fields for the creation of a save file
        private List<string>  sfFactMemberEntry;
        private List<object> sfFacts;
        private List<KeyValuePair<Hyperlink, int>> referenceList;
        private int factMemberIndex;

        #endregion Fields & Properties

        public FileSaver(EventAggregator ea)
        {
            Ea = ea;
            Subscriptions.Add(Ea.Subscribe<PublishDocumentEventModel>(m => OpenSaveFileDialog(m.Data)));
        }

        #region Methods

        /// <summary>
        /// Fires an event that requests the neccesary data from other classes.
        /// </summary>
        public void SaveFile()
        {
            Ea.Publish(new RequestDocumentForSavingEventModel() { });
        }
        
        /// <summary>
        /// Opens a SaveFileDialog and handles its actions.
        /// </summary>
        /// <param name="docOb">the document to be saved</param>
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
                //Task.Run(() => SaveFileToJson(selectedPath, docOb)); //at some point try to make saver async.
                SaveFileToJson(selectedPath, docOb);
                ValuesChanged = false;
            }
            else
            {
                MessageBox.Show("Error while saving");
            }
        }

        /// <summary>
        /// Converts the provided DocumentObject into a saveable format and
        /// writes it to the path location.
        /// </summary>
        /// <param name="path">the location where the file will be saved to.</param>
        /// <param name="docOb">the document to be serialized and saved.</param>
        private void SaveFileToJson(string path, DocumentObject docOb)
        {
            ClearSaveFileFields();
            CreateFactMemberList(docOb.TotalFacts);
            SerializeFacts(docOb.TotalFacts);
            
            var serializedTranscription = SerializeFlowDocument(docOb.TranscriptionDocument);

            var saveableDocOb = new {
                docOb.Settings,
                FactMembers = sfFactMemberEntry,
                Transcription = serializedTranscription,
                Facts = sfFacts };

            SerializeAndWrite(path, saveableDocOb);           
        }

        /// <summary>
        /// Clears and reinitializes the fields needed in the
        /// creation of a saveable DocumentObject
        /// </summary>
        private void ClearSaveFileFields()
        {
            sfFactMemberEntry = null;
            sfFacts = null;
            referenceList = null;
            factMemberIndex = 0;

            sfFactMemberEntry = new List<string>();
            sfFacts = new List<object>();
            referenceList = new List<KeyValuePair<Hyperlink, int>>();
        }

        /// <summary>
        /// Creates a combined list out of all the factmembers of all Facts
        /// </summary>
        /// <param name="totalFacts"></param>
        private void CreateFactMemberList(ObservableCollection<Fact> totalFacts)
        {
            foreach (var fact in totalFacts)
            {
                SerializeFactMemberDictionary(fact);
            }
        }

        /// <summary>
        /// Serializes all Facts and stores them in a Field.
        /// </summary>
        /// <param name="totalFacts"></param>
        private void SerializeFacts(ObservableCollection<Fact> totalFacts)
        {
            foreach (var fact in totalFacts)
            {
                var factDocList = SerializeFlowDocument(fact.FactDocument);
                sfFacts.Add(new { fact.ID, FactDocument = factDocList });
            }
        }

        /// <summary>
        /// Takes the fact's dictionary and turns it into a list of string,
        /// containing a reference indicator specified between [([x])], the
        /// Hyperlink's text, the type of Factmember, and the serialized Factmember object.
        /// it also populates a temporary reference list for later cross-referencing.
        /// </summary>
        /// <param name="fact"></param>
        private void SerializeFactMemberDictionary(Fact fact)
        {
            foreach (var kvPair in fact.TotalFactMembers)
            {
                var hLinkText = fact.GetTextFromHyperlink(kvPair.Key);
                var serializedFactMember = JsonConvert.SerializeObject(kvPair.Value);
                var factMemberType = kvPair.Value.GetType().Name;

                var ExtrasPropertyNames = kvPair.Value.GetListOfSetContent(kvPair.Value.SelectedSetName);
                var Extras = new Dictionary<string, string>();
                var ExtraPropValues = new string[15] {
                kvPair.Value.ExtraPropVal0,
                kvPair.Value.ExtraPropVal1,
                kvPair.Value.ExtraPropVal2,
                kvPair.Value.ExtraPropVal3,
                kvPair.Value.ExtraPropVal4,
                kvPair.Value.ExtraPropVal5,
                kvPair.Value.ExtraPropVal6,
                kvPair.Value.ExtraPropVal7,
                kvPair.Value.ExtraPropVal8,
                kvPair.Value.ExtraPropVal9,
                kvPair.Value.ExtraPropVal10,
                kvPair.Value.ExtraPropVal11,
                kvPair.Value.ExtraPropVal12,
                kvPair.Value.ExtraPropVal13,
                kvPair.Value.ExtraPropVal14 };

                for (int i = 0; i < ExtrasPropertyNames.Length; i++)
                {
                    Extras.Add(ExtrasPropertyNames[i], ExtraPropValues[i]);
                }
                var serializedExtras = JsonConvert.SerializeObject(Extras);

                var entry = $"[([{factMemberIndex}])] | {hLinkText} | {factMemberType} | {serializedFactMember} | {serializedExtras}";
                sfFactMemberEntry.Add(entry);
                referenceList.Add(new KeyValuePair<Hyperlink, int>(kvPair.Key, factMemberIndex));
                factMemberIndex++;
            }
        }

        /// <summary>
        /// Converts a FlowDocument's content to a list of string
        /// where the Hyperlinks are replaced with reference indicators.
        /// </summary>
        /// <param name="doc">the FlowDocument to be converted</param>
        /// <returns>List of strings representing each inline in the document</returns>
        private List<string> SerializeFlowDocument(FlowDocument doc)
        {
            var flowDocumentList = new List<string>();
            
            var par = doc.Blocks.FirstBlock as Paragraph;
            foreach (var inline in par.Inlines)
            {
                var type = inline.GetType();
                switch (type.ToString())
                {
                    case "System.Windows.Documents.Run":
                        var run = inline as Run;
                        flowDocumentList.Add(run.Text);
                        break;
                    case "System.Windows.Documents.Hyperlink":
                        var hlink = inline as Hyperlink;
                        foreach (var item in referenceList)
                        {
                            if (item.Key == hlink)
                            {
                                flowDocumentList.Add($"([({item.Value})])");
                                break;
                            }
                        }
                        break;
                    case "System.Windows.Documents.LineBreak":
                        flowDocumentList.Add("\r\n");
                        break;
                    default:
                        throw new ArgumentException($"Could not parse {type}");
                }
            }
            return flowDocumentList;
        }

        /// <summary>
        /// Writes the object to the specified path on disk.
        /// </summary>
        /// <param name="path">location where the file will be saved to.</param>
        /// <param name="newObject">the file to be saved.</param>
        private void SerializeAndWrite(string path, object newObject)
        {
            JsonSerializer sr = new JsonSerializer
            {
                Formatting = Formatting.Indented,
            };
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    sr.Serialize(writer, newObject);
                }
            }
            File.SetAttributes(path, FileAttributes.ReadOnly);
        }

        #endregion Methods


        #region unused & testing methods

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

        private void SaveFileToJson1(string path, DocumentObject docOb)
        {
            var jDocOb = new Dictionary<string, object>();

            var list = new List<string> { "listItem1", "listItem2" };
            var array = new string[2] { "arrayItem1", "arrayItem2" };

            jDocOb.Add("SETTINGS", docOb.Settings);
            jDocOb.Add("LIST", list);
            jDocOb.Add("ARRAY", array);
            jDocOb.Add("Reference", docOb.Settings);

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
        }
        private void SaveFileToJson2(string path, DocumentObject docOb)
        {
            var serializeSettings = true;
            var serializeImages = false;
            var serializeTranscriptionDoc = true;
            var serializeFactDocs = false;
            var serializeFactMembers = false;

            var jDocOb = new Dictionary<string, object>
            {
                { "TITLE", Path.GetFileName(path) }
            };

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


            var keyStringz = new List<string>();
            var keyStrings = new string[docOb.TotalFacts.Count()];

            var facts = docOb.TotalFacts;
            foreach (var fact in facts)
            {
                var i = 0;
                var totalFactMembers = fact.TotalFactMembers;
                foreach (var key in totalFactMembers.Keys)
                {
                    var keyString = fact.GetTextFromHyperlink(key);
                    //keyStrings.Add(keyString);
                    keyStrings[i] = keyString;
                    i++;
                }
            }
            jDocOb.Add("KEYS", keyStrings);

            var list = new List<string>
            {
                "Sentence first part ",
                keyStrings[0],
                " last part of sentence.",
            };

            jDocOb.Add("TEST DOC", list);

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
            //var strW = new StringWriter();
            //var wr = new JsonTextWriter(strW)
            //{
            //    Formatting = Formatting.Indented
            //};
            //wr.WriteStartObject();
            //wr.WritePropertyName("NAME");
            //wr.WriteValue("Bob Testington");
            //wr.WritePropertyName("ARRAY");
            //wr.WriteStartArray();
            //wr.WriteValue("Item1");
            //wr.WriteValue("Item1");
            //wr.WriteEndArray();
            //wr.WritePropertyName("OBJECT2");
            //wr.WriteStartObject();
            //wr.WritePropertyName("sub");
            //wr.WriteValue(1);
            //wr.WriteEndObject();
            //wr.WriteEndObject();
            //wr.Flush();

            //var jsonText = strW.GetStringBuilder().ToString();
            //using (StreamWriter sw = new StreamWriter(path+".json"))
            //{
            //    strW.Write(jsonText);
            //}


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

        #endregion
    }
}
