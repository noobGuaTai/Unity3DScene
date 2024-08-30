using UnityEngine;
using UnityEditor;

public class BatchReplaceShader : EditorWindow
{
    private Shader newShader; // 新的Shader
    private Material[] materialsToReplace; // 要替换的材质

    [MenuItem("Tools/Batch Replace Shader")]
    public static void ShowWindow()
    {
        GetWindow<BatchReplaceShader>("Batch Replace Shader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Replace Shader", EditorStyles.boldLabel);

        // 显示Shader选择框
        newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);

        // 显示材质选择按钮
        if (GUILayout.Button("Select Materials"))
        {
            SelectMaterials();
        }

        // 如果有选中的材质，显示替换按钮
        if (materialsToReplace != null && materialsToReplace.Length > 0)
        {
            GUILayout.Label($"Selected {materialsToReplace.Length} materials", EditorStyles.helpBox);

            if (GUILayout.Button("Replace Shader"))
            {
                ReplaceShader();
            }
        }
        else
        {
            GUILayout.Label("No materials selected.", EditorStyles.helpBox);
        }
    }

    private void SelectMaterials()
    {
        materialsToReplace = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);
        if (materialsToReplace.Length == 0)
        {
            Debug.LogWarning("No materials selected!");
        }
        else
        {
            Debug.Log($"Selected {materialsToReplace.Length} materials.");
        }
    }

    private void ReplaceShader()
    {
        if (newShader == null)
        {
            Debug.LogError("Please assign a new shader!");
            return;
        }

        foreach (Material mat in materialsToReplace)
        {
            if (mat != null)
            {
                Undo.RecordObject(mat, "Replace Shader");
                mat.shader = newShader;
                EditorUtility.SetDirty(mat); // 标记材质已修改
            }
        }

        AssetDatabase.SaveAssets(); // 保存修改
        Debug.Log("Shader replacement completed.");
    }
}
