namespace RdcManPluginFix.Interfaces
{
    using System;
    using RdcMan;

    public interface IPluginFix : IPlugin
    {
        event EventHandler<EventArgs> OnContextMenuEvent;

        event EventHandler<EventArgs> OnDockServerEvent;

        event EventHandler<EventArgs> OnUndockServerEvent;

        event EventHandler<EventArgs> PostLoadEvent;

        event EventHandler<EventArgs> PreLoadEvent;

        event EventHandler<EventArgs> SaveSettingsEvent;

        event EventHandler<EventArgs> ShutdownEvent;
    }
}