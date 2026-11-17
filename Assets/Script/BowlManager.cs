using UnityEngine;
using System.Collections;

[System.Serializable]
public class VisualVarianSuwir {
    public string kodeIsian; 
    public Sprite base2_Isian1; 
    public Sprite base3_Nasi2;  
    public Sprite base4_Isian2; 
    public Sprite base5_Penuh;  
    public Sprite nasiReady;    
}

public class BowlManager : MonoBehaviour {
    public enum BowlState { Kosong, Nasi1, Isian1, Nasi2, Isian2, Nasi3 }
    public BowlState stateSekarang = BowlState.Kosong;
    public string isianSaatIni = "";

    [Header("Gambar Umum Mangkuk & Bungkus")]
    public Sprite gambarMangkukKosong;
    public Sprite gambarBase1_Nasi1; 
    public Sprite gambarSudahDibungkus; // Gambar Nasi warp.png 

    [Header("Daftar Gambar Per Varian (3 Rasa)")]
    public VisualVarianSuwir[] daftarVarian;

    private SpriteRenderer sr;
    private Vector3 posisiAwalMeja;
    private Vector3 skalaAsli;
    private bool sedangDrag = false;
    private Vector3 offsetMouse;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        posisiAwalMeja = transform.position;
        skalaAsli = transform.localScale;
        ResetMangkuk();
    }

    public bool ProcessIngredient(string jenisBahan) {
        return TambahBahan(jenisBahan);
    }

    public bool TambahBahan(string jenisBahan) {
        bool adalahNasi = (jenisBahan == "Nasi" || jenisBahan == "Rice");
        bool adalahSuwir = (AmbilDataVarian(jenisBahan) != null);

        if (stateSekarang == BowlState.Kosong && adalahNasi) {
            stateSekarang = BowlState.Nasi1;
            sr.sprite = gambarBase1_Nasi1;
            EfekMembal();
            return true;
        }
        else if (stateSekarang == BowlState.Nasi1 && adalahSuwir) { 
            isianSaatIni = jenisBahan;
            stateSekarang = BowlState.Isian1;
            UpdateGambarVarian();
            EfekMembal();
            return true;
        }
        else if (stateSekarang == BowlState.Isian1 && adalahNasi) {
            stateSekarang = BowlState.Nasi2;
            UpdateGambarVarian();
            EfekMembal();
            return true;
        }
        else if (stateSekarang == BowlState.Nasi2 && jenisBahan == isianSaatIni) {
            stateSekarang = BowlState.Isian2;
            UpdateGambarVarian();
            EfekMembal();
            return true;
        }
        else if (stateSekarang == BowlState.Isian2 && adalahNasi) {
            stateSekarang = BowlState.Nasi3;
            UpdateGambarVarian();
            EfekMembal();
            return true;
        }
        return false;
    }

    void UpdateGambarVarian() {
        VisualVarianSuwir v = AmbilDataVarian(isianSaatIni);
        if (v == null) return;

        if (stateSekarang == BowlState.Isian1) sr.sprite = v.base2_Isian1;
        else if (stateSekarang == BowlState.Nasi2) sr.sprite = v.base3_Nasi2;
        else if (stateSekarang == BowlState.Isian2) sr.sprite = v.base4_Isian2;
        else if (stateSekarang == BowlState.Nasi3) sr.sprite = v.base5_Penuh;
    }

    VisualVarianSuwir AmbilDataVarian(string kode) {
        foreach (VisualVarianSuwir v in daftarVarian) {
            if (v.kodeIsian == kode) return v;
        }
        return null;
    }

    void OnMouseDown() {
        sedangDrag = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        offsetMouse = transform.position - mousePos;
        sr.sortingOrder = 15;
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
        sr.sortingOrder = 5;

        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

        bool aksiBerhasil = false;
        bool diAtasMeja = false;

        foreach (Collider2D hit in hits) {
            if (hit.gameObject == gameObject) continue;

            if (hit.CompareTag("TrashCan") || hit.name.Contains("Trash")) {
                ResetMangkuk();
                aksiBerhasil = true;
                break;
            }

            // PERBAIKAN: Mangkuk mencetak objek nasi baru!
            if (stateSekarang == BowlState.Nasi3 && (hit.CompareTag("PaperWrap") || hit.name.Contains("Paper"))) {
                
                // 1. Bikin objek nasi ajaib secara otomatis di atas kertas
                GameObject objNasi = new GameObject("NasiCetak_" + isianSaatIni);
                objNasi.transform.position = hit.transform.position; 
                
                // 2. Tempelkan gambarnya
                SpriteRenderer srNasi = objNasi.AddComponent<SpriteRenderer>();
                VisualVarianSuwir v = AmbilDataVarian(isianSaatIni);
                srNasi.sprite = v.nasiReady;
                srNasi.sortingOrder = 6;
                
                // 3. Tambahkan Collider otomatis
                objNasi.AddComponent<BoxCollider2D>();
                
                // 4. Pasang script penggerak nasi
                WrappedRice scriptWrap = objNasi.AddComponent<WrappedRice>();
                scriptWrap.isian = isianSaatIni;
                scriptWrap.gambarDibungkus = gambarSudahDibungkus;
                
                // 5. Mangkuknya langsung mereset dirinya ke posisi awal agar bisa masak lagi!
                ResetMangkuk(); 
                aksiBerhasil = true;
                break;
            }

            if (hit.CompareTag("Table") || hit.name.Contains("Meja")) {
                diAtasMeja = true;
            }
        }

        if (!aksiBerhasil && !diAtasMeja) {
            transform.position = posisiAwalMeja;
        }
    }

    public void ResetMangkuk() {
        stateSekarang = BowlState.Kosong;
        isianSaatIni = "";
        sr.sprite = gambarMangkukKosong;
        transform.position = posisiAwalMeja;
        transform.localScale = skalaAsli;
    }

    void EfekMembal() {
        StopAllCoroutines();
        StartCoroutine(AnimasiMembal());
    }

    IEnumerator AnimasiMembal() {
        float timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli, skalaAsli * 1.2f, timer / 0.1f);
            yield return null;
        }
        timer = 0f;
        while (timer < 0.1f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli * 1.2f, skalaAsli, timer / 0.1f);
            yield return null;
        }
        transform.localScale = skalaAsli;
    }
}