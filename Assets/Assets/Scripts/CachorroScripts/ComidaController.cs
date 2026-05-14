using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ComidaController : SpawnableObjects
{    
    private Camera _cam;
    private Rigidbody2D _rb;

    private bool _isDragging = false;
    private Vector2 _lastPosition;
    private Vector2 _speed;

    public float velocityMultiplier = 1f;

    [HideInInspector] public bool IsEaten = false;
    private bool _isTimeOut = false;

    private void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();

        SetSpawnPosition();
    }

    private float _timeLeft = 5f;

    void Update()
    {
        if (_timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;
        }
        else
        {
            _timeLeft = 0;
            _isTimeOut = true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInput(touch.position, touch.phase);
        }

        if (Input.GetMouseButtonDown(0))
            HandleInput(Input.mousePosition, TouchPhase.Began);

        if (Input.GetMouseButton(0))
            HandleInput(Input.mousePosition, TouchPhase.Moved);

        if (Input.GetMouseButtonUp(0))
            HandleInput(Input.mousePosition, TouchPhase.Ended);


        CheckWin();
    }

    private void CheckWin()
    {
        if (!IsEaten && _isTimeOut)
        {
            Debug.Log("Você ganhou!");
            // Aqui você pode adicionar lógica para o que acon
        }
    }

    private void HandleInput(Vector2 screenPos, TouchPhase phase)
    {
        Vector2 worldPos = _cam.ScreenToWorldPoint(screenPos);

        if (phase == TouchPhase.Began)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.transform == transform)
            {
                _isDragging = true;
                _rb.velocity = Vector2.zero;
                _lastPosition = worldPos;
            }
        }

        if (phase == TouchPhase.Moved && _isDragging)
        {
            Vector2 currentPosition = worldPos;

            _speed = (currentPosition - _lastPosition) / Time.deltaTime;

            _rb.MovePosition(currentPosition);

            _lastPosition = currentPosition;
        }

        if (phase == TouchPhase.Ended && _isDragging)
        {
            _isDragging = false;

            _rb.velocity = _speed * velocityMultiplier;
        }
    }
}
