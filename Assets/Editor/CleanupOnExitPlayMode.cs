/*
using UnityEngine;
using UnityEditor;
using Unity.Netcode;

[InitializeOnLoad]
public static class CleanupOnExitPlayMode
{
    static CleanupOnExitPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting Play Mode: Cleaning up network components...");
            RemoveComponentsOfType<NetworkObject>();
            RemoveComponentsOfType<NetworkTransformClient>();
        }
    }

    private static void RemoveComponentsOfType<T>() where T : Component
    {
        // Remove components from scene objects
        T[] sceneComponents = GameObject.FindObjectsOfType<T>();
        foreach (T component in sceneComponents)
        {
            GameObject.DestroyImmediate(component);
        }

        // Remove components from prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            T[] prefabComponents = prefab.GetComponentsInChildren<T>(true);
            foreach (T component in prefabComponents)
            {
                GameObject.DestroyImmediate(component, true);
                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
    }
}
*/
