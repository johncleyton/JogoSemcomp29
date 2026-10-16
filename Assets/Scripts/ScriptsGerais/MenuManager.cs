using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Animator animator;
    public void Jogar()
    {
        StartCoroutine(CarregarJogo());
    }

    IEnumerator CarregarJogo()
    {
        animator.SetTrigger("Jogar");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }
    
    public void Sair()
    {
        StartCoroutine(SairJogo());
    }
    
    IEnumerator SairJogo()
    {
        animator.SetTrigger("Fechar");
        yield return new WaitForSeconds(3.667f);
        Application.Quit();
    }
}
