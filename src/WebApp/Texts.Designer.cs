﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kundenportal.AdminUi.WebApp {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Texts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Texts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Kundenportal.AdminUi.WebApp.Texts", typeof(Texts).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strukturgruppe erstellen.
        /// </summary>
        internal static string LinkTitleCreateStructureGroup {
            get {
                return ResourceManager.GetString("LinkTitleCreateStructureGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Eine Struktur ist ein Ordner, für den Benutzer berechtigt und eine vordefinierte Ordner- und Dateistruktur angelegt werden kann..
        /// </summary>
        internal static string StructureExplanation {
            get {
                return ResourceManager.GetString("StructureExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Eine Strukturgruppe ist eine Gruppierung von Strukturen, über die zusammengehörende Strukturen gemeinsam angelegt werden können. Ein Beispiel wäre mehrere Strukturen für ein gemeinsames Projekt..
        /// </summary>
        internal static string StructureGroupExplanation {
            get {
                return ResourceManager.GetString("StructureGroupExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Struktur erstellen.
        /// </summary>
        internal static string TitleCreateStructure {
            get {
                return ResourceManager.GetString("TitleCreateStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strukturgruppen.
        /// </summary>
        internal static string TitleStructureGroups {
            get {
                return ResourceManager.GetString("TitleStructureGroups", resourceCulture);
            }
        }
    }
}
