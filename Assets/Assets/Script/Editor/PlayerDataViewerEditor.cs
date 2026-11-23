#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataViewer))]
public class PlayerDataViewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerDataViewer viewer = (PlayerDataViewer)target;

        // Draw default inspector (Profile snapshot, Owned Cards list, Active Deck, History, etc.)
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Quick Debug Buttons", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Live Data", GUILayout.Height(28)))
        {
            viewer.RefreshViewer();
        }

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Give Debug Card", GUILayout.Height(28)))
        {
            viewer.GiveDebugCard();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+100 Coins", GUILayout.Height(24)))
        {
            viewer.AddDebugCoins();
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Reset Cards", GUILayout.Height(24)))
        {
            if (EditorUtility.DisplayDialog("Reset Cards", "Are you sure you want to reset all cards to default starter cards?", "Yes", "Cancel"))
            {
                viewer.ResetCards();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }
}
#endif
