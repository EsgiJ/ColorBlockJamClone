using System.Threading.Tasks;
using ColorBlockJamClone.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;

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

            if(_level == null)
            {
                EditorGUILayout.HelpBox("Select an existing LevelDataSO or create a new one.", MessageType.Info);
            }

            DrawSettings();
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

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            var size = EditorGUILayout.Vector2IntField("Grid Size(W x H)", _level.gridSize);
            size.x = Mathf.Clamp(size.x, 3, 12);
            size.x = Mathf.Clamp(size.x, 3, 14);
            _level.gridSize = size;

            var timeLimit = EditorGUILayout.FloatField("Time Limit(sec)", _level.timeLimit);
            _level.timeLimit = Mathf.Max(5f, timeLimit);

            if(EditorGUI.EndChangeCheck())
                MarkDirty();
        }

        // To ensure save the level if we make any change on it
        private void MarkDirty()
        {
            if(_level == null)
                return;

            EditorUtility.SetDirty(_level);   
        }
    } 
}