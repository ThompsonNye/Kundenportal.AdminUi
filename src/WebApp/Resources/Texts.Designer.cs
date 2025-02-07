﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kundenportal.AdminUi.WebApp.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Texts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Texts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Kundenportal.AdminUi.WebApp.Resources.Texts", typeof(Texts).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Erstellen.
        /// </summary>
        public static string ButtonTextCreate {
            get {
                return ResourceManager.GetString("ButtonTextCreate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name.
        /// </summary>
        public static string LabelEditStructureGroupName {
            get {
                return ResourceManager.GetString("LabelEditStructureGroupName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bitte geben Sie den Namen ein.
        /// </summary>
        public static string PlaceholderEditStructureGroupName {
            get {
                return ResourceManager.GetString("PlaceholderEditStructureGroupName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Eine Struktur ist ein Ordner, für den Benutzer berechtigt und eine vordefinierte Ordner- und Dateistruktur angelegt werden können..
        /// </summary>
        public static string StructureExplanation {
            get {
                return ResourceManager.GetString("StructureExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Eine Strukturgruppe ist eine Gruppierung von Strukturen, über die zusammengehörende Strukturen gemeinsam angelegt werden können. Ein Beispiel wäre mehrere Strukturen für ein gemeinsames Projekt..
        /// </summary>
        public static string StructureGroupExplanation {
            get {
                return ResourceManager.GetString("StructureGroupExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Struktur erstellen.
        /// </summary>
        public static string TitleCreateStructure {
            get {
                return ResourceManager.GetString("TitleCreateStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strukturgruppe erstellen.
        /// </summary>
        public static string TitleCreateStructureGroup {
            get {
                return ResourceManager.GetString("TitleCreateStructureGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strukturgruppe bearbeiten.
        /// </summary>
        public static string TitleEditStructureGroup {
            get {
                return ResourceManager.GetString("TitleEditStructureGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Strukturgruppen.
        /// </summary>
        public static string TitleStructureGroups {
            get {
                return ResourceManager.GetString("TitleStructureGroups", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Der Pfadteil enthält ungültige Zeichen, ungültige Zeichen sind: &lt; &gt; ? &quot; : | / \ * . ’ # %.
        /// </summary>
        public static string ValidationErrorEditStructureGroupNameContainsInvalidCharacters {
            get {
                return ResourceManager.GetString("ValidationErrorEditStructureGroupNameContainsInvalidCharacters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Das Feld {0} muss ausgefüllt werden..
        /// </summary>
        public static string ValidationErrorFieldRequired {
            get {
                return ResourceManager.GetString("ValidationErrorFieldRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Das Feld {0} darf maximal {1} Zeichen lang sein..
        /// </summary>
        public static string ValidationErrorFieldTooLong {
            get {
                return ResourceManager.GetString("ValidationErrorFieldTooLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Eine Strukturgruppe mit diesem Namen existiert bereits..
        /// </summary>
        public static string ValidationErrorStructureGroupFolderExists {
            get {
                return ResourceManager.GetString("ValidationErrorStructureGroupFolderExists", resourceCulture);
            }
        }
    }
}
