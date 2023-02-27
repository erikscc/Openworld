using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[CustomEditor(typeof(ESCapeWorldEditor))]
public class CreateCubeTool:Editor
{
    static CreateCubeTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the prefab
        ESCapeWorldEditor myPrefab = (ESCapeWorldEditor)target;

        // Show a preview of the prefab
        GUILayout.Label("Prefab Preview:");

        myPrefab.selectedToPaintWith = (GameObject)EditorGUILayout
                                        .ObjectField(myPrefab.selectedToPaintWith, 
                                                     typeof(GameObject), 
                                                     false, 
                                                     GUILayout.Width(100), 
                                                     GUILayout.Height(100)
                                                     );    
    }
    static void OnSceneGUI(SceneView sceneView)
    {
		if ((Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftShift))
		{
            Vector2 mousePos = Event.current.mousePosition;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
            Vector3 worldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ChangeSkins(cube,worldPos);
        }
    }
    private static void ChangeSkins(GameObject targetObject, Vector3 worldPos)
	{
        ESCapeWorldEditor mycubeToUse = FindObjectOfType<ESCapeWorldEditor>();
        targetObject.transform.position =
               new Vector3(
                               Mathf.Round(worldPos.x),
                               Mathf.Round(0),
                               Mathf.Round(worldPos.z)
                           );

        targetObject.GetComponent<MeshFilter>().mesh = mycubeToUse.selectedToPaintWith.GetComponent<MeshFilter>().sharedMesh;
        targetObject.GetComponent<MeshRenderer>().materials = mycubeToUse.selectedToPaintWith.GetComponent<MeshRenderer>().sharedMaterials;
    }
}
