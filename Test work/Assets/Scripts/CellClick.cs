using UnityEngine;
using UnityEngine.EventSystems;



public class CellClick : MonoBehaviour, IPointerClickHandler
{
    public enum State // Обозначение состояний игровой клетки
    {
        empty = 0,
        block = -2,
        target = -1,
    }

    private Transform _transform;
    private GameObject target;
    private GameObject block;

    public State state = State.empty; // Состояние клетки
    public bool canBeBlock = true; // Возможность установить в клетке препятствие

    public GameObject targetPref; // Отображение блока
    public GameObject blockPref; // Отображение отмеченной клетки

    void Awake()
    {
        _transform = gameObject.GetComponent<Transform>();
    }

    public void OnPointerClick(PointerEventData eventData) // Отслеживание кликов мыши
    {
        bool boardState = GameObject.Find("Board").GetComponent<BoardController>().speshialBehavior;
        if (eventData.pointerId == -1) // Клик левой клавиши
        {
            if (boardState)
                return;
            if (!canBeBlock)
                return;
            if (state == State.block)
            {
                state = State.empty;
                Destroy(block);
                Destroy(target);
            }
            else
            {
                state = State.block;
                Destroy(target);
                block = Instantiate(blockPref, new Vector3(_transform.position.x, blockPref.GetComponent<Transform>().position.y, _transform.position.z), Quaternion.identity);
            }
        }
        else if (eventData.pointerId == -2) // Клик правой клавиши
        {
            if (boardState)
                return;
            if (state == State.target)
            {
                state = State.empty;
                Destroy(block);
                Destroy(target);
            }
            else
            {
                state = State.target;
                Destroy(block);
                target = Instantiate(targetPref, new Vector3(_transform.position.x, targetPref.GetComponent<Transform>().position.y, _transform.position.z), Quaternion.identity);
            }
        }
    }
}
