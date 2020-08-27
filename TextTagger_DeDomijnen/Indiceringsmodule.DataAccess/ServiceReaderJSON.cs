using Indiceringsmodule.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Indiceringsmodule.DataAccess
{
    public class ServiceReaderJSON : IDocumentReader
    {
        private readonly IFileSystem _fileSystem;

        #region Constructors

        //Constructor for production
        public ServiceReaderJSON() : this(new FileSystem()) { }

        //Constructor for test environments
        public ServiceReaderJSON(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Work in progress.
        /// Program may not need to load JSON files.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FlowDocument LoadDocument(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Takes a FlowDocument, asynchronously serializes its contents
        /// to a Json string, then saves it to the location of
        /// the selectedPath string.
        /// </summary>
        /// <param name="doc">the FlowDocument to be saved.</param>
        /// <param name="selectedPath">full path where the file will be saved.</param>
        /// <returns>operation Task info</returns>
        public Task SaveDocument(FlowDocument doc, string selectedPath)
        {
            //TODO - Clean up method
            if (doc == null) throw new NullReferenceException($"Document cannot be null.{doc}");
            if (selectedPath == null) throw new NullReferenceException($"Path cannot be null.{selectedPath}");
            if (Path.IsPathRooted(selectedPath) == false) throw new ArgumentException("Path was not valid.", $"{selectedPath}");

            var task = Task.Run(() =>
            {
                var serializedDocument = XamlWriter.Save(doc);
                var jdoc = JsonConvert.SerializeObject(serializedDocument, Formatting.Indented);

                using (StreamWriter writer = _fileSystem.File.CreateText(selectedPath))
                {
                    var serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    serializer.Serialize(writer, jdoc);

                    writer.Write(jdoc);
                }
            });
            return task;

            //var range = new TextRange(doc.ContentStart, doc.ContentEnd);
            //var x = XamlWriter.Save(range);
            //var path1 = @"C:\\Users\\ATeeu\\source\\repos\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\SavedFiles\\Jrange.json";
            //var jrange = JsonConvert.SerializeObject(x, Formatting.Indented);

            //MemoryStream mStream = new MemoryStream();
            //range.Save(mStream, DataFormats.XamlPackage);
            //mStream.Seek(0, SeekOrigin.Begin);

            //var path2 = @"C:\\Users\\ATeeu\\source\\repos\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\SavedFiles\\Jdoc.json";

            //using (StreamWriter writer = File.CreateText(path1))
            //{
            //    var serializer = new JsonSerializer();
            //    serializer.Formatting = Formatting.Indented;
            //    serializer.Serialize(writer, jdoc);
            //    writer.Write(jrange);
            //}

            //var jrange = JsonConvert.SerializeObject(range, Formatting.Indented, 
            //    new JsonSerializerSettings()
            //    {
            //        //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            //        //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //    });
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
