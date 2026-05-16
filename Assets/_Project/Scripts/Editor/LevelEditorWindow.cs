using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Wall;
using log4net.Core;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEditor.Rendering.LookDev;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorBlockJamClone.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelDataSO _level;

        private Vector2 _scrollPos;

        private const float CELL = 36f;
        private const float EDGE = 24f;

        private static readonly Color LEVEL_BACKGROUND_COLOR = new Color(0.18f, 0.18f, 0.2f);
        private static readonly Color CELL_DEFAULT_BACKGROUND_COLOR = new Color(0.32f, 0.32f, 0.36f);
        private static readonly Color CELL_BLOCKED_BACKGROUND_COLOR = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color WALL_BACKGROUND_COLOR = new Color(0.45f, 0.45f, 0.5f);

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
            EditorGUILayout.Space(8);

            if(_level == null)
            {
                EditorGUILayout.HelpBox("Select an existing LevelDataSO or create a new one.", MessageType.Info);
                return;
            }

            DrawSettings();
            EditorGUILayout.Space(8);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawGrid();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(8);
            
            DrawStats();
            DrawActions();
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

        private void DrawGrid()
        {
            var size = _level.gridSize;
            float totalWidth = size.x * CELL + EDGE * 2 + 20;
            float totalHeight = size.y * CELL + EDGE * 2 + 20;

            Rect rect = GUILayoutUtility.GetRect(totalWidth, totalHeight);
            rect = new Rect(rect.x + 10, rect.y + 10, totalWidth - 20, totalHeight - 20);

            EditorGUI.DrawRect(rect, LEVEL_BACKGROUND_COLOR);

            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int row = size.y - 1 - y;
                    var cellR = new Rect(rect.x + EDGE + x * CELL, rect.y + EDGE + row * CELL, CELL, CELL);
                    DrawCell(cellR, new Vector2Int(x, y));
                }
            }

            for (int i = 0; i < size.x; i++) 
                DrawEdgeSlot(rect, GridSide.Top, i);
            for (int i = 0; i < size.x; i++) 
                DrawEdgeSlot(rect, GridSide.Bottom, i);
            for (int i = 0; i < size.y; i++) 
                DrawEdgeSlot(rect, GridSide.Left, i);
            for (int i = 0; i < size.y; i++) 
                DrawEdgeSlot(rect, GridSide.Right, i);
        }

        private void DrawCell(Rect rect, Vector2Int pos)
        {
            Color backgroundColor = CELL_DEFAULT_BACKGROUND_COLOR;
            string label = "";

            bool isBlocked = _level.blockedCells != null && System.Array.IndexOf(_level.blockedCells, pos) >= 0;
            if(isBlocked)
            {
                backgroundColor = CELL_BLOCKED_BACKGROUND_COLOR;
                label = "X";
            }

            var block = FindBlockAt(pos);
            if(block.HasValue)
            {
                backgroundColor = ColorFor(block.Value.color);
                label = block.Value.color.ToString().Substring(0, 1);
            }

            EditorGUI.DrawRect(rect, backgroundColor);
            DrawBorder(rect, Color.black);
            
            GUI.Label(rect, label, new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.MiddleCenter });
        }

        private void DrawEdgeSlot(Rect total, GridSide side, int posAlongSide)
        {
            Rect rect = EdgeRect(total, side, posAlongSide);
            Color background = new Color(0.25f, 0.25f, 0.3f);

            var gate = FindGateAt(side, posAlongSide);
            if (gate.HasValue) 
                background = ColorFor(gate.Value.color);

            var wall = FindWallAt(side, posAlongSide);
            if (wall.HasValue) 
                background = WALL_BACKGROUND_COLOR;

            EditorGUI.DrawRect(rect, background);
            DrawBorder(rect, new Color(0, 0, 0, 0.6f));
        }

        private Rect EdgeRect(Rect total, GridSide side, int pos)
        {
            int width = _level.gridSize.x, height = _level.gridSize.y;
            return side switch
            {
                GridSide.Top    => new Rect(total.x + EDGE + pos * CELL, total.y, CELL, EDGE),
                GridSide.Bottom => new Rect(total.x + EDGE + pos * CELL, total.y + EDGE + height * CELL, CELL, EDGE),
                GridSide.Left   => new Rect(total.x, total.y + EDGE + (height - 1 - pos) * CELL, EDGE, CELL),
                GridSide.Right  => new Rect(total.x + EDGE + width * CELL, total.y + EDGE + (height - 1 - pos) * CELL, EDGE, CELL),
                _ => default
            };
        }

        private static void DrawBorder(Rect r, Color c)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), c);
            EditorGUI.DrawRect(new Rect(r.x + r.width - 1, r.y, 1, r.height), c);
            EditorGUI.DrawRect(new Rect(r.x, r.y + r.height - 1, r.width, 1), c);
        }

        private void DrawStats()
        {
            int blockCount = _level.blocks?.Length ?? 0;
            int gateCount = _level.gates?.Length ?? 0;
            int wallCount = _level.walls?.Length ?? 0;
            int blockedCellCount = _level.blockedCells?.Length ?? 0;

            EditorGUILayout.LabelField($"Blocks: {blockCount} | Gates: {gateCount} | Walls: {wallCount} | Blocked Cells: {blockedCellCount}",EditorStyles.miniLabel);
        }
        
        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Save", GUILayout.Height(30)))
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[LevelEditorWindow] Saved {_level.name}");
            }
            if(GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                if(EditorUtility.DisplayDialog("Clear All", "Are you sure you want to delete all blocks/walls/gates/blocked cells?", "Yes", "Cancel"))
                {
                    Undo.RecordObject(_level, "Clear All");
                    _level.blocks = new BlockPlacement[0];
                    _level.gates = new GatePlacement[0];
                    _level.walls = new WallPlacement[0];
                    _level.blockedCells= new Vector2Int[0];
                    MarkDirty();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // To ensure save the level if we make any change on it
        private void MarkDirty()
        {
            if(_level == null)
                return;

            EditorUtility.SetDirty(_level);   
        }

        private BlockPlacement? FindBlockAt(Vector2Int pos)
        {
            if (_level.blocks == null) 
                return null;

            foreach (var block in _level.blocks)
            {
                if (block.shape == null) 
                    continue;
                foreach (var offset in block.shape.GetRotatedOffsets(block.rotationSteps))
                {
                    if (block.gridPosition + offset == pos) 
                        return block;
                }
            }

            return null;
        }

        private GatePlacement? FindGateAt(GridSide side, int pos)
        {
            if (_level.gates == null) 
                return null;

            foreach (var gate in _level.gates)
            {
                if (gate.side == side && pos >= gate.positionAlongSide && pos < gate.positionAlongSide + gate.width)
                    return gate;
            }

            return null;
        }

        private WallPlacement? FindWallAt(GridSide side, int pos)
        {
            if (_level.walls == null) 
                return null;

            foreach (var wall in _level.walls)
            {
                if (wall.side == side && pos >= wall.positionAlongSide && pos < wall.positionAlongSide + wall.width)
                    return wall;
            }

            return null;
        }

        private static Color ColorFor(BlockColor c) => c switch
        {
            BlockColor.Red    => Color.red,
            BlockColor.Blue   => Color.blue,
            BlockColor.Green  => Color.green,
            BlockColor.Yellow => Color.yellow,
            BlockColor.Purple => new Color(0.65f, 0.30f, 0.85f),
            BlockColor.Orange => new Color(1.00f, 0.55f, 0.10f),
            _ => Color.gray
        };
    } 
}