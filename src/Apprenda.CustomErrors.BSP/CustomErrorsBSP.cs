﻿using System;
using Apprenda.API.Extension.Bootstrapping;
using System.IO;
using System.Xml;

namespace Apprenda.BSP
{
    public class CustomErrorsBSP : BootstrapperBase
    {
        public override BootstrappingResult Bootstrap(BootstrappingRequest bootstrappingRequest)
        {
            //Only modify .NET Websites Components that do not belong to the Apprenda Team
            if (bootstrappingRequest.ComponentType == ComponentType.AspNet && 
                !bootstrappingRequest.DevelopmentTeamAlias.Equals("apprenda", StringComparison.InvariantCultureIgnoreCase))
            {
                return ModifyConfigFiles(bootstrappingRequest);
            }
            else
            {
                return BootstrappingResult.Success();
            }
        }

        private static BootstrappingResult ModifyConfigFiles(BootstrappingRequest bootstrappingRequest)
        {
            //Search for all web.config files within the component being deployed
            string[] configFiles = Directory.GetFiles(bootstrappingRequest.ComponentPath, "web.config", SearchOption.AllDirectories);
            
            foreach (string file in configFiles)
            {
                var result = ModifyXML(bootstrappingRequest, file);
                if (!result.Succeeded)
                {
                    //If an XML modification fails, return a failure for the BSP
                    return result;
                }
            }
            return BootstrappingResult.Success();
 
        }

        private static BootstrappingResult ModifyXML(BootstrappingRequest bootstrappingRequest, string filePath)
        {

            try
            {
                //Traverse the web.config file and find the required section
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                XmlNode appsettingsNode = xmlDoc.SelectSingleNode("//system.web");

                XmlElement customErrors = (XmlElement)xmlDoc.SelectSingleNode("//system.web/customErrors");

                //If there is no custom errors setting defined, add it and save the modifed configuration file
                if (null == customErrors && appsettingsNode != null)
                {
                    XmlNode newElement = xmlDoc.CreateNode(XmlNodeType.Element, "customErrors", null);
                    XmlAttribute attribute1 = xmlDoc.CreateAttribute("mode");
                    attribute1.Value = "off";
                    newElement.Attributes.Append(attribute1);
                    appsettingsNode.AppendChild(newElement);
                    xmlDoc.Save(filePath);
                    return BootstrappingResult.Success();
                }
                //If there is a custom errors setting configuration, overwrite it and set it to off
                else if (null != customErrors && appsettingsNode != null)
                {
                    customErrors.Attributes["mode"].Value = "off";
                    xmlDoc.Save(filePath);
                    return BootstrappingResult.Success();
                }
                return BootstrappingResult.Success();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BootstrappingResult.Failure(new[] { ex.InnerException.Message });
                }
                else
                {
                    return BootstrappingResult.Failure(new[] { ex.Message });
                }
            }
            
        }
    }
}
