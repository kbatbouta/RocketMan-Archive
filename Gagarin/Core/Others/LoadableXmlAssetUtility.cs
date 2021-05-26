using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class LoadableXmlAssetUtility
    {
        public static void Push(XmlNode node, LoadableXmlAsset asset, XmlDocument document)
        {
            XmlElement current = document.CreateElement("DefXmlNode");
            XmlElement nLoadableId = document.CreateElement("LoadableXmlAssetId");
            nLoadableId.InnerText = asset?.GetLoadableId() ?? string.Empty;
            current.AppendChild(nLoadableId);
            current.AppendChild(document.ImportNode(node, true));
            document.DocumentElement.AppendChild(current);
        }

        public static void Dump(Dictionary<XmlNode, LoadableXmlAsset> assetlookup, XmlDocument document, string outputPath)
        {
            XmlDocument dump = new XmlDocument();
            dump.RemoveAll();
            dump.AppendChild(dump.CreateElement("DefsXmlStorage"));
            LoadableXmlAsset asset;
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (!assetlookup.TryGetValue(node, out asset))
                    asset = null;
                Push(node, asset, dump);
            }
            dump.Save(outputPath);
        }

        public static XmlDocument Load(Dictionary<string, LoadableXmlAsset> idToLoadable, Dictionary<XmlNode, LoadableXmlAsset> assetlookup, string dumpPath)
        {
            XmlDocument document = new XmlDocument();
            assetlookup.Clear();
            Load(idToLoadable, assetlookup, document, dumpPath);
            return document;
        }

        public static void Load(Dictionary<string, LoadableXmlAsset> idToLoadable, Dictionary<XmlNode, LoadableXmlAsset> assetlookup, XmlDocument document, string dumpPath)
        {
            XmlDocument dump = new XmlDocument();
            dump.Load(dumpPath);
            document.RemoveAll();
            document.AppendChild(document.CreateElement("Defs"));
            assetlookup.Clear();
            foreach (XmlNode node in dump.DocumentElement.ChildNodes)
            {
                string id = node.FirstChild?.InnerText ?? string.Empty;
                XmlNode inner = document.ImportNode(node.LastChild, true);
                if (!id.NullOrEmpty() && idToLoadable.TryGetValue(id, out LoadableXmlAsset loadable))
                    assetlookup[node] = loadable;
                document.DocumentElement.AppendChild(inner);
            }
        }

        public static string GetLoadableId(this LoadableXmlAsset asset)
        {
            string result = asset.name + "$" + asset.FullFilePath + "$" + "$" + (asset.mod?.PackageId ?? "[unkown]").ToLower();
            return result.Replace('/', '$');
        }
    }
}
