#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

public class FBXMeshExtractor {
    private static string _progressTitle = "Extracting Meshes";
    private static string[] _sourceExtensions = { ".fbx", ".obj" };
    private static string _targetExtension = ".asset";


    [MenuItem("Assets/Extract Meshes and Animations", validate = true)]
    private static bool ExtractMeshesMenuItemValidate() {
        for (int i = 0; i < Selection.objects.Length; i++) {
            if (!EndsWithAnyValidExt(AssetDatabase.GetAssetPath(Selection.objects[i])))
                return false;
        }
        return true;
    }

    private static bool EndsWithAnyValidExt(string path) {
        foreach (string ext in _sourceExtensions)
            if (path.EndsWith(ext))
                return true;
        return false;
    }

    [MenuItem("Assets/Extract Meshes and Animations")]
    private static void ExtractMeshesMenuItem() {
        EditorUtility.DisplayProgressBar(_progressTitle, "", 0);
        for (int i = 0; i < Selection.objects.Length; i++) {
            EditorUtility.DisplayProgressBar(_progressTitle, Selection.objects[i].name, (float)i / (Selection.objects.Length - 1));
            ExtractMeshes(Selection.objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void ExtractMeshes(Object selectedObject) {
        //Create Folder Hierarchy
        string selectedObjectPath = AssetDatabase.GetAssetPath(selectedObject);
        string parentfolderPath = selectedObjectPath.Substring(0, selectedObjectPath.Length - (selectedObject.name.Length + 5));
        string objectFolderName = selectedObject.name;
        string objectFolderPath = parentfolderPath + "/" + objectFolderName;

        //Create Meshes
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(selectedObjectPath);

        for (int i = 0; i < objects.Length; i++) {
            if (objects[i] is Mesh) {
                EditorUtility.DisplayProgressBar(_progressTitle, selectedObject.name + " : " + objects[i].name, (float)i / (objects.Length - 1));

                Mesh mesh = Object.Instantiate(objects[i]) as Mesh;

                AssetDatabase.CreateAsset(mesh, parentfolderPath + "/" + objects[i].name + _targetExtension);
            } else if (objects[i] is AnimationClip) {
                EditorUtility.DisplayProgressBar(_progressTitle, selectedObject.name + " : " + objects[i].name, (float)i / (objects.Length - 1));

                AnimationClip anim = Object.Instantiate(objects[i]) as AnimationClip;

                AssetDatabase.CreateAsset(anim, parentfolderPath + "/" + objects[i].name.Substring(objects[i].name.IndexOf('|') + 1) + _targetExtension);
            }
        }

        //Cleanup
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif