using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopoMovement : MonoBehaviour
{
    private void OnMouseDrag()
    {
        Vector3 posicaoMouse = Input.mousePosition;

        Vector3 WposicaoMouse = Camera.main.ScreenToWorldPoint(posicaoMouse);
        transform.position = new Vector3(WposicaoMouse.x, transform.position.y, transform.position.z);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
