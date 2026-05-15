using System.Collections.Generic;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Block;
using ColorBlockJamClone.Gameplay.Gate;
using ColorBlockJamClone.Gameplay.Grid;
using ColorBlockJamClone.Gameplay.Input;
using ColorBlockJamClone.Gameplay.Timer;
using ColorBlockJamClone.Gameplay.Wall;
using UnityEngine;

namespace ColorBlockJamClone.Core
{
    public class GameplayBootstrapper : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] private LevelDataSO[] _levels;

        [Header("Shared Assets")]
        [SerializeField] private ColorPaletteSO _palette;
        [SerializeField] private float _cellSize = 2f;
        [SerializeField] private Transform _gridOrigin;

        [Header("Prefabs")]
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private Gate _gatePrefab;
        [SerializeField] private Wall _wallPrefab;
        [SerializeField] private GameObject _floorCellPrefab;
        [SerializeField] private GameObject _blockedCellPrefab;

        [Header("Parent References in Scene")]
        [SerializeField] private Transform _floorParent;
        [SerializeField] private Transform _blocksParent;
        [SerializeField] private Transform _gatesParent;
        [SerializeField] private Transform _wallParent;

        [Header("Input")]
        [SerializeField] private DragInputHandler _dragInput;

        [Header("Feedback")]
        [SerializeField] private FeedbackConfigSO _feedback;

        // Runtime state
        private GridSystem _grid;
        private BlockMover _mover;
        private Timer _timer;
        private MonoPool<Block> _blockPool;
        
        private readonly List<Block> _activeBlocks = new();
        private readonly List<Gate> _activeGates = new();
        private readonly List<Wall> _activeWalls = new();
        private readonly List<GameObject> _activeFloor = new();


        public GridSystem Grid => _grid;
        public IReadOnlyList<Block> Blocks => _activeBlocks;
        public IReadOnlyList<Gate> Gates => _activeGates;
        public IReadOnlyList<Wall> Wall => _activeWalls;
        public int CurrentLevelIndex { get; private set; }
        public float CurrentLevelDuration => _levels[CurrentLevelIndex].timeLimit;

        private void Awake()
        {
            _blockPool = new MonoPool<Block>(_blockPrefab, _blocksParent, prewarm: 16);
            _timer = new Timer(0f);
            _timer.OnExpired += HandleTimerExpired;
        }

        private void Start()
        {
            LoadLevel(0);
        }

        private void Update()
        {
            _timer?.Tick(Time.deltaTime);
            if (_timer != null && _timer.IsRunning)
                GameEvents.RaiseTimerTick(_timer.Remaining);
        }

        public void LoadLevel(int index)
        {
            ClearLevel();
            CurrentLevelIndex = Mathf.Clamp(index, 0, _levels.Length - 1);
            var data = _levels[CurrentLevelIndex];
            BuildLevel(data);
            _dragInput.ResetForNewLevel();
        }

        public void RestartLevel() => LoadLevel(CurrentLevelIndex);
        public void NextLevel() => LoadLevel((CurrentLevelIndex + 1) % _levels.Length);

        private void BuildLevel(LevelDataSO data)
        {
            
            Vector3 centeredOrigin = _gridOrigin.position - new Vector3(
                data.gridSize.x * _cellSize * 0.5f,
                0f,
                data.gridSize.y * _cellSize * 0.5f
            );

            _grid = new GridSystem(data.gridSize.x, data.gridSize.y, _cellSize, centeredOrigin, data.blockedCells);

            BuildFloorVisual();
            SpawnBlocks(data);
            SpawnGates(data);
            SpawnWalls(data);

            _mover = new BlockMover(_grid);
            _dragInput.Initialize(_grid, _mover, _activeGates, OnBlockExited, OnFirstInput);

            _timer.Reset(data.timeLimit);

            GameManager.Instance?.SetState(GameState.Start);
            GameEvents.RaiseLevelLoaded(CurrentLevelIndex);

            Debug.Log($"[GameplayBootstrapper] Level '{data.name}' ready " + $"({data.gridSize.x}x{data.gridSize.y}, {_activeBlocks.Count} blocks).");
        }

        private void ClearLevel()
        {
            foreach (var b in _activeBlocks)
            {
                b.ResetForReuse();
                _blockPool.Release(b);
            }
            _activeBlocks.Clear();

            foreach (var g in _activeGates) 
                Destroy(g.gameObject);
            _activeGates.Clear();

            foreach (var f in _activeFloor) 
                Destroy(f);
            _activeFloor.Clear();

            foreach (var g in _activeWalls) 
                Destroy(g.gameObject);
            _activeWalls.Clear();

            _timer?.Pause();
        }

        private void BuildFloorVisual()
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var cell = _grid.GetCell(new Vector2Int(x, y));
                    var prefab = cell.IsBlocked ? _blockedCellPrefab : _floorCellPrefab;

                    if (prefab == null) 
                        continue;

                    var go = Instantiate(prefab, _floorParent);
                    var target = _grid.GridToWorldCentered(new Vector2Int(x, y));
                    target.y = go.transform.position.y; 
                    go.transform.position = target;
                    go.name = $"Cell_{x}_{y}{(cell.IsBlocked ? "_Blocked" : "")}";
                    _activeFloor.Add(go);
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

                var block = _blockPool.Get();
                block.transform.SetParent(_blocksParent);
                block.Initialize(bp.shape, bp.color, bp.gridPosition, bp.rotationSteps, _palette, _feedback,_cellSize);

                var target = _grid.GridToWorldCentered(bp.gridPosition);
                target.y = 1.5f; 
                block.transform.position = target;

                _grid.Occupy(block);
                _activeBlocks.Add(block);
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
                _activeGates.Add(gate);
            }
        }

        private void SpawnWalls(LevelDataSO data)
        {
            if (data.walls == null) 
                return;

            foreach (var w in data.walls)
            {
                var wall = Instantiate(_wallPrefab, _wallParent);
                wall.Initialize(w.side, w.positionAlongSide, w.width, _grid);
                _activeWalls.Add(wall);
            }
        }
        
        private void OnFirstInput()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Start)
            {
                GameManager.Instance.SetState(GameState.Gameplay);
                _timer.Start();
                GameEvents.RaiseLevelStarted();
            }
        }

        private void OnBlockExited(Block block)
        {
            if (!_activeBlocks.Contains(block)) 
                return;

            _activeBlocks.Remove(block);
            block.ResetForReuse();
            _blockPool.Release(block);

            GameEvents.RaiseBlockExited(_activeBlocks.Count);

            if (_activeBlocks.Count == 0 &&
                GameManager.Instance != null &&
                GameManager.Instance.State == GameState.Gameplay)
            {
                _timer.Pause();
                GameManager.Instance.SetState(GameState.Complete);
                GameEvents.RaiseLevelCompleted();
            }
        }

        private void HandleTimerExpired()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.State == GameState.Gameplay)
            {
                GameManager.Instance.SetState(GameState.GameOver);
                GameEvents.RaiseLevelFailed();
            }
        }
    }
}