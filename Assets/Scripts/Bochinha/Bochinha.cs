using UnityEngine;

public class BochinhaLauncher : MonoBehaviour
{
    public static BochinhaLauncher Instance;

    [Header("Configurações de Força")]
    public Transform launchPoint;
    public float maxForce = 15f; // Valores 2D costumam precisar de ajustes diferentes
    public float forceMultiplier = 3f;

    private GameObject prefabAtualParaLancar;
    private bool podeLancar = false;
    private Vector2 dragStartPos;
    private bool isDragging = false;
    private LineRenderer aimLine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        aimLine = GetComponent<LineRenderer>();
        if(aimLine) aimLine.positionCount = 0;
    }

    public void SetupTurn(GameObject prefabParaLancar, bool isBolim)
    {
        prefabAtualParaLancar = prefabParaLancar;
        podeLancar = true;
    }

    void Update()
    {
        if (!podeLancar) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = GetMouseWorldPosition();
            if(aimLine) aimLine.positionCount = 2;
        }

        if (isDragging)
        {
            Vector2 currentMousePos = GetMouseWorldPosition();
            Vector2 dragVector = dragStartPos - currentMousePos;
            
            if (aimLine)
            {
                aimLine.SetPosition(0, launchPoint.position);
                Vector2 targetDir = dragVector.normalized;
                float forceMagnitude = Mathf.Min(dragVector.magnitude * 0.5f, maxForce);
                
                // Desenha a linha de acordo com a força
                aimLine.SetPosition(1, (Vector2)launchPoint.position + (targetDir * forceMagnitude));
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if(aimLine) aimLine.positionCount = 0;

            Vector2 dragVector = dragStartPos - GetMouseWorldPosition();
            Vector2 launchDirection = dragVector.normalized;
            float launchForce = Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);

            Launch(launchDirection, launchForce);
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(worldPos.x, worldPos.y);
    }

    private void Launch(Vector2 direction, float force)
    {
        podeLancar = false;

        GameObject novaBola = Instantiate(prefabAtualParaLancar, launchPoint.position, Quaternion.identity);
        Rigidbody2D rb2d = novaBola.GetComponent<Rigidbody2D>();
        
        if (rb2d != null)
        {
            // Aplica impulso no ambiente 2D
            rb2d.AddForce(direction * force, ForceMode2D.Impulse);
        }

        BochinhaGameManager.Instance.BolaLancada(novaBola);
    }
}