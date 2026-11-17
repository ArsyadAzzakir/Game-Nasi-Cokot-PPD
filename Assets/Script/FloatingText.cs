using UnityEngine;
using System.Collections;

public class FloatingText : MonoBehaviour {
    // Bisa kamu ubah-ubah angkanya langsung dari Unity Inspector nanti!
    public float kecepatanY = 1.5f; 
    public float waktuHilang = 2.5f; // Total waktu hidup diperlama jadi 2.5 detik
    public float waktuTahan = 1.0f;  // Waktu teks terbaca jelas SEBELUM mulai memudar
    
    private SpriteRenderer spriteRenderer;
    private Color warna;
    private Vector3 ukuranAsli;

    public void Setup(Sprite gambarTeks, bool keAtas) {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        
        spriteRenderer.sprite = gambarTeks;
        warna = spriteRenderer.color;
        
        // Gerakan diperlambat sedikit biar rileks dibaca
        kecepatanY = keAtas ? 1.5f : -1.0f; 
        
        ukuranAsli = transform.localScale;
        transform.localScale = Vector3.zero;
        
        // Rotasinya dibuat sedikit lebih kalem
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));

        StartCoroutine(AnimasiJuicy());
        
        // Hancurkan objek otomatis setelah waktu habis
        Destroy(gameObject, waktuHilang);
    }

    IEnumerator AnimasiJuicy() {
        // --- FASE 1: POP OUT (0.15 detik) ---
        float timer = 0;
        float durasiPopOut = 0.15f;
        while (timer < durasiPopOut) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, ukuranAsli * 1.3f, timer / durasiPopOut);
            transform.position += new Vector3(0, kecepatanY * Time.deltaTime, 0); // Tetap bergerak sambil pop-out
            yield return null;
        }

        // --- FASE 2: MENGEMPIS (0.1 detik) ---
        timer = 0;
        float durasiKempis = 0.1f;
        while (timer < durasiKempis) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(ukuranAsli * 1.3f, ukuranAsli, timer / durasiKempis);
            transform.position += new Vector3(0, kecepatanY * Time.deltaTime, 0);
            yield return null;
        }
        transform.localScale = ukuranAsli;

        // --- FASE 3: HANG TIME (1 Detik) ---
        // Teks melayang pelan dengan warna 100% terang agar pemain sempat baca
        timer = 0;
        while (timer < waktuTahan) {
            timer += Time.deltaTime;
            transform.position += new Vector3(0, kecepatanY * Time.deltaTime, 0);
            yield return null; 
        }

        // --- FASE 4: MEMUDAR PERLAHAN ---
        timer = 0;
        float sisaWaktuFade = waktuHilang - durasiPopOut - durasiKempis - waktuTahan;
        while (timer < sisaWaktuFade) {
            timer += Time.deltaTime;
            transform.position += new Vector3(0, kecepatanY * Time.deltaTime, 0);
            
            // Efek Alpha transparan
            if (spriteRenderer != null) {
                warna.a = Mathf.Lerp(1f, 0f, timer / sisaWaktuFade);
                spriteRenderer.color = warna;
            }
            yield return null;
        }
    }
}