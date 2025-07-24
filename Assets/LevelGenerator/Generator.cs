using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Generator : MonoBehaviour
{
    // Singleton: cho phép truy cập GameManager từ bất kỳ đâu
    public static Generator instance;

    // Dữ liệu level: số dòng, cột, dữ liệu ống
    [SerializeField] private LevelData _level;

    // Prefab của ô Pipe để sinh ra trên grid
    [SerializeField] private SpawnCell _cellPrefab;
    [SerializeField] private int _row, _col;

    // Cờ kiểm tra xem game đã kết thúc chưa
    private bool hasGameFinished;

    // Mảng lưu toàn bộ các Pipe trong màn chơi
    private SpawnCell[,] pipes;

    // Danh sách các Pipe bắt đầu có nước (PipeType == 1)
    private List<SpawnCell> startPipes;

    private void Awake()
    {
        // Thiết lập singleton
        instance = this;
        hasGameFinished = false;
        CreateLevelData();
        // Tạo level khi game bắt đầu
        SpawnLevel();
    }
    private void CreateLevelData()
    {
        if (_level.Col == _col && _level.Row == _row) return;
        _level.Row = _row;
        _level.Col = _col;
        _level.Data = new List<int>();

        for (int i = 0; i < _row; i++)
        {
            for(int j =0; j < _col; j++)
            {
                _level.Data.Add(0);
            }
        }
    }

    private void SpawnLevel()
    {
        // Khởi tạo mảng ống theo số dòng, cột của level
        pipes = new SpawnCell[_level.Row, _level.Col];

        // Danh sách các ống khởi đầu (đầy nước sẵn)
        startPipes = new List<SpawnCell>();

        // Duyệt qua toàn bộ hàng (i) và cột (j)
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                // Tính toán vị trí spawn từng ống (chính giữa mỗi ô)
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f);

                // Tạo một pipe mới từ prefab
                SpawnCell tempPipe = Instantiate(_cellPrefab);

                // Đặt vị trí pipe vừa tạo vào đúng vị trí trên grid
                tempPipe.transform.position = spawnPos;

                // Gọi Init() để khởi tạo loại ống và trạng thái
                tempPipe.Init(_level.Data[i * _level.Col + j]);

                // Lưu vào mảng để sau dễ truy xuất theo hàng/cột
                pipes[i, j] = tempPipe;

                // Nếu là pipe khởi đầu (PipeType == 1), thêm vào danh sách
                if (tempPipe.PipeType == 1)
                {
                    startPipes.Add(tempPipe);
                }
            }
        }

        // Thiết lập kích cỡ camera để hiển thị toàn bộ level, có thu nhỏ 1.2x
        Camera.main.orthographicSize = Mathf.Max(_level.Row, _level.Col) * 1.2f;

        // Căn giữa camera theo chiều ngang và dọc của grid
        Vector3 cameraPos = Camera.main.transform.position;
        cameraPos.x = _level.Col * 0.5f;
        cameraPos.y = _level.Row * 0.5f;
        Camera.main.transform.position = cameraPos;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int row = Mathf.FloorToInt(mousePos.y);
        int col = Mathf.FloorToInt(mousePos.x);
        if (row < 0 || col < 0) return;
        if (row >= _level.Row) return;
        if (col >= _level.Col) return;

        if (Input.GetMouseButtonDown(0))
        {
            pipes[row, col].UpdateInput();
            StartCoroutine(ShowHint());
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            pipes[row, col].Init(0);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            pipes[row, col].Init(1);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            pipes[row, col].Init(2);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            pipes[row, col].Init(3);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            pipes[row, col].Init(4);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            pipes[row, col].Init(5);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            pipes[row, col].Init(6);
        }
        StartCoroutine(ShowHint());
    }
    private IEnumerator ShowHint()
    {
        yield return new WaitForSeconds(0.1f);
        ResetStartPipe();
        CheckFill();
        CheckWin();
        SaveData();
    }

    private void CheckFill()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                SpawnCell tempPipe = pipes[i, j];
                if (tempPipe.PipeType != 0)
                {
                    tempPipe.IsFilled = false;
                }
            }
        }
        Queue<SpawnCell> check = new Queue<SpawnCell>();
        HashSet<SpawnCell> finished = new HashSet<SpawnCell>();
        foreach (var pipe in startPipes)
        {
            check.Enqueue(pipe);
        }
        while (check.Count > 0)
        {
            SpawnCell pipe = check.Dequeue();
            finished.Add(pipe);
            List<SpawnCell> connected = pipe.ConnectedPipes();
            foreach (var connectedPipe in connected)
            {
                if (!finished.Contains(connectedPipe))
                {
                    check.Enqueue(connectedPipe);
                }
            }
        }

        foreach (var filled in finished)
        {
            filled.IsFilled = true;
        }
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                SpawnCell tempPipe = pipes[i, j];
                tempPipe.UpdateFilled();
            }
        }
    }
    private void ResetStartPipe()
    {
        startPipes = new List<SpawnCell>();
        for (int i = 0; i < _level.Row; i++)
        {
            for(int j=0; j < _level.Col; j++)
            {
                if (pipes[i,j].PipeType == 1)
                {
                    startPipes.Add(pipes[i,j]);
                }
            }
        }
    }
    private void SaveData()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                _level.Data[i * _level.Col + j] = pipes[i, j].PipeData;
            }
        }
        EditorUtility.SetDirty(_level);
    }

    private void CheckWin()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                if (!pipes[i, j].IsFilled)
                    return;
            }
        }
    }
}
