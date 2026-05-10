using UnityEngine;

public class PipaMovement : MonoBehaviour
{

    public float velocity = 300f;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PipaMovement script has started.");
        rb.gravityScale = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicou");
            rb.velocity = Vector2.up * velocity * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
