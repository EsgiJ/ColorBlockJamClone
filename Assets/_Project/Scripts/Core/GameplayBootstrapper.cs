using System.Collections.Generic;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Block;
using ColorBlockJamClone.Gameplay.Gate;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

namespace ColorBlockJamClone.Core
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelDataSO _levelToLoad;

        [Header("Shared Assets")]
        [SerializeField] private ColorPaletteSO _palette;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Transform _gridOrigin;

        [Header("Prefabs")]
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private Gate _gatePrefab;
        [SerializeField] private GameObject _floorCellPrefab;
        [SerializeField] private GameObject _blockedCellPrefab;

        [Header("Parent References in Scene")]
        [SerializeField] private Transform _floorParent;
        [SerializeField] private Transform _blocksParent;
        [SerializeField] private Transform _gatesParent;

        // Runtime state
        private GridSystem _grid;
        private readonly List<Block> _blocks = new();
        private readonly List<Gate> _gates = new();

        public GridSystem Grid => _grid;
        public IReadOnlyList<Block> Blocks => _blocks;
        public IReadOnlyList<Gate> Gates => _gates;

        private void Start()
        {
            if (_levelToLoad == null)
            {
                Debug.LogError("[GameplayBootstrapper] No level assigned.");
                return;
            }
            BuildLevel(_levelToLoad);
        }

        private void BuildLevel(LevelDataSO data)
        {
            _grid = new GridSystem(
                data.gridSize.x,
                data.gridSize.y,
                _cellSize,
                _gridOrigin.position,
                data.blockedCells
            );

            BuildFloorVisual(data);
            SpawnBlocks(data);
            SpawnGates(data);

            Debug.Log($"[GameplayBootstrapper] Built level '{data.name}' " +
                      $"({data.gridSize.x}x{data.gridSize.y}), " +
                      $"{_blocks.Count} blocks, {_gates.Count} gates.");
        }

        private void BuildFloorVisual(LevelDataSO data)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var cell = _grid.GetCell(new Vector2Int(x, y));
                    var prefab = cell.IsBlocked ? _blockedCellPrefab : _floorCellPrefab;
                    if (prefab == null) continue;

                    var go = Instantiate(prefab, _floorParent);
                    var target = _grid.GridToWorldCentered(new Vector2Int(x, y));
                    target.y = go.transform.position.y; 
                    go.transform.position = target;
                    go.name = $"Cell_{x}_{y}{(cell.IsBlocked ? "_Blocked" : "")}";
                }
            }
        }

        private void SpawnBlocks(LevelDataSO data)
        {
            if (data.blocks == null) 
                return;

            foreach (var bp in data.blocks)
            {
                if (bp.shape == null) 
                    continue;

                var block = Instantiate(_blockPrefab, _blocksParent);
                block.Initialize(bp.shape, bp.color, bp.gridPosition, bp.rotationSteps, _palette, _cellSize);
                var target = _grid.GridToWorldCentered(bp.gridPosition);
                target.y = 1.5f; 
                block.transform.position = target;
                _blocks.Add(block);
            }
        }

        private void SpawnGates(LevelDataSO data)
        {
            if (data.gates == null) 
                return;
            foreach (var gp in data.gates)
            {
                var gate = Instantiate(_gatePrefab, _gatesParent);
                gate.Initialize(gp.color, gp.side, gp.positionAlongSide, gp.width, _palette, _grid);
                _gates.Add(gate);
            }
        }
    }
}