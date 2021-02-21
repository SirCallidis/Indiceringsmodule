using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Indiceringsmodule.Common.DocumentObject;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Linq;
using System.Windows.Controls;

namespace Indiceringsmodule.UnitTests
{
    /// <summary>
    /// Summary description for Fact_Should
    /// </summary>
    [TestClass]
    public class Fact_Should
    {
        public Fact_Should()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CreatePersonSuccessfully()
        {
            // --ARRANGE--
            //create a Fact with an imput string
            var factInput = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. \r\nVestibulum mollis commodo sodales. \r\nSuspendisse sodales ipsum quis lorem maximus elementum.";
            var fact = new Fact(0, factInput);

            //prep the FactDocument for the creation of a textselection
            var factDocPar = fact.FactDocument.Blocks.FirstBlock as Paragraph;
            var pointerStart = factDocPar.ContentStart.GetPositionAtOffset(73);
            var pointerEnd = factDocPar.ContentStart.GetPositionAtOffset(79);

            //insert document in a RichTextBox and create a selection
            var rtb = new RichTextBox(fact.FactDocument);
            rtb.Selection.Select(pointerStart, pointerEnd);

            Assert.AreEqual(fact.GetTotalNumberOfFactMembers(), 0);
            Assert.AreEqual(rtb.Selection.Text, "mollis");

            // --ACT--

            //split text on selection and create a hyperlink out of selection
            var link = new Hyperlink();
            //fact.CreatePerson(link, rtb.Selection.Text);



            // --ASSERT--
            Assert.AreEqual(fact.GetTotalNumberOfFactMembers(), 1);
            Assert.AreEqual(fact.GetFactMember(link).ID, 0);
            
        }

        [TestMethod]
        public void CompareSetsSuccessfully()
        {
            // --ARRANGE--

            var link1 = new Hyperlink(new Run("1"));
            var link2 = new Hyperlink(new Run("2"));
            var link3 = new Hyperlink(new Run("3"));
            var link4 = new Hyperlink(new Run("4"));

            var linksList = new List<Hyperlink> { link1, link2, link3, link4};
            var dict = new Dictionary<Hyperlink, string>();
            dict.Add(link1, "obj1");
            dict.Add(link2, "obj2");
            dict.Add(link3, "obj3");
            //dict.Add(link4, "obj4");

            var allGreen = false;
            var areThereDuplicates = false;
            var duplicates = new List<Hyperlink>();
            var mismatch = false; //both list have an equal length, but some items don't match up
            var linksListExceptions = new List<Hyperlink>();
            var dictExceptions = new List<Hyperlink>();
            var linkListHasMore = false;
            var dictKeysHasMove = false;
            var differenceCount = 0;


            //--ACT--

            if (linksList.Count != linksList.Distinct().Count())
            {
                var hash = new HashSet<Hyperlink>();
                
                foreach (var link in linksList)
                {
                    if (!hash.Add(link))
                    {
                        duplicates.Add(link);
                        areThereDuplicates = true;
                    }
                }
            }
            else
            {
                if (linksList.Count == dict.Keys.Count)
                {
                    IEnumerable<Hyperlink> matches = linksList.Intersect(dict.Keys);
                    if(matches.Count() == linksList.Count())
                    {
                        allGreen = true;
                    }
                    else
                    {
                        mismatch = true;
                        linksListExceptions = linksList.Except(dict.Keys).ToList();
                        dictExceptions = dict.Keys.Except(linksList).ToList();
                    }
                }
                else
                {
                    var greater = Math.Max(linksList.Count, dict.Keys.Count);
                    if (greater == linksList.Count)
                    {
                        linkListHasMore = true;
                        differenceCount = greater - dict.Keys.Count;
                        linksListExceptions = linksList.Except(dict.Keys).ToList();
                    }
                    else if (greater == dict.Keys.Count)
                    {
                        dictKeysHasMove = true;
                        differenceCount = greater - linksList.Count;
                        dictExceptions = dict.Keys.Except(linksList).ToList();
                    }
                }
            }
        }
    }
}
