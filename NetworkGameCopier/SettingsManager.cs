﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NLog;

namespace NetworkGameCopier
{
    public class SettingsManager
    {
        private const string SettingsXmlPath = "settings.xml";
        private static readonly SettingsManager Instance = new SettingsManager();

        public static SettingsManager GetInstance()
        {
            return Instance;
        }

        public enum SettingsKey
        {
            SteamLibKey,
            BlizzardPathKey
        }

        private readonly SerializableDictionary<SettingsKey, string> _settings
            = new SerializableDictionary<SettingsKey, string>();

        private bool _isIoNeeded = false;

        private SettingsManager()
        {
            try
            {
                LoadSettings();
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Info("Fresh start! " + e.Message);
                LoadDefaultValues();
            }
        }

        public void LoadSettings()
        {
            if(_settings.Count != 0) _settings.Clear();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (XmlReader reader = XmlReader.Create(new StreamReader(SettingsXmlPath), settings))
            {
                _settings.ReadXml(reader);
            }
        }

        public void LoadDefaultValues()
        {
            SetDefaultBlizzardPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            SetDefaultSteamLibrary(Path.Combine(SteamOperations.GetInstance().SteamappsPath, "common"));
        }

        public void SetDefaultSteamLibrary(string path)
        {
            _settings[SettingsKey.SteamLibKey] = path;
            IoDone();
        }

        public void SetDefaultBlizzardPath(string path)
        {
            _settings[SettingsKey.BlizzardPathKey] = path;
            IoDone();
        }

        public string GetDefaultSteamLibrary()
        {
            return _settings[SettingsKey.SteamLibKey];
        }

        public string GetDefaultBlizzardPath()
        {
            //return @"C:\teste";
            return _settings[SettingsKey.BlizzardPathKey];
        }

        private void IoDone()
        {
            if(!_isIoNeeded) _isIoNeeded = true;
        }

        public void ForceSave()
        {
            if(!_isIoNeeded) return;
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (XmlWriter writer = XmlWriter.Create(new StreamWriter(SettingsXmlPath, false), settings))
            {
                _settings.WriteXml(writer);
            }
            _isIoNeeded = false;
        }

    }


 
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.None)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            //if(reader.NodeType == XmlNodeType.None) reader.Skip();
            //else reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
