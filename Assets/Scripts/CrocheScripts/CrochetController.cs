using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using System.IO;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class CrochetController : MinigameBase
{
    public LineRenderer lineRenderer;
    private List<Vector2> pointsList = new List<Vector2>();
    private bool isDrawing = false;
    
    public GameObject no;
    
    [Header("Configurações de Posição e Espaçamento")]
    public Transform pontoInicial; 
    public float distanciaAposPrimeiroNo = 0.6f; // Distância do nó da mesa para o primeiro da corrente
    public float distanciaEntreNos = 0.4f;       // Distância entre os nós da corrente em si
    private float distAcumulada = 0f;

    [Header("Sprites do Nó Inicial")]
    public Sprite spriteInicialComLaco; 
    public Sprite spriteInicialSemLaco; 

    [Header("Sprites dos Nós Seguintes")]
    public Sprite spritePosteriorComLaco; 
    public Sprite spritePosteriorSemLaco; 
    
    private SpriteRenderer ultimoNoRenderer; 

    private Gesture[] trainingSet;

    public int nosAtuais = 0, nosMax = 3;
    public TMP_Text textoProgresso;
    public GameObject lacoTutorial;
    public Animator animator;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        nosMax = Mathf.Min(4 + faseAtual/10, 8);

        return 10f;
    }

    private void Start()
    {
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        trainingSet = new Gesture[gesturesXml.Length];
        for (int i = 0; i < gesturesXml.Length; i++)
            trainingSet[i] = GestureIO.ReadGestureFromXML(gesturesXml[i].text);

        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        for (int i = 0; i < filePaths.Length; i++)
            trainingSet[i] = GestureIO.ReadGestureFromFile(filePaths[i]);
        textoProgresso.text = $"{nosAtuais} / {nosMax}";
        lacoTutorial.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (lacoTutorial.activeInHierarchy)
                lacoTutorial.SetActive(false);
            StartDrawing();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
            ContinueDrawing();
        else if (Input.GetMouseButtonUp(0) && isDrawing)
            EndDrawing();
    }

    void StartDrawing()
    {
        isDrawing = true;
        pointsList.Clear();
        lineRenderer.positionCount = 0;
        AddPoint(Input.mousePosition);
    }

    void ContinueDrawing()
    {
        Vector2 currentMousePos = Input.mousePosition;
        if (pointsList.Count == 0 || Vector2.Distance(currentMousePos, pointsList[pointsList.Count - 1]) > 10f)
            AddPoint(currentMousePos);
    }

    void AddPoint(Vector2 mousePos)
    {
        pointsList.Add(mousePos);
        lineRenderer.positionCount = pointsList.Count;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        lineRenderer.SetPosition(pointsList.Count - 1, worldPos);
    }

    void EndDrawing()
    {
        isDrawing = false;
        EvaluateShape();
    }

    void EvaluateShape()
    {
        Point[] pointArray = new Point[pointsList.Count];
        for (int i = 0; i < pointsList.Count; i++)
            pointArray[i] = new Point(pointsList[i].x, -pointsList[i].y, 0);

        Gesture candidateGesture = new Gesture(pointArray, "UserDrawnShape");

        if (trainingSet != null && trainingSet.Length > 0)
        {
            Result result = PointCloudRecognizer.Classify(candidateGesture, trainingSet);
            if (result.GestureClass == "croche")
                desenharHinge();
            else
                Debug.Log($"Forma reconhecida: {result.GestureClass} com pontuação de similaridade: {result.Score}");
        }
        else
            Debug.LogWarning("Nenhum template de gesto carregado no trainingSet para comparar!");
    }

    void desenharHinge()
    {
        Transform origem = pontoInicial != null ? pontoInicial : transform;

        GameObject novoHinge = Instantiate(no, origem.position + (origem.up * distAcumulada), origem.rotation, transform);
        SpriteRenderer sr = novoHinge.GetComponent<SpriteRenderer>();

        if (nosAtuais == 0)
        {
            if (sr != null) sr.sprite = spriteInicialComLaco;
            // Define o salto inicial que geralmente é diferente
            distAcumulada += distanciaAposPrimeiroNo;
        }
        else
        {
            if (ultimoNoRenderer != null)
            {
                if (nosAtuais == 1) 
                    ultimoNoRenderer.sprite = spriteInicialSemLaco;
                else 
                    ultimoNoRenderer.sprite = spritePosteriorSemLaco;
            }

            if (sr != null) sr.sprite = spritePosteriorComLaco;
            
            // Define o salto padrão para o resto da corrente
            distAcumulada += distanciaEntreNos;
        }

        ultimoNoRenderer = sr;
        nosAtuais++;
        textoProgresso.text = $"{nosAtuais} / {nosMax}";
        if (nosAtuais >= nosMax)
        {
            animator.SetTrigger("vitoria");
            Vencer();
        }
    }
}