using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Arremesso")]
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float saveDelay = 0.1f;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float pullForce = 10f;

    [Header("Perspectiva")]
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float shrinkSpeed = 1.5f;

    private bool isDragging = false;
    private bool isSaving = false;
    private bool isThrown = false;
    private bool isStoped = false;

    private Vector3 initialScale;
    private Vector2 lastPosition;
    private Rigidbody2D rb;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

                if (!isSaving)
                {
                    StartCoroutine(SavePosition());
                }

                transform.position = mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                ReleaseBall();
            }
        }

        // Diminui o tamnho da bola pra parecer que tá indo longe
        if (isThrown && !isDragging)
        {
            Vector3 targetScale = initialScale * minScale;
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, shrinkSpeed * Time.deltaTime);
        }
    }

    // Bola puxada para o centro do gol, acho que facilita um pouco o minigame
    private void FixedUpdate()
    {
        if (isThrown && !isDragging && !isStoped)
        {
            if (targetPoint != null)
            {
                Vector2 direction = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;
                rb.AddForce(direction * pullForce);
            }
            else
            {
                rb.AddForce((-1) * transform.position);
            }
        }
    }

    private IEnumerator SavePosition()
    {
        isSaving = true;
        lastPosition = (Vector2)transform.position;
        yield return new WaitForSeconds(saveDelay);
        isSaving = false;
    }

    private void OnMouseDown()
    {
        // Impede de pegar a bola de novo
        if (isThrown) return;

        isDragging = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
    }

    private void ReleaseBall()
    {
        isDragging = false;
        isThrown = true;
        rb.isKinematic = false;

        rb.velocity = ((Vector2)transform.position - lastPosition) * throwForce;
    }

    public void stopBall()
    {
        isStoped = true;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
    }

    public bool getIsThrown() { return isThrown; }
}