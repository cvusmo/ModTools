using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModTools.Utilities
{
    public class ModData
    {
        public List<string> Orientations { get; set; } = new List<string>();
        public int Resolution { get; set; }
        public int TargetResolution { get; set; }
        public int SliceCount { get; set; }
        public string BaseDirectory { get; set; }
        public IReadOnlyList<string> TexturesList => texturesList.AsReadOnly();

        private List<string> texturesList = new List<string>();
        private List<string> failedTextures = new List<string>();
        private string textureDirectory;

        internal string TextureDirectory
        {
            get => textureDirectory;
            set
            {
                if (Directory.Exists(value))
                {
                    textureDirectory = value;
                }
                else
                {
                    Debug.LogWarning($"Directory '{value}' does not exist!");
                }
            }
        }
        public void AddTexture(string texture)
        {
            if (!texturesList.Contains(texture))
            {
                texturesList.Add(texture);
                Debug.Log($"Added texture: {texture}");
            }
            else
            {
                Debug.LogWarning($"Texture '{texture}' is already in the list!");
            }
        }
        public void RemoveTexture(string texture)
        {
            if (texturesList.Contains(texture))
            {
                texturesList.Remove(texture);
                Debug.Log($"Removed texture: {texture}");
            }
            else
            {
                Debug.LogWarning($"Texture '{texture}' was not found in the list!");
            }
        }
        internal Dictionary<string, List<string>> GroupTextures(List<string> textureList)
        {
            Dictionary<string, List<string>> groupedTextures = new Dictionary<string, List<string>>();

            Debug.Log($"Total imported textures: {texturesList.Count}");
            foreach (string texture in textureList)
            {
                string baseName = GetBaseName(texture);
                if (!groupedTextures.ContainsKey(baseName))
                {
                    groupedTextures[baseName] = new List<string>();
                }
                groupedTextures[baseName].Add(texture);
            }

            foreach (var group in groupedTextures)
            {
                Debug.Log($"Group: {group.Key}");
                foreach (var texture in group.Value)
                {
                    Debug.Log($" - Texture: {texture}");
                }
            }

            Debug.Log($"Total groups after grouping: {groupedTextures.Count}");
            return groupedTextures;
        }
        private string GetBaseName(string texture)
        {
            return Path.GetFileNameWithoutExtension(texture).Split('_')[0];
        }
    }
}