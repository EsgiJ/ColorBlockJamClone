using System;
using UnityEngine;

namespace ColorBlockJamClone.Data
{
    [CreateAssetMenu(fileName = "BlockShape_", menuName = "ColorBlockJamClone/Block Shape")]
    public class BlockShapeSO : ScriptableObject
    {
        [SerializeField] private string _shapeName;
        [SerializeField] private Vector2Int[] _cellOffsets = { Vector2Int.zero };

        [SerializeField] private GameObject _visualPrefab;

        public string ShapeName => _shapeName;
        public Vector2Int[] CellOffsets => _cellOffsets;
        public GameObject VisualPrefab => _visualPrefab;

        public Vector2Int[] GetRotatedOffsets(int rotationSteps)
        {
            rotationSteps = (((rotationSteps % 4) + 4) % 4);

            var result = new Vector2Int[_cellOffsets.Length];
            for (int i = 0; i < _cellOffsets.Length; i++)
                result[i] = RotateOffset(_cellOffsets[i], rotationSteps);
            return result;
        } 

        /// <summary>
        /// This is actualle 2D Rotation formula 
        ///     x' = x.cos(a) - y.sin(a)
        ///     y' = x.sin(a) + y.cos(a)
        /// </summary>
        private static Vector2Int RotateOffset(Vector2Int p, int steps)
        {
                return steps switch
                {
                    1 => new Vector2Int(p.y, -p.x),     // 90 Clockwise
                    2 => new Vector2Int(-p.x, -p.y),    // 180 Clockwise
                    3 => new Vector2Int(-p.y, p.x),     // 270 Clockwise
                    _ => p
                };
        }
    } 
}
