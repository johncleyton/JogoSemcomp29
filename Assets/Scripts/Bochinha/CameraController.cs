using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform defaultView;
    public float smoothSpeed = 0.125f;
    // O Z negativo garante que a câmera enxergue o plano 2D
    public Vector3 offset = new Vector3(0, 0, -10f); 

    void Update()
    {
        if (BochinhaGameManager.Instance != null && BochinhaGameManager.Instance.currentBolim != null)
        {
            GameObject[] bochas = GameObject.FindGameObjectsWithTag("Bocha");
            Transform target = null;

            foreach(var b in bochas)
            {
                if(b.GetComponent<Rigidbody2D>().velocity.magnitude > 0.5f)
                {
                    target = b.transform;
                    break;
                }
            }

            if(target == null && BochinhaGameManager.Instance.currentBolim.GetComponent<Rigidbody2D>().velocity.magnitude > 0.5f)
            {
                target = BochinhaGameManager.Instance.currentBolim.transform;
            }

            if (target != null)
            {
                Vector3 desiredPosition = target.position + offset;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
                return;
            }
        }

        if (defaultView != null)
        {
            Vector3 defaultPos = new Vector3(defaultView.position.x, defaultView.position.y, -10f);
            transform.position = Vector3.Lerp(transform.position, defaultPos, smoothSpeed);
        }
    }
}