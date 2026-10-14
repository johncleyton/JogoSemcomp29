using UnityEngine;

public enum Regiao {
    Sul,
    Sudeste,
    CentroOeste,
    Nordeste,
    Norte,
    Brasil
}

public enum TipoJogo {
    Normal,
    Sobrevivencia
}

[CreateAssetMenu(fileName = "NovoMinigame", menuName = "Semcomp/Dados do Minigame")]
public class MinigameData : ScriptableObject
{
    public string nomeDoJogo;
    public string instrucao; 
    public string nomeDaCena;
    public TipoJogo tipoJogo;
    public Regiao regiao;
    public string NomeDaRegiaoFormatado
    {
        get
        {
            if (regiao == Regiao.CentroOeste)
                return "Centro-Oeste";
            return regiao.ToString();
        }
    }
}