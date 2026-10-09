using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LevelDifficulty
{
    VeryEasy,
    Easy,
    Medium,
    Hard,
    VeryHard
}

public class DifficultyController : MonoBehaviour
{
    // Quantas fases do GameManagerRework são necessárias para subir um nível de dificuldade
    [SerializeField] private int _fasesPorNivel = 3;

    [SerializeField] private int _maxNumberOfCachorros = 5;
    [SerializeField] private int _minNumberOfCachorros = 1;
    [SerializeField] private float _minCachorroSpeed = 1.5f;
    [SerializeField] private float _maxCachorroSpeed = 9.5f;
    [SerializeField] private float _speedVariation = 0.5f;
    [SerializeField] private AnimationCurve _speedCurve;
    private int _numberOfCachorros = -1;

    public static DifficultyController Instance;

    void Awake()
    {
        if (Instance != null)
            GameObject.Destroy(this);
        Instance = this;
    }

    // Chamado pelo CachorroGameManager (via ConfigurarDificuldade) com a fase atual do GameManagerRework
    public void SpawnarCachorros(int faseAtual)
    {
        SetCachorroParameters(GetDifficulty(faseAtual));
    }

    public int GetNumberOfCachorros()
    {
        return _numberOfCachorros;
    }

    private LevelDifficulty GetDifficulty(int faseAtual)
    {
        int nivel = (faseAtual - 1) / Mathf.Max(_fasesPorNivel, 1);
        int maiorNivel = Enum.GetValues(typeof(LevelDifficulty)).Length - 1;
        return (LevelDifficulty)Mathf.Clamp(nivel, 0, maiorNivel);
    }

    private void SetCachorroParameters(LevelDifficulty difficulty)
    {
        int numberOfCachorros = Mathf.RoundToInt(Mathf.Lerp(_minNumberOfCachorros, _maxNumberOfCachorros, 
            (float)difficulty / Enum.GetValues(typeof(LevelDifficulty)).Length));
        int cachorroSpeed = Mathf.RoundToInt(Mathf.Lerp(_minCachorroSpeed, _maxCachorroSpeed, 
            _speedCurve.Evaluate((float)difficulty / Enum.GetValues(typeof(LevelDifficulty)).Length)));
        Debug.Log(numberOfCachorros);
        for (int i = 0; i < numberOfCachorros; i++)
        {
            CachorroSpawner.Instance.SpawnCachorro(cachorroSpeed + UnityEngine.Random.Range(-_speedVariation, _speedVariation));
        }
    }
}
