#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataViewer))]
public class PlayerDataViewerEditor : Editor
{
    private bool showCardListFold = true;

    public override void OnInspectorGUI()
    {
        PlayerDataViewer viewer = (PlayerDataViewer)target;

        // Custom Header Banner
        EditorGUILayout.Space(6);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("PLAYER DATA & CARD INVENTORY VIEWER", headerStyle);
        EditorGUILayout.Space(4);

        // Stats Box
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Player Profile Overview", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"• Coins: {PlayerProfile.coins}   |   Level: {PlayerProfile.level}   |   HP: {PlayerProfile.MaxHp}   |   XP: {PlayerProfile.xp}/{PlayerProfile.XpToNext}");
        EditorGUILayout.LabelField($"• Total Cards: {viewer.totalOwnedCards}   |   Upgraded Cards: {viewer.totalUpgradedCards}   |   Max Level: {viewer.maxLevelCards}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // Card Level Overview Panel
        showCardListFold = EditorGUILayout.Foldout(showCardListFold, "Owned Cards Level Overview", true, EditorStyles.foldoutHeader);
        if (showCardListFold)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (viewer.ownedCards == null || viewer.ownedCards.Count == 0)
            {
                EditorGUILayout.LabelField("No cards owned yet. Click 'Reset Player Cards to Starter' or 'Give Debug Card' below.");
            }
            else
            {
                foreach (var item in viewer.ownedCards)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Level label with color coding
                    GUIStyle levelStyle = new GUIStyle(EditorStyles.label);
                    if (item.isMaxLevel)
                    {
                        levelStyle.normal.textColor = new Color(0.9f, 0.75f, 0.1f); // Gold
                        EditorGUILayout.LabelField($"[Level {item.level} MAX]", levelStyle, GUILayout.Width(110));
                    }
                    else if (item.isUpgraded)
                    {
                        levelStyle.normal.textColor = new Color(0.2f, 0.85f, 0.3f); // Green
                        EditorGUILayout.LabelField($"[Level {item.level} UPGRADED]", levelStyle, GUILayout.Width(130));
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"[Level {item.level}]", GUILayout.Width(70));
                    }

                    // Card Name & ID
                    EditorGUILayout.LabelField($"{item.cardName} ({item.cardId})", EditorStyles.boldLabel, GUILayout.Width(180));

                    // Max Level & Class
                    EditorGUILayout.LabelField($"Max: {item.maxLevel} | Cost: {item.cost}", GUILayout.Width(110));

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(8);

        // Draw default inspector for full details (Active deck, audit log, etc.)
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Quick Debug & Testing Controls", EditorStyles.boldLabel);

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

        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button("Upgrade Debug Card", GUILayout.Height(28)))
        {
            viewer.UpgradeDebugCard();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+100 Coins", GUILayout.Height(26)))
        {
            viewer.AddDebugCoins();
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Reset Cards to Starter", GUILayout.Height(26)))
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
