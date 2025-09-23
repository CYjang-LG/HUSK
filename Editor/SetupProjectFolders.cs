// Assets/Editor/SetupProjectFolders.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SetupProjectFolders
{
    [MenuItem("Tools/Setup Project Folders")]
    public static void CreateFolders()
    {
        string[] rootFolders = new[]
        {
            "Assets/Core",
            "Assets/Core/Scripts",
            "Assets/Core/Data",
            "Assets/Core/Prefabs",
            
            "Assets/Player",
            "Assets/Player/Scripts",
            "Assets/Player/Prefabs",
            "Assets/Player/Sprites",
            "Assets/Player/Animations",
            "Assets/Player/Data",
            
            "Assets/Enemy",
            "Assets/Enemy/Scripts",
            "Assets/Enemy/Prefabs",
            "Assets/Enemy/Sprites",
            "Assets/Enemy/Animations",
            "Assets/Enemy/Data",
            
            "Assets/Weapons",
            "Assets/Weapons/Scripts",
            "Assets/Weapons/Prefabs",
            "Assets/Weapons/Sprites",
            "Assets/Weapons/Data",
            
            "Assets/UI",
            "Assets/UI/Scripts",
            "Assets/UI/Prefabs",
            "Assets/UI/Sprites",
            "Assets/UI/Fonts",
            
            "Assets/Environment",
            "Assets/Environment/Scripts",
            "Assets/Environment/Prefabs",
            "Assets/Environment/Sprites",
            "Assets/Environment/Materials",
            
            "Assets/Items",
            "Assets/Items/Scripts",
            "Assets/Items/Prefabs",
            "Assets/Items/Sprites",
            "Assets/Items/Data",
            
            "Assets/Stages",
            "Assets/Stages/Scripts",
            "Assets/Stages/Scenes",
            "Assets/Stages/Data",
            
            "Assets/Systems",
            "Assets/Systems/Scripts",
            "Assets/Systems/Data"
        };

        foreach (var folder in rootFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = System.IO.Path.GetDirectoryName(folder);
                string newFolder = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, newFolder);
                Debug.Log($"Created folder: {folder}");
            }
            else
            {
                Debug.Log($"Folder already exists: {folder}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Project folder setup complete.");
    }
}
#endif
