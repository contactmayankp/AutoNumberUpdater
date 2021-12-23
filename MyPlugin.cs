using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Sdmsols.XTB.AutoNumberUpdater
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "AutoNumberUpdater"),
        ExportMetadata("Description", "Updates existing records where auto number field is missing value"),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "AAABAAEAICAQAAAAAADoAgAAFgAAACgAAAAgAAAAQAAAAAEABAAAAAAAgAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAACAAAAAgIAAgAAAAIAAgACAgAAAgICAAMDAwAAAAP8AAP8AAAD//wD/AAAA/ wD / AP//AAD///8A8Hd3d3d3d3d3d3d3d3d3d/AAAAAAAAAAAAAAAAAAAADwAAAAAAAAAAAAAAAAAAAA8AcAcAAHBwAAAHAAAHAHcPB3j / +HCIiIgAeIB4gHiHDwD / h / +A////8A/wf/d/8A8AcAd/8P8Hd / gP9///f/APcHf//3D/BwD/D/fwD3/wDwD/+HcA/wAA/w//8A//8A8A/wB4AP+Ij/cP/3AA//APAA///wD///93D/8AAP/wDwAAAAcAAAAAAAAAAABwAA8ABwAAAAAAAAAAAAAAAAAPAAAAAAAAAAd3AAAAAAAADwAAAAAAAAAAAAAAAAAAAA8AAAAAAAAABwAAAAAAAAAPAAAAAAAAB393AAAAAAAADwAAAAAAAHf / gHAAAAAAAA8AAAAAAHcPf / cAAAAAAAAPAAAAAAcH9 / f / hwAAAAAADwAAAAAAB / 9 / f4AAAAAAAA8AAAAAAAd / h / DwAAAAAAAPAAAAAAAH8Ph / AAAAAAAADwAAAAAAB38PePAAAAAAAA8AAAAAAHd48PfwAAAAAAAPAAAAAAcId3h3iAAAAAAADwAAAAAAAAB3cAAAAAAAAA8AAAAAAAcHdwcHAAAAAAAPAAAAAAAAAAAAAAAAAAAADwAAAAAAAAAAAAAAAAAAAA93d3d3d3d3d3d3d3d3d3d/////////////////////8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA =="),
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFYAAABQCAYAAACDD4LqAAAAAXNSR0IArs4c6QAAEmVJREFUeF7tnQeQVEUXhS9mxYSKBBUDGAGlLAOIiWBCEdEyIhgBRbRMKJhFUcEABjAAigEVzFkJYiJI0jKgKGZQMANGRPav7+qZuj5nZmdYnNl/mVc19WZm3+vXffr0ubdv356tVlZWVrZ48WJbaaWVrKyszEpHxRGoVq2aVQNYAF2yZImtsMIKFS+1VIKlgD3ooIMc2BJjlw0rnnvuub8Y26pVqxKoywZTL+XFF1/8C9iWLVs6sFC4dFQcgRSwLVq0cGBLUlBxUCnh5Zdf/ouxAnbZFFsq5aWXXioB+1/QIAUsGotXUAyN5Zl//vmny9CKK66Y0nppfjrt554oW3qva4vRjthB48aN+6fx+i96L1lmbLT8ZyYn8qOZrAAwB4DjW+u6lVdeOWULIrDuN/79im6jviu07SgKsEmgAQ4wea266qopJoqV6gjOXBNBpCxdB6D6HL8vhrdTNGCTbAMUyYBAkTTpb+qQ6L0IdO79/ffffVqu73RdMWShKMBKUwEKtv7xxx/GEAccyQASwAFQYimfFdPgzL28IlMpg8+UyXtekpNCyJyeUVRgAUAAAsBqq61mtWrVsho1avj3G220kTVr1sy22mormz9/fsq40TGrrLKK/fjjj/baa6/ZF198Yb/++qv98ssv9tVXX9nChQtTGEYGV0lgAUNGRSzjvN9++9mmm25qa6+9tq255pq27rrrevsBC5AA7OOPP7YPP/zQ7+cFqBtssIFtscUW3gl0QL169fz777//3oH96aef7PPPP7dZs2bZe++958wv5FEwxsqVkqVftGiRNWjQwK688kp79NFH7dNPP7Wff/7ZfvjhBwdlrbXWMiYtO+20kwMG6MgF77kXdsLid99911599VX75ptvHLwNN9zQ1lhjDe8o4h9cf+aZZ1r16tULiasVDFhZbgEM82DcaaedZueee641bdrUgWTow+QFCxY4GKuvvroDwnfvvPOOXXLJJfbtt9/a8ccfbyeffHLK9YKlgPjWW2/Z2LFjbeLEibblllta586drVOnTt4xhTwKCqysvNyrrbfe2ht+66232lVXXWWPPfaYvf32285ehvell17qDASwGTNm2IUXXujywD0nnXSSewG4Z/fee6/ddddd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"),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class MyPlugin : PluginBase,IPayPalPlugin
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new AutoNumberUpdater();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }

        public string DonationDescription => "Auto Number Updater";
        public string EmailAccount => "mayank.pujara@gmail.com";
    }
}