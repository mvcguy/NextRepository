﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Repository.MySql.UnitTests.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Repository.MySql.UnitTests.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to 	
        ///	DROP DATABASE IF EXISTS NextDatalayer;
        ///
        ///	CREATE DATABASE NextDataLayer; 
        ///
        ///	USE NextDataLayer;
        ///
        ///	CREATE TABLE  Products (
        ///			Id int(11)   NOT NULL auto_increment,
        ///			Name varchar(180) NULL,
        ///			Description varchar(250) NULL,
        ///			PRIMARY KEY (Id)
        ///	);
        ///		
        ///
        ///
        ///	CREATE TABLE  ProductsLog (
        ///			Id int(11)  NOT NULL auto_increment,
        ///			Message varchar(500) NULL,
        ///			LastUpdated datetime NULL,
        ///			PRIMARY KEY (Id)
        ///	);
        ///
        ///	INSERT INTO Products (Name,Description) values	(&apos;Galaxy S6&apos;,&apos;3GB RAM 32 Internal Storage&apos;);
        ///.
        /// </summary>
        internal static string CreateDatabase {
            get {
                return ResourceManager.GetString("CreateDatabase", resourceCulture);
            }
        }
    }
}
