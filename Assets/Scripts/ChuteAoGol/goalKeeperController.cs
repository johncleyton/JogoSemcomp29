using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goalKeeperController : MonoBehaviour
{
    [SerializeField] float randomJumpRadius = 1f;

    private bool isMoving = false;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector2 genRandomVector()
    {
        return UnityEngine.Random.insideUnitCircle * randomJumpRadius;
    }

    public void moveGoalkeeper()
    {
        if(!isMoving)
        {
            rb.velocity = genRandomVector();
            isMoving = true;
        }
    }

    public void stopGoalKeeper()
    {
        rb.velocity = Vector2.zero;
        isMoving = false;
    }
}
