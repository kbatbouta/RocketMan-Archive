using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using Verse;

namespace Gagarin.Core
{
    public static class DefXmlHelper
    {
        private static HashSet<XmlElement> skipSet = new HashSet<XmlElement>();
        private static Dictionary<string, XmlElement> namelookup = new Dictionary<string, XmlElement>();

        public static void Clear()
        {
            namelookup.Clear();
        }

        public static HashSet<XmlElement> FindDuplicates(XmlNodeList nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is XmlElement node)
                {
                    if (node?.IsEmpty ?? true)
                    {
                        continue;
                    }
                    if (node.HasAttribute("Name"))
                    {
                        TryRegisterNamed(node, node.GetAttribute("Name"));
                    }
                }
            }
            return skipSet;
        }

        private static void TryRegisterNamed(XmlElement node, string name)
        {
            if (skipSet.Contains(node))
            {
                return;
            }
            if (namelookup.TryGetValue(name, out XmlElement other))
            {
                skipSet.Add(other);
            }
            namelookup[name] = node;
        }
    }
}
