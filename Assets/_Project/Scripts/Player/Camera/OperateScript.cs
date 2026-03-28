using UnityEngine;
using UnityEngine.InputSystem;

public class OperateScript : MonoBehaviour{
    public float distance = 3.0f;
    void Update() {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        
        if (Keyboard.current.eKey.wasPressedThisFrame && Physics.Raycast(ray, out hit, distance))
        {
            NextLevel button = hit.collider.GetComponent<NextLevel>();
            if (button != null) {
                button.Press();
            }
        } 
    }
}
