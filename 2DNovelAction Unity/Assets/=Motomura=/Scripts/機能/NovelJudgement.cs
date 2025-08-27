using UnityEngine;
using UnityEngine.InputSystem;


public class NovelJudgement : MonoBehaviour
{
    [SerializeField] private InputActionAsset _PlayerInput;

    void Start()
    {
        _PlayerInput.Enable();
    }

    public void EnableNovelInput()//有効
    {
        Debug.Log("Novel Input Enabled");
        _PlayerInput.Enable();
    }

    public void DisableNovelInput()//無効
    {
        Debug.Log("Novel Input Disabled");
        _PlayerInput.Disable();
    }

}
