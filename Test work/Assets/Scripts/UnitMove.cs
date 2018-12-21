using UnityEngine;
using System.Collections.Generic;

public class UnitMove : MonoBehaviour
{
    public struct coordinate // Хранение координат
    {
        public int x;
        public int y;
        public coordinate(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }
    private Transform _transform;
    private const float cellSize = 1f; // Размер клетки игрового поля
    private GameObject boardLink; // Связь с игровым полем
    private int boardSize; // Размер игрового поля
    private Vector3 worldStepPosition;

    public float speed = 3f; // Скорость движения юнитов
    public coordinate unitPosition; // Позиция юнита на игровом поле
    public coordinate unitTarget; // Позиция конечной клетки в пути юнита 
    public coordinate unitStep; // Позиция следущей клетки на пути к конечной клетке
    public bool speshialBehavior = false; // Режим перемещения к отмеченой клетке

    void Awake ()
    {
        _transform = gameObject.GetComponent<Transform>();
        boardLink = GameObject.Find("Board");
        boardSize = boardLink.GetComponent<BoardController>().size;
    }
    void Start()
    {
        Move();
    }
    void Update ()
    {
        if (_transform.position != worldStepPosition)
        {
            _transform.position = Vector3.MoveTowards(_transform.position, worldStepPosition, speed*Time.deltaTime);
        }
        else
        {
            boardLink.GetComponent<BoardController>().ChangeBlockableState(unitPosition.x, unitPosition.y, true); // Освобождаем от ограничения клетку, где юнит стоял до этого
            unitPosition = unitStep; // Фиксируем перемещение юнита на игровой доске
            boardLink.GetComponent<BoardController>().ChangeBlockableState(unitPosition.x, unitPosition.y, false); // Накладываем ограничение на клетку, где теперь находится юнит

            if (!CheckTarget()) // Проверяем не появился ли блок на целевой клетке
                Move();
            if ((unitPosition.x == unitTarget.x) && (unitPosition.y == unitTarget.y)) // Если юнит достиг цели - выбираем новую
            {
                if (speshialBehavior) // Оставаться на месте по достижении специальной цели
                    return;
                Move();
            }
            else // Если юнит не достиг цели - делаем слудующий шаг к цели
            {
                unitStep = GetUnitStep();
                boardLink.GetComponent<BoardController>().ChangeBlockableState(unitStep.x, unitStep.y, false); // Запрещаем устанавливать блок в клетку, в которую сейчас переместиться юнит
                worldStepPosition = boardLink.GetComponent<BoardController>().GetCellPosition(unitStep.x, unitStep.y);
                worldStepPosition.y = _transform.position.y;
                if ((unitPosition.x == unitStep.x) && (unitPosition.y == unitStep.y)) // Проверяем возможно ли юниту добраться до цели
                    Start();
            }
        }
    }

