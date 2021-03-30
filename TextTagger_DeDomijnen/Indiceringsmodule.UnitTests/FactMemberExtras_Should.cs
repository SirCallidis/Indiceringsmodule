using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using Indiceringsmodule.Common;
using Indiceringsmodule.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Indiceringsmodule.UnitTests
{
    [TestClass]
    public class FactMemberExtras_Should
    {
        [TestMethod]
        public void LoadFactMemberFieldsTestDataCorrectly()
        {
            // --Arrange--
            var targetFile = "C:\\Users\\ATeeu\\Documents\\GitHub\\Indiceringsmodule\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\FactMemberFields.json";
            PopulateFactMemberFieldsFileWithTestData(targetFile);
            var ea = new EventAggregator();
            var loader = new FileLoader(ea);

            // --Act--
            var list = loader.LoadFactMemberExtras(targetFile);

            //var keys = list.Select(x => x.Key);
            //foreach (var key in keys)
            //{
            //    var z = key;
            //}

            var obj1matchName = list.Where(x => x.Key == "Set1")
                                    .Select(x => x.Key)
                                    .First();

            var obj2matchName = list.Where(x => x.Key == "Set2")
                                    .Select(x => x.Key)
                                    .First();

            var obj1PropMatch = list.Where(x => x.Key == "Set1")
                                    .Select(x => x.Value)
                                    .First();

            var obj2PropMatch = list.Where(x => x.Key == "Set2")
                                    .Select(x => x.Value)
                                    .First();

            // --Assert--
            
            //correct number of items in the list?
            Assert.AreEqual(list.Count, 2);

            //do both items have the right names?
            Assert.AreEqual(obj1matchName, "Set1");
            Assert.AreEqual(obj2matchName, "Set2");

            //do both items have the correct amount of properties?
            Assert.AreEqual(obj1PropMatch.Length, 4);
            Assert.AreEqual(obj2PropMatch.Length, 4);

            //are the names of the properties correct for both items?
            Assert.AreEqual(obj1PropMatch[0], "FirstProp1");
            Assert.AreEqual(obj1PropMatch[1], "SecondProp1");
            Assert.AreEqual(obj1PropMatch[2], "ThirdProp1");
            Assert.AreEqual(obj1PropMatch[3], "FourthProp1");

            Assert.AreEqual(obj2PropMatch[0], "FirstProp2");
            Assert.AreEqual(obj2PropMatch[1], "SecondProp2");
            Assert.AreEqual(obj2PropMatch[2], "ThirdProp2");
            Assert.AreEqual(obj2PropMatch[3], "FourthProp2");
        }

        public void PopulateFactMemberFieldsFileWithTestData(string targetFile)
        {           
            var propArray1 = new string[] { "FirstProp1", "SecondProp1", "ThirdProp1", "FourthProp1" };
            var propArray2 = new string[] { "FirstProp2", "SecondProp2", "ThirdProp2", "FourthProp2" };
            var fieldSetList = new List<KeyValuePair<string, string[]>>();
            
            fieldSetList.Add(new KeyValuePair<string, string[]>("Set1", propArray1));
            fieldSetList.Add(new KeyValuePair<string, string[]>("Set2", propArray2));

            JsonSerializer sr = new JsonSerializer
            {
                Formatting = Formatting.Indented,
            };
            using (StreamWriter sw = new StreamWriter(targetFile))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    sr.Serialize(writer, fieldSetList);
                }
            }
        }
    }
}
