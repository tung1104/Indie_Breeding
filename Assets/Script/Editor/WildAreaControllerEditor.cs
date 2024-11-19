using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WildAreaController))]
public class WildAreaControllerEditor : Editor
{
    public WildAreaController wildAreaController;

    private void OnEnable()
    {
        wildAreaController = (WildAreaController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Setup", GUILayout.Height(30)))
        {
            float positionY = 0;
            int indexScale = 0;
            Color color = new Color(1, 1, 1, 1);
            foreach (var wildArea in wildAreaController.WildAreas)
            {
                wildArea.transform.position = new Vector3(0, positionY, 0);
                positionY -= 0.01f;

                wildArea.transform.localScale = new Vector3(wildArea.radius * 2, wildArea.radius * 2, wildArea.radius * 2);
                indexScale++;

                wildArea.colorPropertyBlock.ChangeColor(color);
                color -= new Color(0.03f, 0.03f, 0.03f, 0.03f);

            }
        }
    }
}
