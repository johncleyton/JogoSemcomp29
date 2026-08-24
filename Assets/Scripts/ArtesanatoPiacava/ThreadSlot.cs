using UnityEngine;

public enum OrientacaoFio
{
    Horizontal,
    Vertical
}

[System.Serializable]
public class ThreadSlot
{
    public Vector2Int pontoA;
    public Vector2Int pontoB;
    public OrientacaoFio orientacao;
    public GameObject fioVisual;
    public bool faltando;
    public bool preenchido;

    public bool ConectaPontos(Vector2Int p1, Vector2Int p2)
    {
        return (pontoA == p1 && pontoB == p2) || (pontoA == p2 && pontoB == p1);
    }
}
