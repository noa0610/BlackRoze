using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;

public class AnimTest : MonoBehaviour
{
    public float JumpForce = 5f;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Vector2 moveInput;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        
        moveInput = new Vector2(1, 0);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            moveInput = new Vector2(-1, 0);

        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveInput = new Vector2(1, 0);
        }
        else
        {
            moveInput = new Vector2(0, 0);
        }


        if (Input.GetKeyDown(KeyCode.Space) && _rigidbody2D.velocity.y <= 0)
        {
            _rigidbody2D.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            _animator.SetTrigger("toJump");
        }

        if (moveInput.x != 0)
        {
            _animator.SetTrigger("toWalk");
            transform.localScale = new Vector3(moveInput.x, transform.localScale.y, transform.localScale.z);
        }
        else if(moveInput.x == 0 && _rigidbody2D.velocity.y <= 0)
        {
            _animator.SetTrigger("toIdle");
        }
        

        
    }


}
