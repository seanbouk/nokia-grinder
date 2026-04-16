using UnityEngine;
using UnityEditor;

public class RandomRotateEditor : EditorWindow
{
    private GameObject parentObject;

    [MenuItem("Tools/Random Rotate Leaves (60° Steps)")]
    public static void ShowWindow()
    {
        GetWindow<RandomRotateEditor>("Random Rotate Leaves");
    }

    private void OnGUI()
    {
        GUILayout.Label("Random Y Rotation (60° Steps) for Leaf Nodes", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        if (GUILayout.Button("Apply Rotation"))
        {
            if (parentObject != null)
            {
                ApplyRandomRotation();
            }
            else
            {
                Debug.LogWarning("Please assign a parent object.");
            }
        }
    }

    private void ApplyRandomRotation()
    {
        Transform[] childTransforms = parentObject.GetComponentsInChildren<Transform>();

        int count = 0;
        foreach (Transform child in childTransforms)
        {
            if (child.childCount == 0 && child != parentObject.transform) // Leaf node check
            {
                int[] possibleRotations = { 0, 60, 120, 180, 240, 300 };
                int randomIndex = Random.Range(0, possibleRotations.Length);
                float randomY = possibleRotations[randomIndex];

                Undo.RecordObject(child, "Random 60° Y Rotation"); // Enables undo
                child.localRotation = Quaternion.Euler(child.localRotation.eulerAngles.x, randomY, child.localRotation.eulerAngles.z);
                count++;
            }
        }

        Debug.Log($"Rotated {count} leaf nodes to random multiples of 60°.");
    }
}
