using UnityEngine;

public class CursorCustomPao : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        // CORRIGIDO: evita chamar Camera.main (busca por tag) a cada clique
        cam = Camera.main;
    }

    void Update()
    {
        // Detecta o clique do mouse no PC ou o toque na tela do celular
        if (Input.GetMouseButtonDown(0))
        {
            ClickedAction();
        }
    }

    private void ClickedAction()
    {
        if (cam == null) return;

        // Pega a posição exata de onde o dedo tocou na tela
        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Lança um raio mágico nesse exato ponto para ver se acertou algo
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // Verifica se o objeto tocado é um ingrediente
            if (hit.collider.CompareTag("Ingredientes"))
            {
                IngredientBehaviour ingredientScript = hit.collider.GetComponent<IngredientBehaviour>();

                // CORRIGIDO: o campo "gotCaught" existia mas nunca era usado.
                // Agora ele realmente impede que o mesmo ingrediente seja
                // processado mais de uma vez antes de ser destruído.
                if (ingredientScript != null && !ingredientScript.gotCaught)
                {
                    ingredientScript.gotCaught = true;

                    Debug.Log("Pegou o ingrediente no ar!");

                    // CORRIGIDO: protege contra erro caso CollectIngredients
                    // ainda não exista na cena (ordem de inicialização)
                    if (CollectIngredients.instance != null)
                    {
                        CollectIngredients.instance.AddIngredient(ingredientScript.ingredientData);
                    }

                    Destroy(hit.collider.gameObject);

                    // CORRIGIDO: a verificação da receita agora acontece
                    // automaticamente dentro de AddIngredient(), então não
                    // é mais preciso chamá-la de novo aqui.
                }
            }
        }
    }
}
