using System;
using System.Runtime.CompilerServices;
using System.Xml;
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

        public static string GetLoadableId(this LoadableXmlAsset asset)
        {
            return (asset.name + "$" + asset.FullFilePath + "$" + asset.fullFolderPath + "$" + asset.mod?.PackageId ?? "[unkown]").ToLower();
        }
    }
}
