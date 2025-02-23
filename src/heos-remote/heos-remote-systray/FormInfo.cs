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

        public FormInfo()
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
                doc.LoadXml("<books><A property='a'><B>text</B><C>textg</C><D>99999</D></A></books>");
                //doc.Load("");
            }
            catch (Exception err)
            {

                return;
            }

            ConvertXmlNodeToTreeNode(doc, treeXml.Nodes);
            treeXml.Nodes[0].ExpandAll();
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
