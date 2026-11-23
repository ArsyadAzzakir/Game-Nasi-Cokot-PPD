using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class PengaturanHari {
    public string namaHari = "Hari 1";
    [Header("Target & Kuota Pelanggan")]
    public int targetPelanggan = 6;
    public int kelebihanPelanggan = 7; // Total datang = target + kelebihan
    public int targetUangHarian = 60000;

    [Header("Pengaturan Menu & Ritme")]
    public int maxPelangganBersamaan = 1; 
    public int jumlahVarianNasiTerbuka = 3;
    public int maxItemPerPesanan = 1;
    public bool airMineralTerbuka = false;
    public float jedaSpawnPelanggan = 3.5f;
    public float kesabaranPelanggan = 25f;

    public int TotalPelangganDatang => targetPelanggan + kelebihanPelanggan;
    
}

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public static int indexHariAktif = 0; // Menyimpan progres hari saat scene di-reload

    [Header("Debug / Tes Hari Tertentu")]
    [Tooltip("Set 1-7 untuk langsung tes hari tertentu, set 0 untuk jalan normal dari Hari 1")]
    public int tesMulaiDariHari = 0;

    [Header("Harga & Tip")]
    public int hargaNasiCokot = 10000;
    public int hargaAirMineral = 4000;
    public int maksimalTipPerPelanggan = 3000;

    [Header("Pengaturan Hari 1 - 7")]
    public List<PengaturanHari> daftarHari = new List<PengaturanHari>();

    [Header("UI HUD Target Harian")]
    public TMP_Text teksSisaPelanggan;     // Angka sisa target di kotak ikon 3 orang
    public Image fillBarTargetUang;        // Bar pengisi di dalam kotak coklat gelap
    public TMP_Text teksProgressUang;      // Opsional: "Rp 40.000 / Rp 60.000"
    public TMP_Text teksNamaHari;          // Opsional: "Hari 1"
    public float kecepatanAnimasiBar = 5f;

    [Header("Panel Evaluasi Akhir Hari (Opsional)")]
    public GameObject panelAkhirHari;
    public TMP_Text teksStatusHari;
    public TMP_Text teksRingkasanHari;

    // Status berjalan hari ini
    private int pelangganBerhasil = 0;
    private int uangHariIni = 0;
    private float targetFillBar = 0f;
    private bool hariSudahSelesai = false;

    void Reset() {
        InisialisasiDefault7Hari();
    }

    void Awake() {
        instance = this;

        if (daftarHari == null || daftarHari.Count == 0) {
            InisialisasiDefault7Hari();
        }

        if (tesMulaiDariHari >= 1 && tesMulaiDariHari <= daftarHari.Count) {
            indexHariAktif = tesMulaiDariHari - 1;
        }
        indexHariAktif = Mathf.Clamp(indexHariAktif, 0, daftarHari.Count - 1);
    }

    void Start() {
        pelangganBerhasil = 0;
        uangHariIni = 0;
        targetFillBar = 0f;
        hariSudahSelesai = false;

        if (fillBarTargetUang != null) fillBarTargetUang.fillAmount = 0f;
        if (panelAkhirHari != null) panelAkhirHari.SetActive(false);

        UpdateTampilanHUD();
    }

    void Update() {
        // Animasi pergerakan bar uang harian agar naik mulus
        if (fillBarTargetUang != null) {
            fillBarTargetUang.fillAmount = Mathf.Lerp(
                fillBarTargetUang.fillAmount,
                targetFillBar,
                Time.deltaTime * kecepatanAnimasiBar
            );
        }
    }

    void InisialisasiDefault7Hari() {
        daftarHari = new List<PengaturanHari> {
            new PengaturanHari { namaHari = "Hari 1", targetPelanggan = 6,  kelebihanPelanggan = 7, targetUangHarian = 60000,  jumlahVarianNasiTerbuka = 3, maxItemPerPesanan = 1, airMineralTerbuka = false, jedaSpawnPelanggan = 3.5f, kesabaranPelanggan = 28f },
            new PengaturanHari { namaHari = "Hari 2", targetPelanggan = 8,  kelebihanPelanggan = 6, targetUangHarian = 90000,  jumlahVarianNasiTerbuka = 3, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 3.2f, kesabaranPelanggan = 26f },
            new PengaturanHari { namaHari = "Hari 3", targetPelanggan = 10, kelebihanPelanggan = 5, targetUangHarian = 120000, jumlahVarianNasiTerbuka = 4, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 3.0f, kesabaranPelanggan = 25f },
            new PengaturanHari { namaHari = "Hari 4", targetPelanggan = 12, kelebihanPelanggan = 4, targetUangHarian = 150000, jumlahVarianNasiTerbuka = 5, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 2.8f, kesabaranPelanggan = 24f },
            new PengaturanHari { namaHari = "Hari 5", targetPelanggan = 14, kelebihanPelanggan = 3, targetUangHarian = 180000, jumlahVarianNasiTerbuka = 6, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 2.6f, kesabaranPelanggan = 23f },
            new PengaturanHari { namaHari = "Hari 6", targetPelanggan = 16, kelebihanPelanggan = 2, targetUangHarian = 210000, jumlahVarianNasiTerbuka = 6, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 2.4f, kesabaranPelanggan = 22f },
            new PengaturanHari { namaHari = "Hari 7", targetPelanggan = 18, kelebihanPelanggan = 1, targetUangHarian = 250000, jumlahVarianNasiTerbuka = 6, maxItemPerPesanan = 2, airMineralTerbuka = true,  jedaSpawnPelanggan = 2.2f, kesabaranPelanggan = 20f }
        };
    }

    public PengaturanHari AmbilHariIni() {
        return daftarHari[Mathf.Clamp(indexHariAktif, 0, daftarHari.Count - 1)];
    }

    public void CatatPesananSelesai(int hargaPokok, int uangTip) {
        if (hariSudahSelesai) return;

        pelangganBerhasil++;
        uangHariIni += (hargaPokok + uangTip);
        UpdateTampilanHUD();
    }

    void UpdateTampilanHUD() {
        PengaturanHari hari = AmbilHariIni();

        // Sisa pelanggan terus berkurang saat berhasil melayani (berhenti di 0)
        int sisaTarget = Mathf.Max(0, hari.targetPelanggan - pelangganBerhasil);
        if (teksSisaPelanggan != null) {
            teksSisaPelanggan.text = sisaTarget.ToString();
        }

        // Hitung rasio bar target harian (0.0 sampai 1.0)
        targetFillBar = hari.targetUangHarian > 0
            ? Mathf.Clamp01((float)uangHariIni / hari.targetUangHarian)
            : 1f;

        if (teksProgressUang != null) {
            teksProgressUang.text = $"Rp {uangHariIni:N0} / Rp {hari.targetUangHarian:N0}";
        }

        if (teksNamaHari != null) {
            teksNamaHari.text = hari.namaHari;
        }
    }

    public void AkhiriHari() {
        if (hariSudahSelesai) return;
        hariSudahSelesai = true;

        PengaturanHari hari = AmbilHariIni();
        bool lulusTarget = pelangganBerhasil >= hari.targetPelanggan && uangHariIni >= hari.targetUangHarian;

        Debug.Log($"[{hari.namaHari} SELESAI] Dilayani: {pelangganBerhasil}/{hari.targetPelanggan} | Uang: Rp {uangHariIni:N0}/Rp {hari.targetUangHarian:N0} | Lulus: {lulusTarget}");

        if (panelAkhirHari != null) {
            panelAkhirHari.SetActive(true);
            if (teksStatusHari != null) {
                teksStatusHari.text = lulusTarget ? $"{hari.namaHari} Berhasil!" : $"{hari.namaHari} Gagal!";
            }
            if (teksRingkasanHari != null) {
                teksRingkasanHari.text = $"Pelanggan: {pelangganBerhasil} / {hari.targetPelanggan}\nPendapatan: Rp {uangHariIni:N0} / Rp {hari.targetUangHarian:N0}";
            }
        }
    }

    public void LanjutHariBerikutnya() {
        tesMulaiDariHari = 0;
        if (indexHariAktif < daftarHari.Count - 1) {
            indexHariAktif++;
        } else {
            indexHariAktif = 0; // Kembali ke Hari 1 setelah tamat Hari 7
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void UlangiHari() {
        tesMulaiDariHari = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}