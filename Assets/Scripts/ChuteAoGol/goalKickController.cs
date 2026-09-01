using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class goalKickController : MinigameBase
{
    [Header("Tempo de Chute")]
    [SerializeField] float waitTime = 2f;

    [Header("Referencias a objetos")]
    [SerializeField] GameObject ball;
    [SerializeField] GameObject defense;
    [SerializeField] Collider2D goalCollider;

    private Collider2D ballCollider;
    private Collider2D defenseCollider;
    private BallController ballScript;
    private goalKeeperController defenseScript;

    private bool timeEnded = false;

    // Start is called before the first frame update
    void Start()
    {
        ballCollider = ball.GetComponent<Collider2D>();
        ballScript = ball.GetComponent<BallController>();

        defenseCollider = defense.GetComponent<Collider2D>();
        defenseScript = defense.GetComponent<goalKeeperController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ballScript.getIsThrown())
        {
            defenseScript.moveGoalkeeper();
            StartCoroutine("waiter");
        }

        if(timeEnded)
        {
            ballScript.stopBall();
            defenseScript.stopGoalKeeper();
            if(isGoal())
            {
                Vencer();
            } else
            {
                Perder();
            }
        }
    }

    public override void TempoEsgotado()
    {
        if(!ballScript.getIsThrown())
        {
            base.TempoEsgotado();
        }
    }

    public bool isGoal()
    {
        return (goalCollider.IsTouching(ballCollider) && !ballCollider.IsTouching(defenseCollider));
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(waitTime);
        timeEnded = true;
    }
}
