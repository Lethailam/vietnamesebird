using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlyBehavior : MonoBehaviour
{
    [SerializeField] private float flySpeed = 1.5f;

    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePosition - _rb.position).normalized;
            _rb.velocity = direction * flySpeed;
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
    }
}
