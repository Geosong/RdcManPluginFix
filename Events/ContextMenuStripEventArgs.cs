namespace RdcManPluginFix.Events
{
    using System;
    using System.Windows.Forms;
    using RdcMan;

    public class ContextMenuStripEventArgs : RdcManEventArgs
    {
        public ContextMenuStrip ContextMenuStrip { get; }

        public ContextMenuStripEventArgs(IPluginContext pluginContext, ContextMenuStrip contextMenuStrip) : base(pluginContext)
        {
            ContextMenuStrip = contextMenuStrip ?? throw new ArgumentNullException($"({nameof(ContextMenuStripEventArgs)}) {nameof(contextMenuStrip)} cannot be null");
        }
    }
}