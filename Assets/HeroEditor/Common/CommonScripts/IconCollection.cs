using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.HeroEditor.Common.CommonScripts
{
    /// <summary>
    /// Global object that automatically grabs all required images.
    /// </summary>
    [CreateAssetMenu(fileName = "IconCollection", menuName = "ScriptableObjects/IconCollection")]
    public class IconCollection : ScriptableObject
    {
        public List<Sprite> Backgrounds;

        public static Dictionary<string, IconCollection> Instances = new Dictionary<string, IconCollection>();
        public static IconCollection Active;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Instances = Resources.LoadAll<IconCollection>("").ToDictionary(i => i.Id, i => i);
        }

        public string Id;
        public List<Object> IconFolders;
        public List<ItemIcon> Icons;
        public Sprite DefaultItemIcon;

        /// <summary>
        /// Find item icon by ID.
        /// </summary>
        /// <param name="id">Id of a sprite/icon/item.</param>
        /// <returns></returns>
        public Sprite FindIcon(string id, string expectedType = null)
        {
            if (string.IsNullOrEmpty(id)) return DefaultItemIcon;
            if (id == "Empty") return DefaultItemIcon;

            string baseId = id.Split('#')[0].Trim();

            // Ưu tiên tìm icon đúng loại
            if (!string.IsNullOrEmpty(expectedType))
            {
                var match = Icons.FirstOrDefault(i => i.Id == baseId && i.Path.Contains($"/{expectedType}/"));
                if (match != null) return match.Sprite;
            }

            // Fallback: tìm bất kỳ icon nào có Id khớp
            var icon = Icons.FirstOrDefault(i => i.Id == baseId);
            if (icon != null) return icon.Sprite;

            Debug.LogWarning("Không tìm thấy icon: " + baseId);
            return DefaultItemIcon;
        }
        public ItemIcon FindIconItem(string id, string expectedType = null)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string baseId = id.Split('#')[0].Trim();

            // Ưu tiên theo loại
            if (!string.IsNullOrEmpty(expectedType))
            {
                var matchByType = Icons.FirstOrDefault(i => i.Id == baseId && i.Type == expectedType);
                if (matchByType != null) return matchByType;
            }

            // Fallback nếu không tìm được đúng loại
            return Icons.FirstOrDefault(i => i.Id == baseId);
        }




#if UNITY_EDITOR

        public void Refresh()
        {
            Icons.Clear();

            foreach (var folder in IconFolders)
            {
                if (folder == null) continue;

                var root = AssetDatabase.GetAssetPath(folder);
                var files = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories).ToList();

                foreach (var path in files.Select(i => i.Replace("\\", "/")))
                {
                    var match = Regex.Match(path, @"Assets\/HeroEditor\/(?<Edition>\w+)\/(.+?\/)*Icons\/WithoutBackground\/\w+\/(?<Type>\w+)\/(?<Collection>.+?)\/(.+\/)*(?<Name>.+?)\.png");

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    var edition = match.Groups["Edition"].Value;
                    var collection = match.Groups["Collection"].Value;
                    var type = match.Groups["Type"].Value;
                    var iconName = match.Groups["Name"].Value;
                    var icon = new ItemIcon(edition, collection, type, iconName, path, sprite);

                    if (Icons.Any(i => i.Path == icon.Path))
                    {
                        Debug.LogErrorFormat($"Duplicated icon: {icon.Path}");
                    }
                    else
                    {
                        Icons.Add(icon);
                    }
                }
            }

			Icons = Icons.OrderBy(i => i.Name).ToList();
            EditorUtility.SetDirty(this);
        }

        #endif
    }

}