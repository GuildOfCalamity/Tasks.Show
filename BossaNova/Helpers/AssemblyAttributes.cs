﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Con = System.Diagnostics.Debug;

namespace Tasks.Show.Helpers
{
    public class AssemblyAttributes
    {
        Assembly _assembly = null;
        readonly string _empty = "N/A";

        #region [Assembly Base Accessors]
        public Assembly GetAssembly
        {
            get
            {
                if (_assembly is null)
                    _assembly = Assembly.GetExecutingAssembly(); //-or- Assembly.GetEntryAssembly()

                return _assembly;
            }
        }

        public Version AssemblyVersion
        {
            get => Assembly.GetExecutingAssembly().GetName().Version; //-or- Assembly.GetEntryAssembly()
        }

        public string AssemblyPath
        {
            get => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //get => Path.GetDirectoryName(Assembly.GetEntryAssembly().GetName().CodeBase).Replace("file:\\", "");
        }

        public string AssemblyUser
        {
            get => Environment.UserDomainName + "\\" + Environment.UserName;
        }

        public Type[] GetTypes
        {
            get => GetAssembly.GetTypes();
        }

        public Type[] GetExportedTypes
        {
            get => GetAssembly.GetExportedTypes();
        }

        public Module[] GetModules
        {
            get => GetAssembly.GetModules();
        }

        public Module[] GetLoadedModules
        {
            get => GetAssembly.GetLoadedModules();
        }

        public AssemblyName[] GetAssemblies
        {
            get => GetAssembly.GetReferencedAssemblies();
        }

        public FileStream[] GetFileStreams
        {
            get => GetAssembly.GetFiles();
        }

        public string[] GetResourceNames
        {
            get => GetAssembly.GetManifestResourceNames();
        }

        public IEnumerable<Attribute> GetCustomAttributes
        {
            get => GetAssembly.GetCustomAttributes();
        }

        public Assembly GetSatellite
        {
            get
            {
                try
                {
                    return GetAssembly.GetSatelliteAssembly(System.Threading.Thread.CurrentThread.CurrentCulture);
                }
                catch (FileNotFoundException ex) // no resource file exists
                {
                    Con.WriteLine($"GetSatellite: {ex.Message}");
                    return null;
                }
            }
        }
        #endregion [Assembly Base Accessors]

        #region [Assembly Attribute Accessors]
        public string AssemblyCulture
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCultureAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyCultureAttribute)attributes[0]).Culture;
            }
        }

        public string AssemblyFramework
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((System.Runtime.Versioning.TargetFrameworkAttribute)attributes[0]).FrameworkName;
            }
        }

        public string AssemblyConfiguration
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyConfigurationAttribute)attributes[0]).Configuration;
            }
        }

        public string AssemblyFileVersion
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyFileVersionAttribute)attributes[0]).Version;
            }
        }

        public string AssemblyInformationalVersion
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;

            }
        }

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "") { return titleAttribute.Title; }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) { return _empty; }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion [Assembly Attribute Accessors]

        #region [Assembly Type Accessors]
        public FieldInfo[] GetFields
        {
            get => GetAssembly.GetType().GetFields();
        }

        public PropertyInfo[] GetProperties
        {
            get => GetAssembly.GetType().GetProperties();
        }

        public MemberInfo[] GetMembers
        {
            get => GetAssembly.GetType().GetMembers();
        }

        public MethodInfo[] GetMethods
        {
            get => GetAssembly.GetType().GetMethods();
        }

        public EventInfo[] GetEvents
        {
            get => GetAssembly.GetType().GetEvents();
        }

        public ConstructorInfo[] GetConstructors
        {
            get => GetAssembly.GetType().GetConstructors();
        }

        public Type[] GetInterfaces
        {
            get => GetAssembly.GetType().GetInterfaces();
        }
        #endregion [Assembly Type Accessors]

        #region [Public Methods]
        public Dictionary<string, Version> GetAllAssemblies()
        {
            int idx = 0; // to prevent key collisions only
            Dictionary<string, Version> values = new Dictionary<string, Version>();
            values.Add($"[{++idx}] {GetAssembly.GetName().Name}", GetAssembly.GetName().Version);
            IOrderedEnumerable<AssemblyName> names = GetAssembly.GetReferencedAssemblies().OrderBy(o => o.Name);
            foreach (var sas in names) { values.Add($"[{++idx}] {sas.Name}", sas.Version); }
            return values;
        }
        #endregion [Public Methods]
    }
}
