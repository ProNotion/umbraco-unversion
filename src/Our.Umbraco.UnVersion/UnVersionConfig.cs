using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;

namespace Our.Umbraco.UnVersion
{
    public class UnVersionConfig : IUnVersionConfig
    {
        private readonly static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool ExecuteOnStartup { get; set; }
        public IDictionary<string, List<UnVersionConfigEntry>> ConfigEntries { get; set; }

        public UnVersionConfig(string configPath)
        {
            ConfigEntries = new Dictionary<string, List<UnVersionConfigEntry>>();

            LoadXmlConfig(configPath);
        }

        private void LoadXmlConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Logger.Warn("Couldn't find config file " + configPath);
                return;
            }

            var xmlConfig = new XmlDocument();
            xmlConfig.Load(configPath);

            var rootNode = xmlConfig.SelectSingleNode("/unVersionConfig");
            var hasStartupAttribute = rootNode.Attributes != null && rootNode.Attributes["executeOnStartup"] != null;
            bool executeOnStartup;
            if (hasStartupAttribute && bool.TryParse(rootNode.Attributes["executeOnStartup"].Value, out executeOnStartup))
            {
                ExecuteOnStartup = executeOnStartup;
            }

            foreach (XmlNode xmlConfigEntry in xmlConfig.SelectNodes("/unVersionConfig/add"))
            {
                if (xmlConfigEntry.NodeType == XmlNodeType.Element)
                {
                    var configEntry = new UnVersionConfigEntry
                    {
                        DocTypeAlias = xmlConfigEntry.Attributes["docTypeAlias"] != null
                            ? xmlConfigEntry.Attributes["docTypeAlias"].Value
                            : "$_ALL"
                    };

                    if (xmlConfigEntry.Attributes["rootXpath"] != null)
                        configEntry.RootXPath = xmlConfigEntry.Attributes["rootXpath"].Value;

                    if (xmlConfigEntry.Attributes["maxDays"] != null)
                        configEntry.MaxDays = Convert.ToInt32(xmlConfigEntry.Attributes["maxDays"].Value);

                    if (xmlConfigEntry.Attributes["maxCount"] != null)
                        configEntry.MaxCount = Convert.ToInt32(xmlConfigEntry.Attributes["maxCount"].Value);

                    if (!ConfigEntries.ContainsKey(configEntry.DocTypeAlias))
                        ConfigEntries.Add(configEntry.DocTypeAlias, new List<UnVersionConfigEntry>());

                    if (xmlConfigEntry.Attributes["includeDescendants"] != null)
                    {
                        bool includeDescendants;
                        if (bool.TryParse(xmlConfigEntry.Attributes["includeDescendants"].Value, out includeDescendants))
                        {
                            configEntry.IncludeDescendants = includeDescendants;
                        }
                    }

                    ConfigEntries[configEntry.DocTypeAlias].Add(configEntry);
                }
            }
        }
    }

    public class UnVersionConfigEntry
    {
        public UnVersionConfigEntry()
        {
            MaxDays = MaxCount = int.MaxValue;
        }

        public string DocTypeAlias { get; set; }
        public string RootXPath { get; set; }
        public int MaxDays { get; set; }
        public int MaxCount { get; set; }
        
        public bool IncludeDescendants { get; set; }
    }

    public interface IUnVersionConfig
    {
        bool ExecuteOnStartup { get;  }
        
        IDictionary<string, List<UnVersionConfigEntry>> ConfigEntries { get; }
    }
}