# RdcManPluginFix

 Library which fixes and extends the undocumented plugin capabilities of Microsoft's [Remote Desktop Connection Manager](https://www.microsoft.com/en-gb/download/details.aspx?id=44989) (RDCMan).

## Referencing RDCMan.exe

To be able to develop a plugin you are going to need to reference **RDCMan.exe** in your project.

Note: These are instructions for **Visual Studio 2017**.

1. Start a new **Class Library (.NET Framework)** project
1. From **Project** press **Add Reference...**
1. At the bottom of the **Reference Manager** window press **Browse...**
1. Navigate to your install location for RDCMan
   * Default install location: _C:/Program Files (x86)/Microsoft/Remote Desktop Connection Manager/_
1. Select **RDCMan.exe** and Press **Add**
1. It will also be worth adding **System.Windows.Forms** and **System.ComponentModel.Composition** under **Assemblies** to your references while you are here as these will be needed later
1. You will now be able to access and use components under the **RdcMan** namespace

## Getting RDCMan to load a Plugin

For a plugin to be loaded by RDCMan it needs to meet 4 requirements:

* The plugin file name must match the pattern `Plugin.*.dll`
* The plugin file must be located at the location of the calling assembly (i.e. in the folder where **RDCMan.exe** resides)
* The plugin must export the `RdcMan.IPlugin` interface and therefore...
* The plugin must implement the `RdcMan.IPlugin` interface

If you don't want your assembly to be prefixed with `Plugin.` you can add a post build event to change the name of the produced `.dll`.

```cmd
move $(TargetPath) $(TargetDir)Plugin.$(TargetFileName)
```

### Exporting IPlugin

Exporting the `RdcMan.IPlugin` interface is rather straightforward, you just need to add the `System.ComponentModel.Composition.Export` attribute to your plugin class.

```cs
using System.ComponentModel.Composition;
using RdcMan;

[Export(typeof(IPlugin)]
public class MyPlugin : IPlugin
{
    /*
        Interface implementations
    */
}
```

## IPlugin Interface and Fixes

The `RdcMan.IPlugin` interface contains the functions which the main application will call.

### OnContextMenu

```cs
void OnContextMenu(ContextMenuStrip contextMenuStrip, RdcTreeNode node);
```

Triggered when the user right clicks or presses the menu key on any object in the TreeView on the left of the application.

Parameter        | Information
-----------------|----------------------------------------------------------------------------------------------------
contextMenuStrip | The context menu which is to be shown to the user
node             | What node the context menu was induced on, this will be `null` if it's the background's context menu

### OnDockServer

```cs
void OnDockServer(ServerBase server);
```

This function cannot be triggered so a fix called `BeginningOfOnUndockServer` has been provided in the `RdcManPluginFix` class of this library to fix this.

Note: Best guess of intended functionality has been made for this function when it comes to how it's triggered and what parameter it passes.

With `BeginningOfOnUndockServer` call in place this function will be triggered when the user closes the undocked server's window or uses the `Dock` option on the servers context menu.

Parameter | Information
----------|-------------------------------------------------------------
server    | The server which was being displayed in the undocked window

### OnUndockServer

```cs
void OnUndockServer(IUndockedServerForm form);
```

Triggered when the user uses the `Undock` or `Undock and connect` option on a servers context menu.

Parameter | Information
----------|-------------------------------------------------------------
form      | An object which contains the server which was undocked and the tool bar of the newly undocked window

The `BeginningOfOnUndockServer` function is to be called at the beginning of your implementation of this function, you are to it pass your entire `IPlugin` implementation and the `IUndockedServerForm` object passed to this function.

```cs
public void OnUndockServer(IUndockedServerForm form)
{
    RdcManPluginFix.BeginningOfOnUndockServer(this, form);

    /*
        Implementation of OnUndockServer
    */
}
```

### PostLoad

```cs
void PostLoad(IPluginContext context);
```

Triggered after any `.rdg` file from a previous session has been loaded and after all `PreLoad` functions of all plugins have been called.

Parameter | Information
----------|-------------
context   | An object which contains the mainWindow object and the TreeView object which holds the servers and groups

### PreLoad

```cs
void PreLoad(IPluginContext context, XmlNode xmlNode);
```

Triggered once when RDCMan firsts loads the plugin.

Parameter | Information
----------|-------------
context   | An object which contains the mainWindow object and the TreeView object which holds the servers and groups
xmlNode   | Always `null` and if `SaveSettings` has passed out a `XmlNode` in a previous session a warning about a previous plugin not being found will be shown. Requires `BeginningOfStaticConstructor` in `RdcManPluginFix` to be called.

`BeginningOfStaticConstructor` is to be called in your `IPlugin` implementation in it's static constructor. When this is done `xmlNode` will contain the `XmlNode` passed out in a previous sessions `SaveSettings` call. This `XmlNode` is is wrapped in an extra `<plugin>` element. This will be `null` if this is the first time this plugin is being used or if no `XmlNode` is passed out of `SaveSettings`.

```cs
public class MyPlugin : IPlugin
{
    static MyPlugin()
    {
        RdcManPluginFix.BeginningOfStaticConstructor();
    }

    /*
        Interface implementations
    */
}
```

There is an optional fix called `BeginningOfPreLoadMethod` in `RdcManPluginFix` which is to be placed at the beginning of your `PreLoad` implementation. It takes the passed `XmlNode` and another `XmlNode` reference where it will place a new `XmlNode` which doesn't have the wrapping `<plugin>` element.

