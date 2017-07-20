namespace RdcManPluginFix.Events
{
    using System;
    using RdcMan;

    public class RdcManEventArgs : EventArgs
    {
        public IPluginContext PluginContext { get; }

        public RdcManEventArgs(IPluginContext pluginContext)
        {
            PluginContext = pluginContext;
        }
    }
}