namespace RdcManPluginFix
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Xml;
    using RdcMan;

    public static class RdcManPluginFix
    {
        #region Constructors

        static RdcManPluginFix()
        {
            FixPreLoadMethod();
        }

        #endregion Constructors

        #region User Methods

        /// <summary>
        /// Must be called at the beginning of the OnUndockServer method of plugin
        /// Fixes OnDockServer method so it's called when an undocked server's window is closed
        /// </summary>
        /// <param name="plugin">The current plugin using this method (Pass 'this')</param>
        /// <param name="undockedServerForm">Form which is passed to the OnUndockServer method</param>
        public static void BeginningOfOnUndockServer(IPlugin plugin, IUndockedServerForm undockedServerForm)
        {
            Form form = undockedServerForm as Form;

            form.FormClosed += (s, a) =>
            {
                plugin.OnDockServer(undockedServerForm.Server);
            };
        }

        /// <summary>
        /// Optionally called at the beginning of the PreLoad method to remove wrapping &lt;plugin&gt; node
        /// </summary>
        /// <param name="xmlNode">Original node passed to PreLoad method</param>
        /// <param name="fixedXmlNode">Node to be used instead of original xmlNode</param>
        public static void BeginningOfPreLoadMethod(XmlNode xmlNode, out XmlNode fixedXmlNode)
        {
            fixedXmlNode = xmlNode?.FirstChild;
        }

        /// <summary>
        /// Must be called in a static constructor for the plugin class
        /// Ensures this assembly has been loaded and static constructor of this class has been called which fixes the PreLoad method
        /// </summary>
        public static void BeginningOfStaticConstructor()
        {
        }

        #endregion User Methods

        #region Fix Methods

        /// <summary>
        /// RdcMan isn't consistent on how it checks / assigns the name of the assembly plugin (It uses IPlugin.GetType().Assembly.GetName().Name and IPlugin.GetType().AssemblyQualifiedName)
        /// The effect of this is it not recognising the saved settings from a previous session (Settings location "%localappdata%\Microsoft\Remote Desktop Connection Manager\RDCMan.settings")
        /// This method will be invoked after it has made a list of previous settings but before it checks them against any plugins
        /// It will rename the keys in the plugin dictionary to be the simple name of the plugin assemblies to allow other parts of the application to work as expected
        /// </summary>
        private static void FixPreLoadMethod()
        {
            var rcdmanAssembly = typeof(IPluginContext).Assembly;
            var pluginsDictionary = rcdmanAssembly.InvokeNonPublicStaticPropertyGetMethod<object>("RdcMan.Program", "Plugins", null);
            var keyCollection = pluginsDictionary.InvokePublicPropertyGetMethod<object>("Keys", null);
            var pluginsKeys = keyCollection.InvokeGenericExtensionMethod<List<string>>("ToList", typeof(Enumerable), new Type[] { typeof(string) }, null);

            foreach (var assemblyQualifiedName in pluginsKeys)
            {
                var assemblyName = assemblyQualifiedName.Split(',')[1].Trim();
                var pluginContext = pluginsDictionary.InvokePublicPropertyGetMethod<object>("Item", assemblyQualifiedName);
                pluginsDictionary.InvokePublicMethod<object>("Remove", assemblyQualifiedName);
                pluginsDictionary.InvokePublicMethod<object>("Add", assemblyName, pluginContext);
            }
        }

        #endregion Fix Methods

        #region Reflection Extension Methods

        private static T InvokeGenericExtensionMethod<T>(this object obj, string extensionMethod, Type extenstionClass, Type[] typeArguments, params object[] parameters)
        {
            if (parameters == null)
            {
                parameters = new object[] { };
            }

            object[] extensionParameters = new object[] { obj }.Concat(parameters).ToArray();
            MethodInfo extensionMethodInfo = extenstionClass.GetMethod(extensionMethod);
            extensionMethodInfo = extensionMethodInfo.MakeGenericMethod(typeArguments);
            return (T)extensionMethodInfo.Invoke(null, extensionParameters);
        }

        private static T InvokeNonPublicStaticPropertyGetMethod<T>(this Assembly assembly, string obj, string property, params object[] parameters)
        {
            Type objType = assembly.GetType(obj);
            MethodInfo methodInfo = objType.GetProperty(property, BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true);
            return (T)methodInfo.Invoke(null, parameters);
        }

        private static T InvokePublicMethod<T>(this object obj, string method, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(method);
            return (T)methodInfo.Invoke(obj, parameters);
        }

        private static T InvokePublicPropertyGetMethod<T>(this object obj, string property, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo methodInfo = type.GetProperty(property).GetMethod;
            return (T)methodInfo.Invoke(obj, parameters);
        }

        #endregion Reflection Extension Methods
    }
}