    public void Move() // Стандартное поведение перемещения
    {
        boardLink.GetComponent<BoardController>().ChangeBlockableState(unitPosition.x, unitPosition.y, false); // Запрещаем устанавливать блок в клетку, где стоит юнит
        unitTarget = GenerateRandomTarget(); // Получить случайную доступную клетку
        unitStep = GetUnitStep(); // Получить следущий шаг к цели
        boardLink.GetComponent<BoardController>().ChangeBlockableState(unitStep.x, unitStep.y, false); // Запрещаем устанавливать блок в клетку, в которую сейчас переместиться юнит
        worldStepPosition = boardLink.GetComponent<BoardController>().GetCellPosition(unitStep.x, unitStep.y);
        worldStepPosition.y = _transform.position.y;
    }
    public coordinate GetUnitStep() // Получть следующий шаг к целевой клетке
    {
        int[,] map = boardLink.GetComponent<BoardController>().GetMap(); // Карта положений всех клеток игрового поля
        map[unitTarget.x, unitTarget.y] = 1000; // Отмечаем на карте конечную клетку
        return Waves(ref map, unitPosition.x, unitPosition.y); // Установка следущего шага к цели юнита, если путь к цели свободен
    }
    private coordinate Waves(ref int[,] map, int x, int y) // Волна с поиском цели и установкой следующего шага для юнита
    {
        if ((unitTarget.x == x) && (unitTarget.y == y))
            return new coordinate(x,y);
        map[x, y] = 1; // Позиция юнита на карте
        List<coordinate> Wave = new List<coordinate>();
        List<coordinate> NewWave = new List<coordinate>();
        Wave.Add(new coordinate(x, y)); // Добавление первого элемента в волну
        bool isFind = false;
        while (Wave.Count > 0)
        {
            for (int i = 0; i < Wave.Count; i++)
            {
                if (Wave[i].x + 1 < boardSize)
                    if (map[Wave[i].x + 1, Wave[i].y] == 0)
                    {
                        map[Wave[i].x + 1, Wave[i].y] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x + 1, Wave[i].y));
                    }
                    else if (map[Wave[i].x + 1, Wave[i].y] == 1000)
                    {
                        isFind = true;
                        break;
                    }
                
                if (Wave[i].x - 1 >= 0)
                    if (map[Wave[i].x - 1, Wave[i].y] == 0)
                    {
                        map[Wave[i].x - 1, Wave[i].y] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x - 1, Wave[i].y));
                    }
                    else if (map[Wave[i].x - 1, Wave[i].y] == 1000)
                    {
                        isFind = true;
                        break;
                    }
                if (Wave[i].y + 1 < boardSize)
                    if (map[Wave[i].x, Wave[i].y + 1] == 0)
                    {
                        map[Wave[i].x, Wave[i].y + 1] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x, Wave[i].y + 1));
                    }
                    else if (map[Wave[i].x, Wave[i].y + 1] == 1000)
                    {
                        isFind = true;
                        break;
                    }
                if (Wave[i].y - 1 >= 0)
                    if (map[Wave[i].x, Wave[i].y - 1] == 0)
                    {
                        map[Wave[i].x, Wave[i].y - 1] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x, Wave[i].y - 1));
                    }
                    else if (map[Wave[i].x, Wave[i].y - 1] == 1000)
                    {
                        isFind = true;
                        break;
                    }
            }
            if (isFind)
            {
                List<int> value = new List<int>();
                coordinate step = unitTarget;
                while (map[step.x, step.y] > 2)
                {
                    if (step.x + 1 < boardSize)
                        if (map[step.x + 1, step.y] > 0)
                            value.Add(map[step.x + 1, step.y]);

                    if (step.x - 1 >= 0)
                        if (map[step.x - 1, step.y] > 0)
                            value.Add(map[step.x - 1, step.y]);

                    if (step.y + 1 < boardSize)
                        if (map[step.x, step.y + 1] > 0)
                            value.Add(map[step.x, step.y + 1]);

                    if (step.y - 1 >= 0)
                        if (map[step.x, step.y - 1] > 0)
                            value.Add(map[step.x, step.y - 1]);

                    value.Sort();
                    if (value[0] == 1)
                        return unitTarget;

                    if (step.x + 1 < boardSize)
                        if (map[step.x + 1, step.y] == value[0])
                        {
                            value.Clear();
                            step = new coordinate(step.x + 1, step.y);
                            continue;
                        }

                    if (step.x - 1 >= 0)
                        if (map[step.x - 1, step.y] == value[0])
                        {
                            value.Clear();
                            step = new coordinate(step.x - 1, step.y);
                            continue;
                        }

                    if (step.y + 1 < boardSize)
                        if (map[step.x, step.y + 1] == value[0])
                        {
                            value.Clear();
                            step = new coordinate(step.x, step.y + 1);
                            continue;
                        }

                    if (step.y - 1 >= 0)
                        if (map[step.x, step.y - 1] == value[0])
                        {
                            value.Clear();
                            step = new coordinate(step.x, step.y - 1);
                            continue;
                        }
                }
                return step;
            }
            Wave.Clear();
            Wave = NewWave;
        }
        return unitPosition;
    }
    private coordinate GenerateRandomTarget() // Получить случайную доступную клетку
    {
        int[,] map = boardLink.GetComponent<BoardController>().GetMap(); // Карта положений всех клеток игрового поля
        SimpleWaves(ref map, unitPosition.x, unitPosition.y); // Метод отмечает все клетки, до которых может добраться юнит
        List<coordinate> positions = new List<coordinate>(); // Контейнер для хранения координат доступных клеток
        for (int i = 0; i < boardSize; i++)
            for (int j = 0; j < boardSize; j++)
                if (map[i, j] > 1)
                    positions.Add(new coordinate(i,j));
        // Выбор случайной клетки из доступных в качестве конечной
        if (positions.Count == 0)
            return unitPosition;
        else
            return positions[Random.Range(0, positions.Count)];
    }
    private void SimpleWaves(ref int[,] map, int x, int y) // Отметка доступных для перемещения клеток    
    {
        map[x, y] = 1; // Позиция юнита на карте
        List<coordinate> Wave = new List<coordinate>();
        List<coordinate> NewWave = new List<coordinate>();
        Wave.Add(new coordinate(x, y)); // Добавление первого элемента в волну
        while (Wave.Count > 0)
        {
            for (int i = 0; i < Wave.Count; i++)
            {
                if (Wave[i].x + 1 < boardSize)
                    if (map[Wave[i].x + 1, Wave[i].y] == 0)
                    {
                        map[Wave[i].x + 1, Wave[i].y] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x + 1, Wave[i].y));
                    }
                if (Wave[i].x - 1 >= 0)
                    if (map[Wave[i].x - 1, Wave[i].y] == 0)
                    {
                        map[Wave[i].x - 1, Wave[i].y] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x - 1, Wave[i].y));
                    }
                if (Wave[i].y + 1 < boardSize)
                    if (map[Wave[i].x, Wave[i].y + 1] == 0)
                    {
                        map[Wave[i].x, Wave[i].y + 1] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x, Wave[i].y + 1));
                    }
                if (Wave[i].y - 1 >= 0)
                    if (map[Wave[i].x, Wave[i].y - 1] == 0)
                    {
                        map[Wave[i].x, Wave[i].y - 1] = map[Wave[i].x, Wave[i].y] + 1;
                        NewWave.Add(new coordinate(Wave[i].x, Wave[i].y - 1));
                    }
            }
            Wave.Clear();
            Wave = NewWave;
        }
    }
    public bool CheckTarget() // Пороверка, не установлен ли блок в целевую клетку
    {
        int[,] map = boardLink.GetComponent<BoardController>().GetMap();
        if (map[unitTarget.x, unitTarget.y] == -2)
            return false;
        else
            return true;
    }
}


