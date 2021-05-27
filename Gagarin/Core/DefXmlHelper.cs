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

        private static Dictionary<string, XmlElement> defnamelookup = new Dictionary<string, XmlElement>();

        private static string report = string.Empty;

        public static void Clear()
        {
            defnamelookup.Clear();
            namelookup.Clear();
        }

        public static HashSet<XmlElement> FindDuplicates(XmlNodeList nodes)
        {
            report = string.Empty;
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
                    if (!node.HasChildNodes)
                    {
                        continue;
                    }
                    XmlNodeList children = node.ChildNodes;
                    for (int j = 0; j < children.Count; j++)
                    {
                        if (children[j] is XmlElement sub)
                        {
                            if (sub?.IsEmpty ?? true)
                            {
                                continue;
                            }
                            if (sub.Name == "defName")
                            {
                                TryRegisterDefName(node, node.Name + "$" + sub.InnerText);
                            }
                        }
                    }
                }
            }
            Log.Message($"GAGARIN: FindDuplicates <color=red>report</color>\n{report}");
            return skipSet;
        }

        private static void TryRegisterNamed(XmlElement node, string name)
        {
            report += $"{skipSet.Count}: TryRegisterNamed {name}";
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

        private static void TryRegisterDefName(XmlElement node, string name)
        {
            report += $"{skipSet.Count}: TryRegisterNamedDef {name}\n";
            if (skipSet.Contains(node))
            {
                return;
            }
            if (defnamelookup.TryGetValue(name, out XmlElement other))
            {
                skipSet.Add(other);
            }
            defnamelookup[name] = node;
        }
    }
}
