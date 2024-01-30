using HarmonyLib;
using SRML;
using SRML.SR;
using SRML.Console;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace MoreTitles
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        public static List<Sprite> logos = new List<Sprite>();
        static Sprite originalLogo;
        static Sprite _activeLogo;
        public static Sprite activeLogo
        {
            get => _activeLogo;
            set
            {
                if (!originalLogo)
                {
                    foreach (var c in Resources.FindObjectsOfTypeAll<Sprite>())
                        if (c.name == "logoTitle")
                            originalLogo = c;
                    _activeLogo = originalLogo;
                }
                if (!value || _activeLogo == value)
                    return;
                foreach (var i in Resources.FindObjectsOfTypeAll<Image>())
                    if (i.sprite == _activeLogo || i.sprite == originalLogo)
                        i.sprite = value;
                _activeLogo = value;
            }
        }

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            var filePrefix = modAssembly.GetName().Name + ".";
            foreach (var n in modAssembly.GetManifestResourceNames())
                if (n.EndsWith(".png") && n.StartsWith(filePrefix + "SRLogo_"))
                    logos.Add(LoadImage(n.Remove(0, filePrefix.Length)).CreateSprite());
            try
            {
                if (!Directory.Exists("MoreTitles"))
                    Directory.CreateDirectory("MoreTitles");
                foreach (var f in Directory.GetFiles("MoreTitles"))
                {
                    if (f.EndsWith(".png"))
                    {
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(File.ReadAllBytes(f));
                        tex.filterMode = FilterMode.Bilinear;
                        logos.Add(tex.CreateSprite());
                    }
                }
            } catch (Exception e)
            {
                LogError($"An error occured while trying to load the extra logos from the MoreTitles folder\n{e}");
            }
            SRCallbacks.OnMainMenuLoaded += (x) =>
            {
                Main.Log(logos.Count.ToString());
                var r = Randoms.SHARED.GetInt(logos.Count + 1) - 1;
                if (r == -1)
                    activeLogo = originalLogo;
                else
                    activeLogo = logos[r];
            };
        }
        internal static void Log(string message) => Console.Log($"[{modName}]: " + message);
        internal static void LogError(string message) => Console.LogError($"[{modName}]: " + message);
        internal static void LogWarning(string message) => Console.LogWarning($"[{modName}]: " + message);
        internal static void LogSuccess(string message) => Console.LogSuccess($"[{modName}]: " + message);

        internal static Texture2D LoadImage(string filename)
        {
            var a = modAssembly;
            var spriteData = a.GetManifestResourceStream(a.GetName().Name + "." + filename);
            if (spriteData == null)
            {
                LogError(filename + " does not exist in the assembly");
                return null;
            }
            var rawData = new byte[spriteData.Length];
            spriteData.Read(rawData, 0, rawData.Length);
            var tex = new Texture2D(0, 0);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }
    }

    static class ExtentionMethods
    {
        public static Sprite CreateSprite(this Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);
    }
}