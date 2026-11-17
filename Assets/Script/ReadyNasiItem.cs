using UnityEngine;
using System.Collections;

public class ReadyNasiItem : MonoBehaviour {
    public string isian; 
    private Vector3 offset;
    private Vector3 startPosition;
    private Vector3 originalScale;

    void Start() {
        startPosition = transform.position;
        originalScale = transform.localScale;
        
        // EFEK POP-IN: Lahir dari ukuran 0 membal ke ukuran asli
        StartCoroutine(PopIn());
    }

    IEnumerator PopIn() {
        float timer = 0;
        transform.localScale = Vector3.zero;
        while(timer < 0.2f) {
            timer += Time.deltaTime;
            // Animasi membal melebihi target sedikit (1.2x) sebelum normal
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale * 1.2f, timer / 0.2f);
            yield return null;
        }
        transform.localScale = originalScale;
    }

    void OnMouseDown() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePos;
        offset.z = 0;
        GetComponent<SpriteRenderer>().sortingOrder = 15;
        
        // EFEK LIFT
        transform.localScale = originalScale * 1.15f;
    }

    void OnMouseDrag() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, startPosition.z);
    }

    void OnMouseUp() {
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        transform.localScale = originalScale; // Kembalikan ukuran

        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

        bool berhasilKasih = false;
        foreach (Collider2D hit in hits) {
            Customer pelanggan = hit.GetComponent<Customer>();
            if (pelanggan != null) {
                pelanggan.TerimaPesanan(isian); 
                berhasilKasih = true;
                Destroy(gameObject); 
                break;
            }
        }
        if (!berhasilKasih) transform.position = startPosition; 
    }
}