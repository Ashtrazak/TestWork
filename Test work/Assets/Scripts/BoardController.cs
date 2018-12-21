using UnityEngine;
using System.Collections.Generic;

public class BoardController : MonoBehaviour
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

    private const float cellSize = 1f; // Размер клетки игрового поля
    private GameObject[,] cell; // Контейнер для доступа к ячейкам
    private List<GameObject> unit = new List<GameObject>(); // Список юнитов
    public bool speshialBehavior = false; // Положене кнопки

    [Header("Клетка")]
    public GameObject cellPref;
    [Header("Размер игрового поля")]
    [Range(5, 10)]
    public int size = 5;
    public bool randomSize = false;
    [Header("Юнит")]
    public GameObject unitPref;
    [Header("Количество юнитов")]
    [Range(1, 5)]
    public int unitCounter = 1;
    public bool randomUnit = false;

    void Awake()
    {
        if (randomSize) // Игровое поле
            size = Random.Range(5, 11);
        cell = new GameObject[size, size];
        if (randomUnit) // Юниты
            unitCounter = Random.Range(1, 6);
    }
    private void Start()
    {
        CreateBoard(size);
        CreateUnit(unitCounter);
    }

    private void CreateBoard(int size) // Создание игрового поля
    {
        float startPositionX = transform.position.x - size * cellSize / 2f + cellSize / 2f;
        float startPositionZ = transform.position.z - size * cellSize / 2f - cellSize / 2f;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                cell[i, j] = Instantiate(cellPref, new Vector3(startPositionX + i * cellSize, transform.position.y, startPositionZ + j * cellSize), Quaternion.identity);
                cell[i, j].transform.SetParent(gameObject.transform);
            }
    }
    private void CreateUnit(int unitCounter) // Создание юнитов
    {
        for (int i = 0; i < unitCounter; i++)
        {
            int rndX = Random.Range(0, size);
            int rndY = Random.Range(0, size);
            Vector3 unitPosition = new Vector3(cell[rndX, rndY].transform.position.x, unitPref.transform.position.y, cell[rndX, rndY].transform.position.z);
            GameObject _unit;
            _unit = Instantiate(unitPref, unitPosition, Quaternion.identity);
            _unit.transform.SetParent(gameObject.transform);
            _unit.GetComponent<UnitMove>().unitPosition.x = rndX;
            _unit.GetComponent<UnitMove>().unitPosition.y = rndY;
            unit.Add(_unit);
        }
    }
    public int[,] GetMap() // Создание карты для ориентирования юнитов (Только свободные клетки и несвободные клетки)
    {
        int[,] map = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if ((int)cell[i, j].GetComponent<CellClick>().state == -2)
                    map[i, j] = -2;
                else
                    map[i, j] = 0;
        return map;
    }
    public int[,] GetMapWhithSpecialPositions() // Создание карты для ориентирования юнитов (Все состояния клеток)
    {
        int[,] map = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                map[i, j] = (int)cell[i, j].GetComponent<CellClick>().state;
        return map;
    }
    public Vector3 GetCellPosition(int x, int y) // Получить позицию клетки в игровых координатах
    {
        return cell[x, y].GetComponent<Transform>().position;
    }
    public void ChangeBlockableState(int x, int y, bool var) // Изменить возможность клетки разместить на себе блок
    {
        cell[x, y].GetComponent<CellClick>().canBeBlock = var;
    }
    public void ChangeBehavior() // Переключатель состояний при клике кнопки
    {
        speshialBehavior = !speshialBehavior;
        if (speshialBehavior)
            AddUnitsSpeshialTarget();
        else
            for (int i = 0; i < unit.Count; i++)
                if (unit[i].GetComponent<UnitMove>().speshialBehavior)
                    unit[i].GetComponent<UnitMove>().speshialBehavior = false;
    }
    public void AddUnitsSpeshialTarget()
    {
        if (!speshialBehavior) // Если кнопка не нажата
            return;
        List<coordinate> speshialPositionList = new List<coordinate>();
        int[,] map = GetMapWhithSpecialPositions();
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (map[i, j] == -1)
                    speshialPositionList.Add(new coordinate(i, j));
        if (speshialPositionList.Count == 0) // Если специальных целей нет
            return;
        // Исключим тех юнитов, что уже стоят на клетках или почти их заняли
        List<GameObject> deleteUnitList = new List<GameObject>();
        for (int i = 0; i < unit.Count; i++)
        {
            if (speshialPositionList.Count == 0)
                break;
            for (int j = 0; j < speshialPositionList.Count; j++)
                if (((unit[i].GetComponent<UnitMove>().unitPosition.x == speshialPositionList[j].x) && (unit[i].GetComponent<UnitMove>().unitPosition.y == speshialPositionList[j].y))
                || ((unit[i].GetComponent<UnitMove>().unitStep.x == speshialPositionList[j].x) && (unit[i].GetComponent<UnitMove>().unitStep.y == speshialPositionList[j].y)))
                {
                    unit[i].GetComponent<UnitMove>().speshialBehavior = true;
                    unit[i].GetComponent<UnitMove>().unitTarget.x = speshialPositionList[j].x;
                    unit[i].GetComponent<UnitMove>().unitTarget.y = speshialPositionList[j].y;

                    speshialPositionList.Remove(speshialPositionList[j]);
                    deleteUnitList.Add(unit[i]);
                    break;
                }
        }
        // Исключаем отмеченных юнитов
        List<GameObject> _unit = new List<GameObject>();
        for (int i = 0; i < unit.Count; i++)
            _unit.Add(unit[i]);
        if (deleteUnitList.Count > 0)
            for (int i = 0; i < deleteUnitList.Count; i++)
                _unit.Remove(deleteUnitList[i]);
        if (_unit.Count == 0)
            return;
        for (int i = 0; i < _unit.Count; i++)
        {
            if (speshialPositionList.Count == 0)
                break;
            for (int j = 0; j < speshialPositionList.Count; j++)
                if (WayCheck(_unit[i], speshialPositionList[j]))
                {
                    _unit[i].GetComponent<UnitMove>().speshialBehavior = true;
                    _unit[i].GetComponent<UnitMove>().unitTarget.x = speshialPositionList[j].x;
                    _unit[i].GetComponent<UnitMove>().unitTarget.y = speshialPositionList[j].y;

                    speshialPositionList.Remove(speshialPositionList[j]);
                    break;
                }
        }
    }
    bool WayCheck(GameObject unit, coordinate speshialPosition) // Проверка, возможно ли добраться до цели
    {
        int x = unit.GetComponent<UnitMove>().unitPosition.x;
        int y = unit.GetComponent<UnitMove>().unitPosition.y;
        int[,] map = GetMap(); // Карта положений всех клеток игрового поля
        map[speshialPosition.x, speshialPosition.y] = 1000; // Отмечаем на карте конечную клетку
        map[x, y] = 1; // Позиция юнита на карте
        List<coordinate> Wave = new List<coordinate>();
        List<coordinate> NewWave = new List<coordinate>();
        Wave.Add(new coordinate(x, y)); // Добавление первого элемента в волну
        bool isFind = false;
        while (Wave.Count > 0)
        {
            for (int i = 0; i < Wave.Count; i++)
            {
                if (Wave[i].x + 1 < size)
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
                if (Wave[i].y + 1 < size)
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
                return true;
            Wave.Clear();
            Wave = NewWave;
        }
        return false;
    }
}
