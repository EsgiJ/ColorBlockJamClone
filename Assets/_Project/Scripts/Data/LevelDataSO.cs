using System;
using UnityEngine;

namespace ColorBlockJamClone.Data
{
    [Serializable]
    public struct BlockPlacement
    {
        public BlockShapeSO shape;
        public BlockColor color;
        public Vector2Int gridPosition;     // origin of the cell
        [Range(0, 3)] public int rotationSteps;    
    }

    [Serializable]
    public struct GatePlacement
    {
        public BlockColor color;
        public GridSide side;
        public int positionAlongSide;
        [Range(1, 4)] public int width;
    }

    [CreateAssetMenu(fileName = "Level_", menuName = "ColorBlockJamClone/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Grid")]
        public Vector2Int gridSize = new(7,8);
        public Vector2Int[] blockedCells;

        [Header("Time")]
        [Min(5f)] public float timeLimit = 120f;

        [Header("Blocks")]
        public BlockPlacement[] blocks;

        [Header("Gates")]
        public GatePlacement[] gates;
    }
}
