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
    [SerializeField] private LevelDifficulty _difficulty;

    [SerializeField] private int _maxNumberOfCachorros = 5;
    [SerializeField] private int _minNumberOfCachorros = 1;
    [SerializeField] private float _minCachorroSpeed = 1.5f;
    [SerializeField] private float _maxCachorroSpeed = 9.5f;
    [SerializeField] private float _speedVariation = 0.5f;
    [SerializeField] private AnimationCurve _speedCurve;

    void Awake()
    {
        // _difficulty = GetDifficulty(); COLOCAR AQUI O GETTER PARA PEGAR A DIFICULDADE DO JOGO
        SetCachorroParameters();
    }

    private LevelDifficulty GetDifficulty()
    {
        // COLOCAR AQUI O GETTER PARA PEGAR A DIFICULDADE DO JOGO
        // CONVERTER O VALOR PEGADO PARA O ENUM LevelDifficulty
        return LevelDifficulty.Medium;
    }

    private void SetCachorroParameters()
    {
        int numberOfCachorros = Mathf.RoundToInt(Mathf.Lerp(_minNumberOfCachorros, _maxNumberOfCachorros, 
            (float)_difficulty / Enum.GetValues(typeof(LevelDifficulty)).Length));
        int cachorroSpeed = Mathf.RoundToInt(Mathf.Lerp(_minCachorroSpeed, _maxCachorroSpeed, 
            _speedCurve.Evaluate((float)_difficulty / Enum.GetValues(typeof(LevelDifficulty)).Length)));
        Debug.Log(numberOfCachorros);
        for (int i = 0; i < numberOfCachorros; i++)
        {
            CachorroSpawner.Instance.SpawnCachorro(cachorroSpeed + UnityEngine.Random.Range(-_speedVariation, _speedVariation));
        }
    }
}
