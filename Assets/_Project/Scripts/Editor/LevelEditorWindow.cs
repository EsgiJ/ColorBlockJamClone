using ColorBlockJamClone.Data;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJamClone.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelDataSO _level;

        [MenuItem(itemName: "Tools/Color Block Jam Clone/Level Editor")]
        public static void Init()
        {
            EditorWindow window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(800, 600);
        }

        private void OnGUI()
        {
            DrawHeader();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Color Block Jam Clone Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var newLevel = (LevelDataSO)EditorGUILayout.ObjectField("Level Data", _level, typeof(LevelDataSO), false);
            if(EditorGUI.EndChangeCheck())
                _level = newLevel;

            if(GUILayout.Button("New", GUILayout.Width(60)))
                CreateNewLevel();

            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateNewLevel()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Level", "Level_New", "asset", "Save new level file");
            if(string.IsNullOrEmpty(path))
                return;

            var newLevel = CreateInstance<LevelDataSO>();
            AssetDatabase.CreateAsset(newLevel, path);
            AssetDatabase.SaveAssets();
            _level = newLevel;
        }
    } 
}