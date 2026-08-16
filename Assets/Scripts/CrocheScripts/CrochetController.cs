using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class CrochetController : MinigameBase
{
    public LineRenderer lineRenderer;
    private List<Vector2> pointsList = new List<Vector2>();
    private bool isDrawing = false;
    public GameObject no;
    public float dist;

    private Gesture[] trainingSet;

    public int nosAtuais = 0, nosMax = 3;

    private void Start()
    {
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        trainingSet = new Gesture[gesturesXml.Length];
        for (int i = 0; i < gesturesXml.Length; i++)
            trainingSet[i] = GestureIO.ReadGestureFromXML(gesturesXml[i].text);

        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        for (int i = 0; i < filePaths.Length; i++)
            trainingSet[i] = GestureIO.ReadGestureFromFile(filePaths[i]);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartDrawing();
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
                Debug.Log($"Forma reconhecida: {result.GestureClass} com pontua��o de similaridade: {result.Score}");
        }
        else
            Debug.LogWarning("Nenhum template de gesto carregado no trainingSet para comparar!");
    }

    void desenharHinge()
    {
        Debug.Log("Croche detectado, instanciando nova hinge");
        GameObject novoHinge = Instantiate(no, transform.position + (transform.up * dist), transform.rotation, transform);
        dist += novoHinge.GetComponent<SpriteRenderer>().size.y;
        nosAtuais++;
        if (nosAtuais >= nosMax)
            Vencer();
    }


}