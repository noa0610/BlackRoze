using UnityEngine;
using UnityEngine.InputSystem;

public class NovelEventInput : MonoBehaviour
{
[SerializeField]
private InputActionAsset _inputActions;
[SerializeField]
private GameObject _Player;
private Rigidbody2D _rb2d;
    void Start()
    {
        try
        {
            _rb2d = _Player.GetComponent<Rigidbody2D>();
        }
        catch
        {
            Debug.LogError("Rigidbody2D がないよ");
        }
    }

    public void PlayerEnable()
    {
        _inputActions.Enable();//有効化
        _rb2d.constraints -= RigidbodyConstraints2D.FreezePositionX;
    }

    public void PlayerDisable()
    {
        _inputActions.Disable();//無効化
        _rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        _rb2d.constraints -= RigidbodyConstraints2D.FreezePositionY;
    }
}
