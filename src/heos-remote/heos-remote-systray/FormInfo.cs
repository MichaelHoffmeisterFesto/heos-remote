using heos_remote_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace heos_remote_systray
{
    public class FormInfo : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TreeView treeXml = new TreeView();

        public FormInfo(
            HeosDiscoveredItem? devInfo = null,
            List<Tuple<string, string>>? nowPlay = null)
        {
            // customise this window
            this.Text = "Information on active HEOS device";
            this.Icon = WinFormsUtils.BytesToIcon(Resources.heos_remote_icon_I5p_icon);
            this.Width = 800;
            this.Height = 600;

            // customise tree
            treeXml.Dock = DockStyle.Fill;
            treeXml.Nodes.Clear();
            this.Controls.Add(treeXml);            

            // Load the XML Document
            XmlDocument doc = new XmlDocument();
            try
            {
                //doc.LoadXml("<books><A property='a'><B>text</B><C>textg</C><D>99999</D></A></books>");
                //doc.Load("");


                if (devInfo != null)
                {
                    // add some direct info
                    var din = treeXml.Nodes.Add("#device");
                    din.Nodes.Add($"FriendlyName: {devInfo.FriendlyName}");
                    din.Nodes.Add($"Host: {devInfo.Host}");
                    din.Nodes.Add($"Location: {devInfo.Location}");
                    din.Nodes.Add($"Manufacturer: {devInfo.Manufacturer}");

                    // now playing?
                    if (nowPlay != null && nowPlay.Count > 0)
                    {
                        var npn = treeXml.Nodes.Add("#now-playing");
                        foreach (var np in nowPlay)
                            npn.Nodes.Add($"{np.Item1}: {np.Item2}");
                    }

                    // prepare later add of xml nodes
                    doc.LoadXml(devInfo.XmlDescription);
                }
                else
                    doc.Load("");
            }
            catch (Exception err)
            {

                return;
            }

            // now add all xml
            ConvertXmlNodeToTreeNode(doc, treeXml.Nodes);

            // and expand
            foreach (var n in treeXml.Nodes)
                if (n is TreeNode tn)
                    tn.ExpandAll();
        }

        private void ConvertXmlNodeToTreeNode(XmlNode xmlNode,
          TreeNodeCollection treeNodes)
        {

            TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);

            switch (xmlNode.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    newTreeNode.Text = "<?" + xmlNode.Name + " " +
                      xmlNode.Value + "?>";
                    break;
                case XmlNodeType.Element:
                    newTreeNode.Text = "<" + xmlNode.Name + ">";
                    break;
                case XmlNodeType.Attribute:
                    newTreeNode.Text = "ATTRIBUTE: " + xmlNode.Name;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    newTreeNode.Text = xmlNode.Value;
                    break;
                case XmlNodeType.Comment:
                    newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
                    break;
            }

            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
                }
            }
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
            }
        }
    }
}
