using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{

    public float speed = 500f;

    public RectTransform cursorRect;


    // joystick mobile gameobject
    public Joystick joystick;

    void Update()
    {
        Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);    

        cursorRect.anchoredPosition += direction * speed * Time.deltaTime;

        // limitar bordas da tela
        Vector2 fixedPos = cursorRect.anchoredPosition;

        fixedPos.x = Mathf.Clamp(fixedPos.x, -Screen.width/2, Screen.width/2); 
        fixedPos.y = Mathf.Clamp(fixedPos.y, -Screen.height/2, Screen.height/2); 

        cursorRect.anchoredPosition = fixedPos;

    }


    public void ClickedAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(cursorRect.position);

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {


            IngredientBehaviour ingredientScript = hit.collider.GetComponent<IngredientBehaviour>();

            // ingredientScript != null
            if (hit.collider.CompareTag("Ingredient"))
            {
                
                Debug.Log("pegou objecto!");

                Destroy(hit.collider.gameObject);
            }
        }
    }
}
