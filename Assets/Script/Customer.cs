using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Customer : MonoBehaviour {
    public List<string> daftarPesanan = new List<string>(); 
    public float kesabaran = 25f; 
    
    public string[] semuaMenuNasi = { "Isian_AyamPedas", "Isian_SuwirKecap", "Isian_AyamRica" };

    [HideInInspector] public CustomerSpawner spawner;
    [HideInInspector] public int slotIndex;
    [HideInInspector] public Vector3 targetPosisi;

    public float kecepatanJalan = 3f;
    public float tinggiLoncat = 0.2f;
    public float kecepatanLoncat = 15f;

    [Header("Visual Pesanan (1 atau 2 Item)")]
    public GameObject bubbleObject; 
    public SpriteRenderer pesananIconRenderer;  
    public SpriteRenderer pesananIconRenderer2; 
    public Sprite gambarAyamRica;
    public Sprite gambarAyamPedas;
    public Sprite gambarSuwirAyam;
    public Sprite gambarAirMineral; 

    [Header("Ekspresi Emosi")]
    public Sprite ekspresiNormal;   
    public Sprite ekspresiGelisah;  
    public Sprite ekspresiMarah;    
    
    [Header("Aset Floating Text (Gambar)")]
    public GameObject floatingSpritePrefab; 
    public Sprite[] asetTeksBenar; 
    public Sprite[] asetTeksSalah; 
    public Sprite[] asetTeksMarah; 

    private SpriteRenderer karakterRenderer; 
    private bool sudahSampai = false;
    private float waktuTunggu;
    private Vector3 basePosition;
    private bool sedangReaksi = false; 
    private int totalHargaPokok = 0;

    // VARIABEL BARU UNTUK POSISI TENGAH BALON
    private Vector3 posisiAwalIkon1;
    private float posisiTengahX;

    void Start() {
        waktuTunggu = kesabaran;
        basePosition = transform.position;
        karakterRenderer = GetComponent<SpriteRenderer>();
        
        // MENGHITUNG TITIK TENGAH SECARA OTOMATIS
        if (pesananIconRenderer != null) posisiAwalIkon1 = pesananIconRenderer.transform.localPosition;
        if (pesananIconRenderer != null && pesananIconRenderer2 != null) {
            posisiTengahX = (pesananIconRenderer.transform.localPosition.x + pesananIconRenderer2.transform.localPosition.x) / 2f;
        } else if (pesananIconRenderer != null) {
            posisiTengahX = 0f;
        }

        if (bubbleObject != null) bubbleObject.SetActive(false);
        if (ekspresiNormal != null) karakterRenderer.sprite = ekspresiNormal;
        
        BuatPesananSesuaiHari();
        UpdateIkonBalon();
    }

    void BuatPesananSesuaiHari() {
        daftarPesanan.Clear();
        totalHargaPokok = 0;

        int maxItem = 1;
        int varianTerbuka = 3;
        bool bolehMinum = false;
        int hargaNasi = 10000;
        int hargaMinum = 4000;

        if (GameManager.instance != null) {
            PengaturanHari hari = GameManager.instance.AmbilHariIni();
            maxItem = hari.maxItemPerPesanan;
            varianTerbuka = Mathf.Clamp(hari.jumlahVarianNasiTerbuka, 1, semuaMenuNasi.Length);
            bolehMinum = hari.airMineralTerbuka;
            hargaNasi = GameManager.instance.hargaNasiCokot;
            hargaMinum = GameManager.instance.hargaAirMineral;
        }

        int jumlahPesanan = Random.Range(1, maxItem + 1);

        string nasi1 = semuaMenuNasi[Random.Range(0, varianTerbuka)];
        daftarPesanan.Add(nasi1);
        totalHargaPokok += hargaNasi;

        if (jumlahPesanan > 1) {
            if (bolehMinum && Random.value > 0.5f) {
                daftarPesanan.Add("Minum_Air");
                totalHargaPokok += hargaMinum;
            } else {
                string nasi2 = semuaMenuNasi[Random.Range(0, varianTerbuka)];
                daftarPesanan.Add(nasi2);
                totalHargaPokok += hargaNasi;
            }
        }
    }

    void UpdateIkonBalon() {
        if (pesananIconRenderer != null) {
            if (daftarPesanan.Count >= 1) {
                pesananIconRenderer.gameObject.SetActive(true);
                pesananIconRenderer.sprite = AmbilGambarMenu(daftarPesanan[0]);

                // LOGIKA CERDAS: Kalau pesanan sisa 1, geser ikon ke titik tengah balon!
                if (daftarPesanan.Count == 1) {
                    pesananIconRenderer.transform.localPosition = new Vector3(posisiTengahX, posisiAwalIkon1.y, posisiAwalIkon1.z);
                } else {
                    pesananIconRenderer.transform.localPosition = posisiAwalIkon1; // Kembalikan ke kiri
                }
            } else {
                pesananIconRenderer.gameObject.SetActive(false);
            }
        }

        if (pesananIconRenderer2 != null) {
            if (daftarPesanan.Count >= 2) {
                pesananIconRenderer2.gameObject.SetActive(true);
                pesananIconRenderer2.sprite = AmbilGambarMenu(daftarPesanan[1]);
            } else {
                pesananIconRenderer2.gameObject.SetActive(false);
            }
        }
    }

    Sprite AmbilGambarMenu(string kodeMenu) {
        if (kodeMenu.Contains("Rica")) return gambarAyamRica;
        if (kodeMenu.Contains("Pedas") || kodeMenu.Contains("AsamManis")) return gambarAyamPedas;
        if (kodeMenu.Contains("Kecap") || kodeMenu.Contains("SuwirAyam") || kodeMenu.Contains("SuwirKecap")) return gambarSuwirAyam;
        if (kodeMenu.Contains("Air") || kodeMenu.Contains("Minum")) return gambarAirMineral;
        return null;
    }

    void Update() {
        if (sedangReaksi) return; 

        if (!sudahSampai) {
            basePosition = Vector3.MoveTowards(basePosition, targetPosisi, kecepatanJalan * Time.deltaTime);
            float efekLoncat = Mathf.Abs(Mathf.Sin(Time.time * kecepatanLoncat)) * tinggiLoncat;
            transform.position = new Vector3(basePosition.x, basePosition.y + efekLoncat, basePosition.z);

            if (Vector3.Distance(basePosition, targetPosisi) < 0.05f) {
                sudahSampai = true;
                transform.position = targetPosisi; 
                if (bubbleObject != null) bubbleObject.SetActive(true);
            }
        } 
        else {
            waktuTunggu -= Time.deltaTime;
            float rasioWaktu = waktuTunggu / kesabaran;
            
            if (rasioWaktu > 0.5f) {
                if (ekspresiNormal != null) karakterRenderer.sprite = ekspresiNormal;
                karakterRenderer.color = Color.white; 
            } 
            else if (rasioWaktu > 0.25f) {
                if (ekspresiGelisah != null) karakterRenderer.sprite = ekspresiGelisah;
                karakterRenderer.color = Color.white;
            } 
            else {
                if (ekspresiMarah != null) karakterRenderer.sprite = ekspresiMarah;
                karakterRenderer.color = new Color(1f, 0.6f, 0.6f); 
                transform.position = new Vector3(targetPosisi.x + Random.Range(-0.02f, 0.02f), targetPosisi.y, targetPosisi.z);
            }

            if (waktuTunggu <= 0) {
                if (asetTeksMarah.Length > 0) {
                    MunculkanTeks(asetTeksMarah[Random.Range(0, asetTeksMarah.Length)], false);
                }
                StartCoroutine(ReactAndLeave(false)); 
            }
        }
    }

    public void TerimaPesanan(string itemDiberikan) {
        if (!sudahSampai || sedangReaksi) return; 

        if (daftarPesanan.Contains(itemDiberikan)) {
            daftarPesanan.Remove(itemDiberikan); 
            UpdateIkonBalon(); 

            if (daftarPesanan.Count == 0) {
                HitungBayaranDanTip();
                if (asetTeksBenar.Length > 0) MunculkanTeks(asetTeksBenar[Random.Range(0, asetTeksBenar.Length)], true);
                StartCoroutine(ReactAndLeave(true));
            } else {
                StartCoroutine(LompatKecil());
            }
        } 
        else {
            if (asetTeksSalah.Length > 0) MunculkanTeks(asetTeksSalah[Random.Range(0, asetTeksSalah.Length)], false);
            StartCoroutine(ReactAndLeave(false)); 
        }
    }

    public void TerimaKantongPesanan(List<string> isiKantong) {
        if (!sudahSampai || sedangReaksi) return;

        foreach (string item in isiKantong) {
            if (daftarPesanan.Contains(item)) {
                daftarPesanan.Remove(item);
            } else {
                if (asetTeksSalah.Length > 0) MunculkanTeks(asetTeksSalah[Random.Range(0, asetTeksSalah.Length)], false);
                StartCoroutine(ReactAndLeave(false));
                return;
            }
        }

        UpdateIkonBalon();

        if (daftarPesanan.Count == 0) {
            HitungBayaranDanTip();
            if (asetTeksBenar.Length > 0) MunculkanTeks(asetTeksBenar[Random.Range(0, asetTeksBenar.Length)], true);
            StartCoroutine(ReactAndLeave(true));
        } else {
            StartCoroutine(LompatKecil());
        }
    }

    void HitungBayaranDanTip() {
        if (GameManager.instance == null) return;

        float rasioSisaWaktu = Mathf.Clamp01(waktuTunggu / kesabaran);
        int tipMentah = Mathf.RoundToInt(rasioSisaWaktu * GameManager.instance.maksimalTipPerPelanggan);
        int uangTip = (tipMentah / 500) * 500; 

        GameManager.instance.CatatPesananSelesai(totalHargaPokok, uangTip);
    }

    IEnumerator LompatKecil() {
        float timer = 0;
        while (timer < 0.15f) {
            timer += Time.deltaTime;
            float lompat = Mathf.Sin((timer / 0.15f) * Mathf.PI) * 0.25f;
            transform.position = targetPosisi + new Vector3(0, lompat, 0);
            yield return null;
        }
        transform.position = targetPosisi;
    }

    void MunculkanTeks(Sprite gambarTeks, bool keAtas) {
        if (floatingSpritePrefab != null && gambarTeks != null) {
            GameObject teksObj = Instantiate(floatingSpritePrefab, transform.position + new Vector3(0, 1.5f, -1f), Quaternion.identity);
            FloatingText scriptTeks = teksObj.GetComponent<FloatingText>();
            if (scriptTeks != null) scriptTeks.Setup(gambarTeks, keAtas);
        }
    }

    IEnumerator ReactAndLeave(bool senang) {
        sedangReaksi = true; 
        if (bubbleObject != null) {
            bubbleObject.transform.SetParent(null); 
            StartCoroutine(FloatingBubble(bubbleObject, senang));
        }

        float timer = 0;
        Vector3 posisiAwal = targetPosisi; 

        if (senang) {
            if (ekspresiNormal != null) karakterRenderer.sprite = ekspresiNormal; 
            karakterRenderer.color = Color.white;
            while (timer < 0.3f) {
                timer += Time.deltaTime;
                float lompat = Mathf.Sin((timer / 0.3f) * Mathf.PI) * 0.5f;
                transform.position = posisiAwal + new Vector3(0, lompat, 0);
                yield return null;
            }
        } else {
            karakterRenderer.color = new Color(1f, 0.5f, 0.5f); 
            while (timer < 0.3f) {
                timer += Time.deltaTime;
                transform.position = posisiAwal + new Vector3(Random.Range(-0.1f, 0.1f), 0, 0);
                yield return null;
            }
        }

        if (spawner != null) spawner.KosongkanSlot(slotIndex);

        Vector3 targetKabur = posisiAwal.x < 0 ? new Vector3(-15, posisiAwal.y, posisiAwal.z) : new Vector3(15, posisiAwal.y, posisiAwal.z);
        Vector3 posisiSekarang = posisiAwal;

        while (Vector3.Distance(transform.position, targetKabur) > 0.5f) {
            posisiSekarang = Vector3.MoveTowards(posisiSekarang, targetKabur, kecepatanJalan * 1.5f * Time.deltaTime); 
            float efekLoncat = Mathf.Abs(Mathf.Sin(Time.time * kecepatanLoncat * 1.5f)) * tinggiLoncat;
            transform.position = new Vector3(posisiSekarang.x, posisiSekarang.y + efekLoncat, posisiSekarang.z);
            yield return null;
        }
        Destroy(gameObject); 
    }

    IEnumerator FloatingBubble(GameObject balon, bool benar) {
        float timer = 0;
        Vector3 posisiAwal = balon.transform.position;
        Vector3 posisiTarget = benar ? posisiAwal + new Vector3(0, 2f, 0) : posisiAwal + new Vector3(0, -2f, 0);
        Vector3 skalaAwal = balon.transform.localScale; 
        
        SpriteRenderer sr = balon.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = benar ? Color.green : Color.red;

        while (timer < 0.5f) {
            timer += Time.deltaTime;
            balon.transform.position = Vector3.Lerp(posisiAwal, posisiTarget, timer / 0.5f);
            balon.transform.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, timer / 0.5f);
            yield return null;
        }
        Destroy(balon); 
    }
}