using UnityEngine;
using System.Collections;

public class RiceCookerManager : MonoBehaviour {
    [Header("Pengaturan Kapasitas Nasi")]
    public int kapasitasPorsi = 4;       // Habis setelah 4 porsi nasi cokot
    public int sendokPerPorsi = 3;       // 1 nasi cokot butuh 3 lapis nasi
    public float waktuMasakUlang = 2.5f; // Durasi masak ulang saat kosong diklik

    [Header("4 Tahap Gambar Rice Cooker")]
    public Sprite gambarFull;            // full.png (100%)
    public Sprite gambarDrain2;          // rice cooker drain 2.png
    public Sprite gambarDrain3;          // rice cooker drain 3.png
    public Sprite gambarDrainZero;       // rice cooker drain zero.png (Kosong)

    [Header("Visual Sendokan (Opsional)")]
    public Sprite gambarSendokNasi;      // Gambar nasi yang ikut kursor saat ditarik

    private int totalSendokanMaks;
    private int stokSendokan;
    private bool sedangMasak = false;
    private bool sedangDrag = false;

    private SpriteRenderer sr;
    private Vector3 skalaAsli;
    private Vector3 posisiAsli;
    private GameObject visualDragObj;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        skalaAsli = transform.localScale;
        posisiAsli = transform.position;

        totalSendokanMaks = kapasitasPorsi * sendokPerPorsi;
        stokSendokan = totalSendokanMaks;
        UpdateVisualRiceCooker();
    }

    void OnMouseDown() {
        if (sedangMasak) return;

        // Jika nasi HABIS (0), klik untuk mulai memasak ulang
        if (stokSendokan <= 0) {
            StartCoroutine(ProsesMasakUlang());
            return;
        }

        // Jika nasi MASIH ADA, mulai tarik (drag) nasi ke mangkuk
        sedangDrag = true;
        EfekMembal();

        if (gambarSendokNasi != null) {
            visualDragObj = new GameObject("SendokNasi_Drag");
            SpriteRenderer dragSr = visualDragObj.AddComponent<SpriteRenderer>();
            dragSr.sprite = gambarSendokNasi;
            dragSr.sortingOrder = 20;
            visualDragObj.transform.localScale = skalaAsli;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            visualDragObj.transform.position = mousePos;
        }
    }

    void OnMouseDrag() {
        if (!sedangDrag || visualDragObj == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        visualDragObj.transform.position = mousePos;
    }

    void OnMouseUp() {
        if (!sedangDrag) return;
        sedangDrag = false;

        if (visualDragObj != null) {
            Destroy(visualDragObj);
        }

        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

        foreach (Collider2D hit in hits) {
            BowlManager mangkuk = hit.GetComponent<BowlManager>();
            if (mangkuk != null) {
                // Stok hanya berkurang jika mangkuk memang sedang membutuhkan nasi
                if (mangkuk.ProcessIngredient("Nasi")) {
                    stokSendokan--;
                    UpdateVisualRiceCooker();
                    EfekMembal();
                }
                break;
            }
        }
    }

    void UpdateVisualRiceCooker() {
        if (sr == null) return;

        if (stokSendokan <= 0) {
            sr.sprite = gambarDrainZero;
        } 
        else if (stokSendokan <= sendokPerPorsi * 2) {
            // Sisa 1 - 6 sendokan (porsi ke-3 dan ke-4)
            sr.sprite = gambarDrain3;
        } 
        else if (stokSendokan <= sendokPerPorsi * 3) {
            // Sisa 7 - 9 sendokan (setelah porsi ke-1)
            sr.sprite = gambarDrain2;
        } 
        else {
            // Sisa 10 - 12 sendokan (awal penuh)
            sr.sprite = gambarFull;
        }
    }

    IEnumerator ProsesMasakUlang() {
        sedangMasak = true;
        float timer = 0f;
        sr.color = new Color(1f, 0.85f, 0.7f);

        while (timer < waktuMasakUlang) {
            timer += Time.deltaTime;
            float getarX = Random.Range(-0.03f, 0.03f);
            transform.position = new Vector3(posisiAsli.x + getarX, posisiAsli.y, posisiAsli.z);
            yield return null;
        }

        transform.position = posisiAsli;
        sr.color = Color.white;
        stokSendokan = totalSendokanMaks;
        sedangMasak = false;

        UpdateVisualRiceCooker();
        EfekMembal();
    }

    void EfekMembal() {
        StopCoroutine("AnimasiMembal");
        StartCoroutine("AnimasiMembal");
    }

    IEnumerator AnimasiMembal() {
        float timer = 0f;
        while (timer < 0.08f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli, skalaAsli * 1.12f, timer / 0.08f);
            yield return null;
        }
        timer = 0f;
        while (timer < 0.08f) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(skalaAsli * 1.12f, skalaAsli, timer / 0.08f);
            yield return null;
        }
        transform.localScale = skalaAsli;
    }
}