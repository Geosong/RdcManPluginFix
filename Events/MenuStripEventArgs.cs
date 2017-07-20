namespace RdcManPluginFix.Events
{
    using System;
    using System.Windows.Forms;
    using RdcMan;

    public class MenuStripEventArgs : RdcManEventArgs
    {
        public MenuStrip MenuStrip { get; }

        public MenuStripEventArgs(IPluginContext pluginContext, MenuStrip menuStrip) : base(pluginContext)
        {
            MenuStrip = menuStrip ?? throw new ArgumentNullException($"({nameof(MenuStripEventArgs)}) {nameof(menuStrip)} cannot be null");
        }
    }
}