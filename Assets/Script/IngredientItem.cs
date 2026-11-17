using UnityEngine;

public class IngredientItem : MonoBehaviour {
    public string ingredientType; 
    public BowlManager targetBowl;
    public Sprite dragSprite; 

    private GameObject dragObject;

    void OnMouseDown() {
        dragObject = new GameObject("Dragging_" + ingredientType);
        SpriteRenderer sr = dragObject.AddComponent<SpriteRenderer>();
        
        if (dragSprite != null) {
            sr.sprite = dragSprite;
            // Jika pakai gambar khusus (dragSprite), ukurannya diset manual agar besar dan jelas
            dragObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
        else {
            sr.sprite = GetComponent<SpriteRenderer>().sprite;
            sr.color = GetComponent<SpriteRenderer>().color;
            // EFEK LIFT: Membesar 1.1x dari ukuran asli kontainernya saat dipegang, BUKAN mengecil
            dragObject.transform.localScale = transform.localScale * 1.1f; 
        }
        
        // Efek miring acak tetap ada agar terasa dinamis
        dragObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        
        sr.sortingOrder = 20; 
        UpdateDragPosition();
    }

    void OnMouseDrag() {
        if (dragObject != null) UpdateDragPosition();
    }

    void OnMouseUp() {
        if (dragObject != null) {
            Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

            foreach (Collider2D hit in hits) {
                if (targetBowl != null && hit.gameObject == targetBowl.gameObject) {
                    targetBowl.ProcessIngredient(ingredientType);
                    break;
                }
            }
            Destroy(dragObject);
        }
    }

    void UpdateDragPosition() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 
        dragObject.transform.position = mousePos;
    }
}