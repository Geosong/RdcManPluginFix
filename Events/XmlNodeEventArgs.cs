namespace RdcManPluginFix.Events
{
    using System.Xml;
    using RdcMan;

    public class XmlNodeEventArgs : RdcManEventArgs
    {
        public XmlNode XmlNode { get; set; }

        public XmlNodeEventArgs(IPluginContext pluginContext, XmlNode xmlNode = null) : base(pluginContext)
        {
            XmlNode = xmlNode;
        }
    }
}