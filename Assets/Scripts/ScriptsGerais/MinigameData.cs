using UnityEngine;

[CreateAssetMenu(fileName = "NovoMinigame", menuName = "Semcomp/Dados do Minigame")]
public class MinigameData : ScriptableObject
{
    public string nomeDoJogo;
    public string instrucao; 
    public string nomeDaCena;
    // 0 - normal, 1 - sobrevivência
    public int tipoJogo; 
}