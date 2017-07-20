namespace RdcManPluginFix
{
    using System;
    using System.Windows.Forms;
    using System.Xml;
    using global::RdcManPluginFix.Events;
    using global::RdcManPluginFix.Interfaces;
    using RdcMan;

    public abstract class PluginFix : IPluginFix
    {
        public event EventHandler<EventArgs> OnContextMenuEvent;

        public event EventHandler<EventArgs> OnDockServerEvent;

        public event EventHandler<EventArgs> OnUndockServerEvent;

        public event EventHandler<EventArgs> PostLoadEvent;

        public event EventHandler<EventArgs> PreLoadEvent;

        public event EventHandler<EventArgs> SaveSettingsEvent;

        public event EventHandler<EventArgs> ShutdownEvent;

        private IPluginContext pluginContext = null;

        static PluginFix()
        {
            RdcManPluginFix.BeginningOfStaticConstructor();
        }

        /// <summary>
        /// Triggered when user right clicks a node in the connection tree
        /// </summary>
        /// <param name="contextMenuStrip">Context menu to be displayed to the user</param>
        /// <param name="node">Which node was right clicked</param>
        public void OnContextMenu(ContextMenuStrip contextMenuStrip, RdcTreeNode node)
        {
            OnContextMenuEvent?.Invoke(node, new ContextMenuStripEventArgs(pluginContext, contextMenuStrip));
        }

        /// <summary>
        /// Triggered when user selects dock on server or closes undocked server window
        /// </summary>
        /// <param name="server">Server that was re-docked</param>
        public void OnDockServer(ServerBase server)
        {
            OnDockServerEvent?.Invoke(server, new RdcManEventArgs(pluginContext));
        }

        /// <summary>
        /// Triggered when user selects undock on server to pop it out to a new window
        /// </summary>
        /// <param name="form">Information about what server was undocked and the window that was created for it</param>
        public void OnUndockServer(IUndockedServerForm form)
        {
            RdcManPluginFix.BeginningOfOnUndockServer(this, form);
            OnUndockServerEvent?.Invoke(form.Server, new MenuStripEventArgs(pluginContext, form.MainMenuStrip));
        }

        /// <summary>
        /// Triggered when the connection tree and all plugins have loaded
        /// </summary>
        /// <param name="context">The current state of the application</param>
        public void PostLoad(IPluginContext context)
        {
            PostLoadEvent?.Invoke(context, new RdcManEventArgs(pluginContext));
        }

        /// <summary>
        /// Triggered after main GUI has loaded but before connection tree has loaded
        /// This fix also saves plugin context for future use in other events
        /// </summary>
        /// <param name="context">The current state of the application</param>
        /// <param name="xmlNode">The exported SaveSettings() XmlNode of a previous session with additional plugin node wrapping XML</param>
        public void PreLoad(IPluginContext context, XmlNode xmlNode)
        {
            RdcManPluginFix.BeginningOfPreLoadMethod(xmlNode, out xmlNode);
            pluginContext = context;
            PreLoadEvent?.Invoke(context, new XmlNodeEventArgs(pluginContext, xmlNode));
        }

        /// <summary>
        /// Triggered when user clicks OK in Tools->Options or closes application
        /// Saves the XML node to remote desktop connection manager global settings file
        /// Default location "%localappdata%\Microsoft\Remote Desktop Connection Manager\RDCMan.settings"
        /// </summary>
        /// <returns>Node to be saved in global settings file which will be passed to PreLoad(IPluginContext,XmlNode) on next remote desktop connection manager session</returns>
        public XmlNode SaveSettings()
        {
            XmlNodeEventArgs args = new XmlNodeEventArgs(pluginContext);
            SaveSettingsEvent?.Invoke(pluginContext, args);
            return args.XmlNode;
        }

        /// <summary>
        /// Triggered when remote desktop connection manager is closed
        /// </summary>
        public void Shutdown()
        {
            ShutdownEvent?.Invoke(pluginContext, new RdcManEventArgs(pluginContext));
        }
    }
}