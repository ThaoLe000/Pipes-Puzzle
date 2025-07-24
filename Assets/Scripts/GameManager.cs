using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton: cho phép truy cập GameManager từ bất kỳ đâu
    public static GameManager instance;

    // Dữ liệu level: số dòng, cột, dữ liệu ống
    [SerializeField] private List<LevelData> _levels;
    [SerializeField] private int time;

    // Prefab của ô Pipe để sinh ra trên grid
    [SerializeField] private Pipe _cellPrefab;
    private int currentLevelIndex = 0;
    private LevelData _level;
 
    // Cờ kiểm tra xem game đã kết thúc chưa
    private bool hasGameFinished;

    // Mảng lưu toàn bộ các Pipe trong màn chơi
    private Pipe[,] pipes;

    // Danh sách các Pipe bắt đầu có nước (PipeType == 1)
    private List<Pipe> startPipes;

    private void Awake()
    {
        // Thiết lập singleton
        instance = this;
        hasGameFinished = false;
        // Tạo level khi game bắt đầu
        _level = _levels[currentLevelIndex];
        SpawnLevel();
        StartCoroutine(TimeCount());
    }

    private void SpawnLevel()
    {
        // Khởi tạo mảng ống theo số dòng, cột của level
        pipes = new Pipe[_level.Row, _level.Col];

        // Danh sách các ống khởi đầu (đầy nước sẵn)
        startPipes = new List<Pipe>();

        // Duyệt qua toàn bộ hàng (i) và cột (j)
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                // Tính toán vị trí spawn từng ống (chính giữa mỗi ô)
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f);

                // Tạo một pipe mới từ prefab
                Pipe tempPipe = Instantiate(_cellPrefab);

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
    }
    private IEnumerator ShowHint()
    {
        yield return new WaitForSeconds(0.1f);
        CheckFill();
        CheckWin();
    }

    private void CheckFill()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                Pipe tempPipe = pipes[i, j];
                if (tempPipe.PipeType != 0)
                {
                    tempPipe.IsFilled = false;
                }
            }
        }
        Queue<Pipe> check = new Queue<Pipe>();
        HashSet<Pipe> finished = new HashSet<Pipe>();
        foreach (var pipe in startPipes)
        {
            check.Enqueue(pipe);
        }
        while (check.Count > 0)
        {
            Pipe pipe = check.Dequeue();
            finished.Add(pipe);
            List<Pipe> connected = pipe.ConnectedPipes();
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
                Pipe tempPipe = pipes[i, j];
                tempPipe.UpdateFilled();
            }
        }
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
        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }
    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(2f);
        UIManager.Instance.ShowCompletePanel();
        StopCoroutine(TimeCount());
        NextLevel();
    }
    private void NextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= _levels.Count)
        {
            Debug.Log("You finished all levels!");
            return;
        }
        hasGameFinished = false;
        time = 60;
        _level = _levels[currentLevelIndex];
        UIManager.Instance.UpdateLevelText(currentLevelIndex + 1);
        ClearOldLevel();
        SpawnLevel();

        StartCoroutine(TimeCount());
    }
    private void ClearOldLevel()
    {
        if (pipes != null)
        {
            for (int i = 0; i < pipes.GetLength(0); i++)
            {
                for (int j = 0; j < pipes.GetLength(1); j++)
                {
                    if (pipes[i, j] != null)
                    {
                        Destroy(pipes[i, j].gameObject);
                    }
                }
            }
        }
        if (startPipes != null)
        {
            startPipes.Clear();
        }
        pipes = null;
    }
    private IEnumerator TimeCount()
    {
        while (time > 0 && !hasGameFinished)
        {
            UIManager.Instance.UpdateTimeText(time);
            {
                yield return new WaitForSeconds(1f);
                time--;
                UIManager.Instance.UpdateTimeText(time);
            }
            if(time == 0)
            {
                UIManager.Instance.ShowGameOverPanel();
            }
        }

    }
}
