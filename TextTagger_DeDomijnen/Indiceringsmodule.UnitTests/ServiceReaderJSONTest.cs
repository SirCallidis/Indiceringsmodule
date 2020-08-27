using System;
using System.IO.Abstractions.TestingHelpers;
using System.Windows.Documents;
using Indiceringsmodule.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Indiceringsmodule.UnitTests
{
    [TestClass]
    public class ServiceReaderJSONTest
    {
        [TestMethod]
        public void ServiceReaderSavesFileOnMockFileSystem()
        {
            // ---Arrange---
            // Create a mock input file
            var doc = new FlowDocument();

            // Setup mock file system starting state
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(@"c:\tests\");        

            // Create ServiceReaderJSON with mock file system
            var serviceReader = new ServiceReaderJSON(mockFileSystem);

            // ---Act---
            // process input file
            var path = @"c:\tests\savedDoc.json";
            var task = serviceReader.SaveDocument(doc, path);
            task.Wait();

            // ---Assert---
            // Check mock file system for output file
            Assert.IsTrue(mockFileSystem.FileExists(@"c:\tests\savedDoc.json"));

            // Check if output file in mock file system has content
            var savedJSONfile = mockFileSystem.GetFile(@"c:\tests\savedDoc.json");
            var contents = savedJSONfile.TextContents;
            Assert.IsNotNull(contents);
        }

        [TestMethod]
        public void ServiceReaderSaveDocumentWhereDocIsNull()
        {
            // ---Arrange---
            // Create a mock input file
            FlowDocument doc = null;

            // Create a mock path
            var path = @"c:\tests\savedDoc.json";

            // Setup mock file system starting state
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(@"c:\tests\");

            // Create ServiceReaderJSON with mock file system
            var serviceReader = new ServiceReaderJSON(mockFileSystem);

            // ---Act---
            // process input file
            var ex = Assert.ThrowsException<NullReferenceException>(() =>
            {
                var task = serviceReader.SaveDocument(doc, path);
                task.Wait();
            });

            // ---Assert---
            // Check if correct exception was thrown
            Assert.AreEqual(ex.Message, $"Document cannot be null.");
        }

        [TestMethod]
        public void ServiceReaderSaveDocumentWherePathIsInvalid()
        {
            // ---Arrange---
            // Create a mock input file
            var doc = new FlowDocument();

            // Create a mock path
            var path = @"ThisIsNotAValidPath";

            // Setup mock file system starting state
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(@"c:\tests\");

            // Create ServiceReaderJSON with mock file system
            var serviceReader = new ServiceReaderJSON(mockFileSystem);

            // ---Act---
            // process input file
            var ex = Assert.ThrowsException<Exception>(() =>
            {
                var task = serviceReader.SaveDocument(doc, path);
                task.Wait();
            });

            // ---Assert---
            // Check if correct exception was thrown
            Assert.AreEqual(ex.Message, $"Document cannot be null.");
        }
    }
}
