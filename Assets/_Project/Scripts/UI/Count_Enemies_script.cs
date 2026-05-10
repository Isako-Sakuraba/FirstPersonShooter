using UnityEngine;
using UnityEngine.UI;

public class Count_Enemies_script : MonoBehaviour {
    public int count_enemies = 0;
    [HideInInspector] public bool isfree = false;
    public Text text;

    void Update() {
        Collider triggerCollider = GetComponent<Collider>();
        
        Collider[] hits = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            triggerCollider.transform.rotation
        );
        
        count_enemies = 0;
        foreach (Collider hit in hits) {
            if (hit.CompareTag("Enemy")) {
                count_enemies++;
            }
        }
        text.text = $"Enemies left: {count_enemies}";
        if (count_enemies <= 0) {
            isfree = true;
        }
    }
}