﻿using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using Newtonsoft.Json.Linq;

namespace SpaceWarpPatcher;

internal static class SwinfoTransformer
{
    
    internal static JObject TransformSwinfo(JObject swinfo, string hash, DirectoryInfo directoryInfo)
    {
        var addressables = Path.Combine(directoryInfo.FullName, "addressables");
        string catalog = null;
        if (Directory.Exists(addressables))
        {
            var toBecomeCatalog = Path.Combine(addressables, "catalog.json");
            if (File.Exists(toBecomeCatalog))
            {
                catalog = toBecomeCatalog;
            }
        }

        string entryPoint = swinfo.TryGetValue("EntryPoint", out var value) ? value.Value<string>() : null;
        if (entryPoint != null) {
            var dll = directoryInfo.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (dll != null)
            {
                entryPoint = dll.FullName;
            }
        }

        JObject target = new JObject();
        if (catalog == null)
        {
            if (entryPoint == null)
            {
                target["APIVersion"] = "0.0.1.1"; 
                target["ModVersion"] = swinfo["version"];
                target["ModName"] = swinfo["name"];
                target["ModAuthor"] = swinfo["author"];
                target["ModDescription"] = swinfo["description"];
                target["Hash"] = hash;
            }
            else
            {
                target["APIVersion"] = "0.0.1.1"; 
                target["ModVersion"] = swinfo["version"];
                target["ModName"] = swinfo["name"];
                target["ModAuthor"] = swinfo["author"];
                target["ModDescription"] = swinfo["description"];
                target["EntryPoint"] = entryPoint;
                target["Hash"] = hash;
            }
        }
        else
        {
            if (entryPoint == null)
            {
                target["APIVersion"] = "0.0.1.1"; 
                target["ModVersion"] = swinfo["version"];
                target["ModName"] = swinfo["name"];
                target["ModAuthor"] = swinfo["author"];
                target["ModDescription"] = swinfo["description"];
                target["Catalog"] = catalog;
                target["Hash"] = hash;
            }
            else
            {
                target["APIVersion"] = "0.0.1.1"; 
                target["ModVersion"] = swinfo["version"];
                target["ModName"] = swinfo["name"];
                target["ModAuthor"] = swinfo["author"];
                target["ModDescription"] = swinfo["description"];
                target["EntryPoint"] = entryPoint;
                target["Catalog"] = catalog;
                target["Hash"] = hash;
            }
        }

        return target;
    }
    
    private static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    internal static void TransformModSwinfos()
    {
        var dir = new DirectoryInfo(Path.Combine(Paths.GameRootPath, "GameData", "Mods"));
        if (!dir.Exists) return;
        var allSwinfos =
            dir.EnumerateFiles("swinfo.json",
                SearchOption.AllDirectories);
        foreach (var swinfo in allSwinfos)
        {
            var directory = swinfo.Directory!;
            var target = Path.Combine(directory.FullName, "modinfo.json");
            var swinfoText = File.ReadAllText(swinfo.FullName);
            var swinfoData = JObject.Parse(swinfoText);
            var hash = GetHashString(swinfoText);
            var toCompareHash = "";
            if (File.Exists(target))
            {
                var targetModInfo = JObject.Parse(File.ReadAllText(target));
                if (targetModInfo.ContainsKey("Hash"))
                {
                    toCompareHash = (string)(targetModInfo.GetValue("Hash") as JValue)?.Value;
                }
                else
                {
                    toCompareHash = hash;
                }
            }
            if (hash == toCompareHash) continue;
            File.WriteAllText(target,TransformSwinfo(swinfoData,hash, directory).ToString());
        }
    }
}