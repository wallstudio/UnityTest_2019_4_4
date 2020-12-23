using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class DynamicBackportingEditor : Object
{
    // [InitializeOnLoadMethod]
    // static void ImpoerDllIfNeed()
    // {
    //     try
    //     {
    //         var projectCoreDll = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("System.Core").Single());
    //         Assert.IsTrue(projectCoreDll.EndsWith("System.Core.dll"));
    //     }
    //     catch
    //     {
    //         var editorDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
    //         // C:\Program Files\Unity\Hub\Editor\2019.4.4f1\Editor\Unity.exe
    //         var coreDll = Path.Combine(editorDir, "Data", "MonoBleedingEdge", "lib", "mono", "unityjit", "System.Core.dll");
    //         var importDir = Path.Combine("Assets", "Plugins", Path.GetFileName(coreDll));
    //         File.Copy(coreDll, importDir);
    //         Debug.Log("Imported System.Core.dll");
    //     }

    //     try
    //     {
    //         var projectCsharpDll = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Microsoft.CSharp").Single());
    //         Assert.IsTrue(projectCsharpDll.EndsWith("Microsoft.CSharp.dll"));
    //     }
    //     catch
    //     {
    //         var editorDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
    //         // C:\Program Files\Unity\Hub\Editor\2019.4.4f1\Editor\Unity.exe
    //         var csharpDll = Path.Combine(editorDir, "Data", "MonoBleedingEdge", "lib", "mono", "unityjit", "Microsoft.CSharp.dll");
    //         var importDir = Path.Combine("Assets", "Plugins", Path.GetFileName(csharpDll));
    //         File.Copy(csharpDll, importDir);
    //         Debug.Log("Imported Microsoft.CSharp.dll");
    //     }
    // }
}
