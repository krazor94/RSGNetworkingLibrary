// Tips from https://forum.unity3d.com/threads/c-script-template-how-to-make-custom-changes.273191/
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

internal sealed class ScriptKeywordProcessor : UnityEditor.AssetModificationProcessor
{
    public static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta", "");
        int index = path.LastIndexOf(".");
        if (index < 0)
            return;

        string file = path.Substring(index);
        if (file != ".cs" && file != ".js")
            return;

        index = Application.dataPath.LastIndexOf("Assets");
        path = Application.dataPath.Substring(0, index) + path;
        if (!System.IO.File.Exists(path))
            return;

        string fileContent = System.IO.File.ReadAllText(path);
        // At this part you could actually get the name from Windows user directly or give it whatever you want
        fileContent = fileContent.Replace("#AUTHOR#", "Jake Aquilina");
        fileContent = fileContent.Replace("#CREATIONDATE#", System.DateTime.Now.ToString("dd/MM/yy"));
        fileContent = fileContent.Replace("#CREATIONTIME#", string.Format("{0:hh:mm tt}", System.DateTime.Now));
        fileContent = fileContent.Replace("#COMPANY#", "RealSoft Games");
        fileContent = fileContent.Replace("#WEBSITE#", "https://www.realsoftgames.com/");

        fileContent = fileContent.Replace("#NAMESPACE#", "RealSoftGames");
        System.IO.File.WriteAllText(path, fileContent);
        AssetDatabase.Refresh();
    }
}

#endif