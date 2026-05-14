using UnityEngine;
using ColorBlockJamClone.Data;

namespace ColorBlockJamClone.Gameplay.Block
{   
    public class Block : MonoBehaviour
    {
        [SerializeField] private Transform _cellsParent;

        public BlockShapeSO Shape { get; private set; }
        public BlockColor Color { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public int RotationSteps { get; private set; }

        public void Initialize(BlockShapeSO shape, BlockColor color, Vector2Int gridPosition, int rotationSteps, ColorPaletteSO palette, float cellSize)
        {
            Shape = shape;
            Color = color;
            GridPosition = gridPosition;
            RotationSteps = rotationSteps;

            name = $"Block_{color}_{shape.ShapeName}";
            BuildVisual(palette, cellSize);
        }

        private void BuildVisual(ColorPaletteSO palette, float cellSize)
        {
            var material = palette.GetMaterial(Color);

            if (Shape.VisualPrefab != null)
            {
                var visual = Instantiate(Shape.VisualPrefab, transform);

                var offsets = Shape.GetRotatedOffsets(RotationSteps);
                Vector2 centroid = Vector2.zero;

                foreach (var o in offsets) 
                    centroid += new Vector2(o.x, o.y);

                centroid /= offsets.Length;

                visual.transform.localPosition = new Vector3(
                    centroid.x * cellSize,
                    0f,
                    centroid.y * cellSize
                );

                visual.transform.localRotation = Quaternion.Euler(0f, -RotationSteps * 90f, 0f) * visual.transform.localRotation;

                if (material != null)
                {
                    foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Block] Shape '{Shape.ShapeName}' has no VisualPrefab. " + "Falling back to per-cell visuals.");
            }
        }

        /// <summary>
        /// Returns the grid cells this block currently occupies
        /// </summary>
        public Vector2Int[] GetOccupiedCells()
        {
            var offsets = Shape.GetRotatedOffsets(RotationSteps);
            var result = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
                result[i] = GridPosition + offsets[i];
            return result;
        }
    }
}