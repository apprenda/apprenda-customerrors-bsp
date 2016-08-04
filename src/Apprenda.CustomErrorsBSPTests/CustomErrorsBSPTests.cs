using Microsoft.VisualStudio.TestTools.UnitTesting;
using Apprenda.BSP;
using System;
using System.Collections.Generic;
using Apprenda.API.Extension.Bootstrapping;
using Apprenda.API.Extension.CustomProperties;
using System.IO;
using System.Xml;
using System.Linq;

namespace Apprenda.CustomErrorsBSPTests
{
    [TestClass()]
    public class CustomErrorsBSPTests
    {
        private CustomErrorsBSP bsp;
        private string resourcesTmpPath;
        private const string OffValue = "off";

        private const string PathResources = "../../Resources/";
        private string PathNoConfig = "/Resources/Components/no-config-file/";
        private string PathMissingCustomErrors = "/Resources/Components/config-missing-customerrors/";
        private string PathMissingSystemWeb = "/Resources/Components/config-missing-system-web/";
        private string PathMultiConfig = "/Resources/Components/multi-config-file/";
        private string PathConfigNotXML = "/Resources/Components/config-file-not-xml/";
        private string PathDifferentCase = "/Resources/Components/different-case-config-file/";

        public CustomErrorsBSPTests()
        {
            bsp = new CustomErrorsBSP();
            var curDir = Directory.GetCurrentDirectory();

            resourcesTmpPath = curDir + "/Resources/";

            PathNoConfig = curDir + PathNoConfig;
            PathMissingCustomErrors = curDir + PathMissingCustomErrors;
            PathMissingSystemWeb = curDir + PathMissingSystemWeb;
            PathMultiConfig = curDir + PathMultiConfig;
            PathConfigNotXML = curDir + PathConfigNotXML;
            PathDifferentCase = curDir + PathDifferentCase;
        }

        [TestInitialize]
        public void InitializeTestSuite()
        {
            DirectoryCopy(PathResources, resourcesTmpPath, true);
        }

        [TestCleanup]
        public void FinalizeTestSuite()
        {
            Directory.Delete(resourcesTmpPath, true);
        }

        [TestMethod()]
        public void TestBSPPassesForNonASPNetComponents()
        {
            var request = GenerateRequest("random", ComponentType.LinuxService);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against a Linux Service Component");

            request = GenerateRequest("random", ComponentType.War);
            result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against a WAR Component");

            request = GenerateRequest("random", ComponentType.WcfService);
            result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against a WCF Service Component");

            request = GenerateRequest("random", ComponentType.WindowsService);
            result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against a Windows Service Component");
        }

        [TestMethod()]
        public void TestBSPPassesForApprendaASPNetComponents()
        {
            var request = GenerateRequest("apprenda", ComponentType.AspNet);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an Apprenda website even though it should have been ignored");
        }

        [TestMethod()]
        public void TestBSPPassesForNoConfigFile()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathNoConfig);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an app with no config");
            AssertValueOfCustomErrors(PathNoConfig, null, true);
        }

        [TestMethod()]
        public void TestBSPPassesForDifferentCaseConfigFile()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathDifferentCase);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an app with different case config");
            AssertValueOfCustomErrors(PathDifferentCase, OffValue);
        }

        [TestMethod()]
        public void TestBSPPassesForMissingCustomErrors()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathMissingCustomErrors);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an app with missing customErrors section in config");
            AssertValueOfCustomErrors(PathMissingCustomErrors, OffValue);
        }

        [TestMethod()]
        public void TestBSPPassesForMissingSystemWeb()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathMissingSystemWeb);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an app with missing system.Web section in config");
            AssertValueOfCustomErrors(PathMissingSystemWeb, null, true);
        }

        [TestMethod()]
        public void TestBSPPassesForMultiConfigFile()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathMultiConfig);
            var result = bsp.Bootstrap(request);
            Assert.IsTrue(result.Succeeded, "Failed execution against an app with multiple config files");
            AssertValueOfCustomErrors(PathMultiConfig, OffValue);
        }

        [TestMethod()]
        public void TestBSPPassesForNonXMLConfigFile()
        {
            var request = GenerateRequest("random", ComponentType.AspNet, PathConfigNotXML);
            var result = bsp.Bootstrap(request);
            Assert.IsFalse(result.Succeeded, "Failed execution against an app with non xml config file");
            Assert.IsTrue(result.Errors.Any());
            AssertValueOfCustomErrors(PathConfigNotXML, null, true);
        }

        private BootstrappingRequest GenerateRequest(string devTeamAlias, ComponentType componentType)
        {
            return GenerateRequest(devTeamAlias, componentType, null);
        }

        private BootstrappingRequest GenerateRequest(string devTeamAlias, ComponentType componentType, string componentPath)
        {
            return new MockBootstrappingRequest
            (
                componentPath,
                componentType,
                Guid.Empty,
                Guid.Empty,
                "my component",
                "my app",
                "my version",
                new List<CustomProperty>(),
                devTeamAlias,
                false,
                false,
                false,
                false,
                Stage.Sandbox
            );
        }

        //Method borrowed from MSFT for simplicity.
        //https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void AssertValueOfCustomErrors(string path, string value, bool expectValueNotSet = false)
        {
            string[] configFiles = Directory.GetFiles(path, "web.config", SearchOption.AllDirectories);

            if (!configFiles.Any() && !expectValueNotSet)
            {
                Assert.Fail("No config files found at '{0}'", path);
            }

            foreach (string file in configFiles)
            {
                try
                {
                    //Traverse the web.config file and find the required section
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);
                    XmlElement customErrors = (XmlElement)xmlDoc.SelectSingleNode("//system.web/customErrors");

                    if (customErrors == null && !expectValueNotSet)
                    {
                        Assert.Fail("No customErrors section found at config file '{0}'", path);
                    }
                    else if (customErrors == null && expectValueNotSet)
                    {
                        break;
                    }

                    Assert.AreEqual(value, customErrors.Attributes["mode"].Value, String.Format("Failed setting config file '{0}'", file));
                }
                catch (Exception e)
                {
                    if (!expectValueNotSet)
                    {
                        Assert.Fail("An exception occurred while verifying config files at '{0}': {1} ", path, e.Message);
                    }
                }
            }
        }
    }
}