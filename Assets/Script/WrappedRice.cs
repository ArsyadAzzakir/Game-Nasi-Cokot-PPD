using UnityEngine;
using System.Collections;

public class WrappedRice : MonoBehaviour {
    public string isian;
    public Sprite gambarDibungkus;
    
    private bool sudahDibungkus = false;
    private bool sedangDrag = false;
    private Vector3 posisiAwal;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Vector3 offsetMouse;

    void Start() {
        posisiAwal = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    void OnMouseDown() {
        if (!sudahDibungkus) {
            sudahDibungkus = true;
            sr.sprite = gambarDibungkus;
            
            // FIX KEGEDEAN: Ukuran dikembalikan ke normal (1x)
            transform.localScale = new Vector3(1f, 1f, 1f); 
            
            // FIX SUSAH DIGESER: Hancurkan collider lama, buat yang baru agar ukurannya pas dengan gambar bungkus!
            Destroy(col);
            col = gameObject.AddComponent<BoxCollider2D>();
            
            StartCoroutine(AnimasiMembal());
        } else {
            sedangDrag = true;
            sr.sortingOrder = 17; // Maju ke depan layar saat digeser
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            offsetMouse = transform.position - mousePos;
        }
    }

    void OnMouseDrag() {
        if (!sedangDrag) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos + offsetMouse;
    }

    void OnMouseUp() {
        if (!sedangDrag) return;
        sedangDrag = false;
        sr.sortingOrder = 6;

        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);
        bool diAtasMeja = false;

        foreach (Collider2D hit in hits) {
            if (hit.CompareTag("TrashCan") || hit.name.Contains("Trash")) {
                Destroy(gameObject);
                return;
            }

            PaperBagManager kantong = hit.GetComponent<PaperBagManager>();
            if (kantong != null) {
                if (kantong.MasukkanBarang(isian)) {
                    Destroy(gameObject);
                    return;
                }
            }

            Customer pelanggan = hit.GetComponent<Customer>();
            if (pelanggan != null) {
                // DEBUG LOG: Cek Console-mu saat pelanggan marah untuk melihat perbedaan namanya
                Debug.Log("Kamu memberikan: '" + isian + "' ke pelanggan.");
                
                pelanggan.TerimaPesanan(isian);
                Destroy(gameObject);
                return;
            }
            
            if (hit.CompareTag("Table") || hit.name.Contains("Meja")) {
                diAtasMeja = true;
            }
        }
        
        if (!diAtasMeja) transform.position = posisiAwal;
    }

    IEnumerator AnimasiMembal() {
        Vector3 targetScale = transform.localScale;
        float timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale * 0.8f, targetScale * 1.2f, timer / 0.1f);
            yield return null;
        }
        timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale * 1.2f, targetScale, timer / 0.1f);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}