```cs
public void PreLoad(IPluginContext context, XmlNode xmlNode)
{
    RdcManPluginFix.BeginningOfPreLoadMethod(xmlNode, out xmlNode);
    /*
        Implementation of PreLoad
    */
}
```

### SaveSettings

```cs
XmlNode SaveSettings();
```

Triggered when the user goes to the `Options` window in `Tools` and presses `OK` or when the application is exited.

Return    | Information
----------|-------------
`XmlNode` | An `XmlNode` object which is serialized and added to the `<PluginSettings><plugins>` node of  **%LOCALAPPDATA%\Microsoft\Remote Desktop Connection Manager\RDCMan.settings**

### Shutdown

```cs
void Shutdown();
```

Triggered when the application is exited.

## PluginFix

All information in the previous section about fixing your plugin can be disregarded as I've made a class `RdcManPluginFix.PluginFix` which you just need to inherit and all fixes will be applied. You then don't touch the IPlugin functions but subscribe to the events of the `PluginFix` class.

Note: You will still need to `[Export(typeof(IPlugin))]` your implementation.

### EventArgs

EventArgs                 | Inherits From   | Properties
--------------------------|-----------------|------------
RdcManEventArgs           | EventArgs       | `IPluginContext PluginContext { get; }`
ContextMenuStripEventArgs | RdcManEventArgs | `ContextMenuStrip ContextMenuStrip { get; }`
MenuStripEventArgs        | RdcManEventArgs | `MenuStrip MenuStrip { get; }`
XmlNodeEventArgs          | RdcManEventArgs | `XmlNode XmlNode { get; set; }`

### Events

All events use the standard `delegate void StandardDelegate(object sender, EventArgs e)` delegate to allow the passing of `EventArgs.Empty`.

Event               | Replaces Function | Object Type      | EventArgs Type              | Information
--------------------|-------------------|------------------|-----------------------------|-------------
OnContextMenuEvent  | OnContextMenu     | `RdcTreeNode`    | `ContextMenuStripEventArgs` | `Object` is what node in the `TreeView` the context menu belongs to or `null` if it was the background. `EventArgs` contain the context menu which is about to be shown to the user
OnDockServerEvent   | OnDockServer      | `ServerBase`     | `RdcManEventArgs`           | `Object` is the server that has just been docked. `EventArgs` contains the current state of the RDCMan application
OnUndockServerEvent | OnUndockServer    | `ServerBase`     | `MenuStripEventArgs`        | `Object` is the server that has just been undocked. `EventArgs` contains the `MainMenuStrip` of the new window which is about to be shown to the user
PostLoadEvent       | PostLoad          | `IPluginContext` | `RdcManEventArgs`           | `Object` is the current state of the RDCMan application. `EventArgs` contains the current state of the RDCMan application
PreLoadEvent        | PreLoad           | `IPluginContext` | `XmlNodeEventArgs`          | `Object` is the current state of the RDCMan application. `EventArgs` contains the last `XmlNode` which was set in the `EventArgs` of the `SaveSettingsEvent` in a previous session or `XmlNode` will be `null` if the plugin has never been ran or a `SaveSettingsEvent` has never set the `XmlNode` before
SaveSettingsEvent   | SaveSettings      | `IPluginContext` | `XmlNodeEventArgs`          | `Object` is the current state of the RDCMan application. `EventArgs` contains a `null` `XmlNode` which you are to set, this will be added to the `<PluginSettings><plugins>` node of  **%LOCALAPPDATA%\Microsoft\Remote Desktop Connection Manager\RDCMan.settings**
ShutdownEvent       | Shutdown          | `IPluginContext` | `RdcManEventArgs`           | `Object` is the current state of the RDCMan application. `EventArgs` contains the current state of the RDCMan application

## Creating TreeView Nodes

There are 2 types of `RdcTreeNode` that we can create; `Group` and `Server`. We don't have the ability to create the other 2 types, `SmartGroup` and `FileGroup`.

We can create a `Group` or `Server` by using the static `Create` methods of there classes. They both ask for a `GroupBase`, this will either be another `Group` in the `TreeView` or `RootNode` which can be got from the `IPluginContext.Tree`.

```cs
public void PostLoad(IPluginContext pluginContext)
{
    // Creates a new Group with the name "MyGroup"
    // Group is added to the TreeView at the root level
    Group myGroup = Group.Create("MyGroup", pluginContext.Tree.RootNode);

    // Creates a new Server with the name "localhost" and display name "MyServer"
    // Server is added to the group which was created above
    Server myServer = Server.Create("localhost", "MyServer", myGroup);
}
```

## Example Project

I have created a [Demo Project](https://github.com/Geosong/RdcManDemoPlugin) on github which uses this fix and demonstrates how to use the events.

## Future Improvements

* Extend events
  * `ServerAddedEvent`
  * `ServerChangedEvent`
  * `ServerRemovedEvent`
  * `ServerConnectedEvent`
  * `ServerDisconnectedEvent`
  * `GroupAddedEvent`
  * `GroupChangedEvent`
  * `GroupRemovedEvent`
* Create proxy `SmartGroup` class to allow creation and manipulation of `SmartGroup`
* Create extension method to allow creation of `FileGroup`
* Extend documentation into wiki-page which explains how to create RDCMan plugins
