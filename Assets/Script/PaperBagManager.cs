using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaperBagManager : MonoBehaviour {
    public List<string> isiKantong = new List<string>();
    public int kapasitasMaksimal = 2; 

    private Vector3 posisiAwal;
    private Vector3 skalaAsli;
    private SpriteRenderer sr;
    private bool sedangDrag = false;
    private Vector3 offsetMouse;
    private bool sudahSetup = false;

    // PERBAIKAN 1: Fungsi ini memaksa kantong mengingat posisinya di Editor 
    // meskipun dia sedang disembunyikan/dimatikan.
    void PastikanSetup() {
        if (!sudahSetup) {
            posisiAwal = transform.position;
            skalaAsli = transform.localScale;
            sr = GetComponent<SpriteRenderer>();
            sudahSetup = true;
        }
    }

    void Start() {
        PastikanSetup();
    }

    public void AktifkanKantongBaru() {
        PastikanSetup(); // Ambil koordinat aslinya sebelum pindah!
        isiKantong.Clear();
        transform.position = posisiAwal; 
        gameObject.SetActive(true);
        
        StopAllCoroutines();
        StartCoroutine(AnimasiMembal(1.25f));
    }

    public bool MasukkanBarang(string kodeBarang) {
        if (!gameObject.activeSelf || isiKantong.Count >= kapasitasMaksimal) return false;

        isiKantong.Add(kodeBarang);
        StopAllCoroutines();
        StartCoroutine(AnimasiMembal(1.4f)); 
        return true;
    }

    void OnMouseDown() {
        PastikanSetup();
        sedangDrag = true;
        sr.sortingOrder = 18; 

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        offsetMouse = transform.position - mousePos;
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

        foreach (Collider2D hit in hits) {
            // 1. Dibuang ke Tong Sampah
            if (hit.CompareTag("TrashCan") || hit.name.Contains("Trash")) {
                isiKantong.Clear();
                transform.position = posisiAwal;
                gameObject.SetActive(false);
                return;
            }

            // 2. Diserahkan ke Pelanggan
            Customer pelanggan = hit.GetComponent<Customer>();
            if (pelanggan != null) {
                if (isiKantong.Count > 0) {
                    pelanggan.TerimaKantongPesanan(isiKantong);
                    isiKantong.Clear();
                    transform.position = posisiAwal;
                    gameObject.SetActive(false); 
                } else {
                    transform.position = posisiAwal; // Kalau kosong, paksa balik ke posisi awal meja
                }
                return;
            }
        }

        // PERBAIKAN 2: Aturan kaku "Table" dihapus! 
        // Sekarang kalau kamu melepas kantong di tempat kosong mana pun, 
        // kantongnya akan langsung diam menetap di titik tersebut. 
        // Jauh lebih bebas dan natural saat dimainkan.
    }

    IEnumerator AnimasiMembal(float skala) {
        float timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli, skalaAsli * skala, timer / 0.1f);
            yield return null;
        }
        timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli * skala, skalaAsli, timer / 0.1f);
            yield return null;
        }
        transform.localScale = skalaAsli;
    }
}