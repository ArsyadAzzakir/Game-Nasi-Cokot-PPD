using UnityEngine;
using TMPro;

[System.Serializable]
public class PengaturanHari {
    public string namaHari = "Hari 1";
    public float batasWaktu = 60f;          // Waktu shift (detik)
    public int targetPelanggan = 3;         // Syarat Bintang 1
    public int targetUang = 30000;          // Syarat Bintang 2
    public float targetWaktuSisa = 10f;     // Syarat Bintang 3 (Selesai sebelum waktu habis / sisa waktu minimal)
    
    [Header("Tingkat Kesulitan Hari Ini")]
    public int maxPelangganBersamaan = 1;   // Hari 1 cuma 1 pelanggan di meja!
    public int maxItemPerPesanan = 1;       // Hari 1 pesan 1 barang, Hari lanjut bisa 2 barang
    public int jumlahVarianNasiTerbuka = 1; // 1 = AsamManis, 2 = +Kecap, 3 = +Rica
    public bool airMineralTerbuka = false;  // Apakah pelanggan sudah boleh pesan minum?
}

public class GameManager : MonoBehaviour {
    public static GameManager instance; // Memudahkan script lain mengakses GameManager

    [Header("Status Permainan Saat Ini")]
    public int indexHariSekarang = 0; // 0 = Hari 1, 1 = Hari 2, dst (sampai 6 = Hari 7)
    public int uangHariIni = 0;
    public int pelangganTerlayani = 0;
    public float sisaWaktu;
    public bool shiftAktif = false;

    [Header("Harga & Tip (Rupiah)")]
    public int hargaNasiCokot = 10000;
    public int hargaAirMineral = 4000;
    public int maksimalTipPerPelanggan = 5000;

    [Header("Gembok & Aset Meja (Opsional)")]
    public GameObject[] objekGembokVarian; // Masukkan gambar gembok di atas kotak isian
    public GameObject tumpukanBotolAir;    // Masukkan objek bottle stack di meja

    [Header("UI Text (Opsional - Bisa Diisi Nanti)")]
    public TextMeshProUGUI teksUang;
    public TextMeshProUGUI teksWaktu;
    public TextMeshProUGUI teksTargetPelanggan;
    public TextMeshProUGUI teksHari;

    [Header("Konfigurasi 7 Hari")]
    public PengaturanHari[] daftarHari = new PengaturanHari[7];

    void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        IsiDefault7Hari();
    }

    void Start() {
        MulaiHari(indexHariSekarang);
    }

    void Update() {
        if (!shiftAktif) return;

        sisaWaktu -= Time.deltaTime;
        UpdateUI();

        if (sisaWaktu <= 0) {
            sisaWaktu = 0;
            AkhiriHari();
        }
    }

    public void MulaiHari(int indexHari) {
        indexHariSekarang = Mathf.Clamp(indexHari, 0, daftarHari.Length - 1);
        PengaturanHari hari = AmbilHariIni();

        uangHariIni = 0;
        pelangganTerlayani = 0;
        sisaWaktu = hari.batasWaktu;
        shiftAktif = true;

        // Atur visibilitas tumpukan botol air sesuai hari
        if (tumpukanBotolAir != null) {
            tumpukanBotolAir.SetActive(hari.airMineralTerbuka);
        }

        // Atur gembok varian isian
        for (int i = 0; i < objekGembokVarian.Length; i++) {
            if (objekGembokVarian[i] != null) {
                // Jika index di bawah jumlah varian terbuka, sembunyikan gemboknya
                objekGembokVarian[i].SetActive(i >= hari.jumlahVarianNasiTerbuka);
            }
        }

        UpdateUI();
        Debug.Log("=== MULAI " + hari.namaHari + " ===");
    }

    public PengaturanHari AmbilHariIni() {
        return daftarHari[indexHariSekarang];
    }

    public void CatatPesananSelesai(int bayaranPokok, int uangTip) {
        if (!shiftAktif) return;

        int totalDiterima = bayaranPokok + uangTip;
        uangHariIni += totalDiterima;
        pelangganTerlayani++;

        Debug.Log($"Transaksi Sukses! Pokok: Rp{bayaranPokok} + Tip Cepat: Rp{uangTip} | Total Kas: Rp{uangHariIni}");
        UpdateUI();

        // Cek apakah target pelanggan & uang sudah tembus lebih cepat untuk klaim Bintang 3
        PengaturanHari hari = AmbilHariIni();
        if (pelangganTerlayani >= hari.targetPelanggan && uangHariIni >= hari.targetUang) {
            Debug.Log("Target Harian Tercapai Lebih Awal! Sisa Waktu: " + Mathf.CeilToInt(sisaWaktu) + " detik.");
        }
    }

    void AkhiriHari() {
        shiftAktif = false;
        PengaturanHari hari = AmbilHariIni();

        bool bintang1 = pelangganTerlayani >= hari.targetPelanggan;
        bool bintang2 = uangHariIni >= hari.targetUang;
        bool bintang3 = bintang1 && bintang2 && (sisaWaktu >= hari.targetWaktuSisa);

        int totalBintang = (bintang1 ? 1 : 0) + (bintang2 ? 1 : 0) + (bintang3 ? 1 : 0);

        Debug.Log($"=== {hari.namaHari} SELESAI! ===");
        Debug.Log($"Pelanggan: {pelangganTerlayani}/{hari.targetPelanggan} | Uang: Rp{uangHariIni}/Rp{hari.targetUang}");
        Debug.Log($"Perolehan Bintang: {totalBintang} Bintang!");
    }

    void UpdateUI() {
        PengaturanHari hari = AmbilHariIni();
        if (teksUang != null) teksUang.text = "Rp " + uangHariIni.ToString("N0");
        if (teksWaktu != null) teksWaktu.text = Mathf.CeilToInt(sisaWaktu).ToString() + "s";
        if (teksTargetPelanggan != null) teksTargetPelanggan.text = pelangganTerlayani + "/" + hari.targetPelanggan;
        if (teksHari != null) teksHari.text = hari.namaHari;
    }

    // Mengisi nilai awal 7 hari otomatis jika belum diatur di Inspector
    void IsiDefault7Hari() {
        if (daftarHari == null || daftarHari.Length != 7) daftarHari = new PengaturanHari[7];
        
        for (int i = 0; i < 7; i++) {
            if (daftarHari[i] == null) daftarHari[i] = new PengaturanHari();
            daftarHari[i].namaHari = "Hari " + (i + 1);
            daftarHari[i].batasWaktu = 60f + (i * 15f); // Waktu makin panjang karena pesanan makin banyak
            daftarHari[i].targetPelanggan = 3 + (i * 2); // Hari 1: 3 orang, Hari 2: 5 orang, dst
            daftarHari[i].targetUang = daftarHari[i].targetPelanggan * 12000;
            daftarHari[i].targetWaktuSisa = 5f;

            // Pengaturan Kesulitan Bertahap:
            daftarHari[i].maxPelangganBersamaan = (i == 0) ? 1 : (i < 3 ? 2 : 3); // Hari 1 cuma 1 pelanggan!
            daftarHari[i].maxItemPerPesanan = (i == 0) ? 1 : 2; // Mulai Hari 2 bisa pesan 2 barang
            daftarHari[i].jumlahVarianNasiTerbuka = (i == 0) ? 1 : (i == 1 ? 2 : 3);
            daftarHari[i].airMineralTerbuka = (i >= 1); // Air mineral buka mulai Hari 2
        }
    }
}