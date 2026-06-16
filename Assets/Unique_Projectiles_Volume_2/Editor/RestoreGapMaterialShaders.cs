using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class RestoreGapMaterialShaders
{
    const string Root = "Assets/Unique_Projectiles_Volume_2/Materials";
    const string BrokenGuid = "0406db5a14f94604a8c57ccfbc9f3b46";
    static readonly Regex BrokenShaderLine = new Regex(
        @"m_Shader: \{fileID: 4800000, guid: 0406db5a14f94604a8c57ccfbc9f3b46, type: 3\}");

    const string DesktopAdd = "c183104a0d06f5c4780d2ec95079ab3f";
    const string DesktopAb = "9cb3ed36f15f80143a80a3903f518c86";
    const string MobileAdd = "478df33d3f7bde4428b640ec712e6ea9";
    const string MobileAb = "a7d432525b1d71f408e587fece91edc5";
    const string LegacyMobileAdd = "772c876c2069d0b488fb2840de6e4f6b";
    const string LegacyMobileAb = "0850b508c5c025442a9d0a4cfe05c747";
    const string Ice = "7f173d1e98ec5884cbcb1ffe4ec53c52";

    [MenuItem("Tools/Unique Projectiles/Przywroc shadery GAP na materialach")]
    public static void RestoreAll()
    {
        var restored = 0;
        foreach (var path in Directory.GetFiles(Root, "*.mat", SearchOption.AllDirectories))
        {
            var assetPath = path.Replace('\\', '/');
            var index = assetPath.IndexOf("Assets/");
            if (index >= 0)
                assetPath = assetPath.Substring(index);

            var text = File.ReadAllText(path);
            if (!text.Contains(BrokenGuid))
                continue;

            var targetGuid = ResolveShaderGuid(assetPath, Path.GetFileNameWithoutExtension(path));
            var replacement = "m_Shader: {fileID: 4800000, guid: " + targetGuid + ", type: 3}";
            var newText = BrokenShaderLine.Replace(text, replacement);
            if (newText == text)
                continue;

            File.WriteAllText(path, newText);
            restored++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("GAP Materials", "Przywrocono shadery GAP na " + restored + " materialach.", "OK");
    }

    static string ResolveShaderGuid(string path, string name)
    {
        var n = name.ToLowerInvariant();
        var p = path.Replace('\\', '/').ToLowerInvariant();

        if (n.Contains("icespikes"))
            return Ice;

        if (p.Contains("/legacy/"))
        {
            if (n.Contains("mobab") || n.Contains("abscroll"))
                return LegacyMobileAb;
            return LegacyMobileAdd;
        }

        if (n.Contains("mobab") || n.Contains("vol2_mobab"))
            return MobileAb;

        if (n.Contains("mobadd") || n.Contains("vol2_mobadd") || n.Contains("square04_mobadd"))
            return MobileAdd;

        if (n.Contains("abscroll") || n.Contains("vol2_ab") || n.Contains("_ab"))
            return DesktopAb;

        if (n.Contains("addscroll") || n.Contains("vol2_add") || n.Contains("_add"))
            return DesktopAdd;

        if (n.Contains("ground") || n.Contains("sphere"))
            return DesktopAb;

        return DesktopAdd;
    }
}
