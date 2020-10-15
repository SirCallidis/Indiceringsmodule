using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Presentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Indiceringsmodule.UnitTests
{
    [TestClass]
    public class DocumenObject_Should
    {
        [TestMethod]
        public void SelectOneFactUp()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 3 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 3; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 3);

            // --ACT--
            //select the lowest ID fact and check if it can step up.
            var lowestID = docOb.TotalFacts.Min(f => f.ID);
            var result1 = docOb.IsThereAHigherFactID(lowestID);
            //select the second ID fact and check if it can step up.
            var secondID = 1;
            var result2 = docOb.IsThereAHigherFactID(secondID);

            // --ASSERT--
            //check if both results are true.
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void NotSelectOneFactUp()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 2 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 2; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 2);

            // --ACT--
            //select the highest ID fact and check if it can step up.
            var highestID = docOb.TotalFacts.Max(f => f.ID);
            var result = docOb.IsThereAHigherFactID(highestID);

            // --ASSERT--
            //check if result is false, meaning there is no higher number to select.
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ThrowExceptionOnOneFactUp()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 2 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 2; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 2);

            // --ACT--
            //select the lowest ID fact and check if it can step up.
            var outOfBoundsID = 4;
            //below: should throw IndexOutOfRangeException
            docOb.IsThereAHigherFactID(outOfBoundsID);
        }

        [TestMethod]
        public void SelectOneFactDown()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 3 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 3; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 3);

            // --ACT--
            //select the highest ID fact and check if it can step up.
            var highestID = docOb.TotalFacts.Max(f => f.ID);
            var result1 = docOb.IsThereALowerFactID(highestID);
            //select the second ID fact and check if it can step down.
            var secondID = 1;
            var result2 = docOb.IsThereALowerFactID(secondID);

            // --ASSERT--
            //check if both results are true.
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void NotSelectOneFactDown()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 2 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 2; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 2);

            // --ACT--
            //select the lowest ID fact and check if it can step up.
            var lowestID = docOb.TotalFacts.Min(f => f.ID);
            var result = docOb.IsThereALowerFactID(lowestID);

            // --ASSERT--
            //check if result is false, meaning there is no higher number to select.
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ThrowExceptionOnOneFactDown()
        {
            // --ARRANGE--
            //create a DocumentObject and instantiate 2 facts.
            var docOb = new DocumentObject();
            for (int i = 0; i < 2; i++)
            {
                docOb.CreateFact("");
            }
            Assert.IsTrue(docOb.TotalFacts.Count == 2);

            // --ACT--
            var outOfBoundsID = -1;
            //below: should throw IndexOutOfRangeException
            docOb.IsThereALowerFactID(outOfBoundsID);
        }
    }
}
