using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour {
    public GameObject[] customerPrefabs; 
    public Transform[] titikMuncul; 
    public Transform[] titikAntrean; 
    
    [Header("Pengaturan Waktu Muncul (Detik)")]
    public float waktuMinimum = 3f; 
    public float waktuMaksimum = 7f; 

    private float timer;
    private bool[] slotTerisi;

    void Start() {
        timer = 1.5f; 
        slotTerisi = new bool[titikAntrean.Length];
    }

    void Update() {
        // Jangan munculkan pelanggan jika shift hari sudah berakhir
        if (GameManager.instance != null && !GameManager.instance.shiftAktif) return;

        timer -= Time.deltaTime;
        if (timer <= 0) {
            MunculkanPelanggan();
            timer = Random.Range(waktuMinimum, waktuMaksimum); 
        }
    }

    void MunculkanPelanggan() {
        // Hitung berapa pelanggan yang sedang ada di meja sekarang
        int pelangganAktif = 0;
        List<int> slotKosong = new List<int>();

        for (int i = 0; i < slotTerisi.Length; i++) {
            if (slotTerisi[i]) pelangganAktif++;
            else slotKosong.Add(i);
        }

        // Cek batas maksimal pelanggan bersamaan hari ini dari GameManager
        int batasPelanggan = 3;
        if (GameManager.instance != null) {
            batasPelanggan = GameManager.instance.AmbilHariIni().maxPelangganBersamaan;
        }

        // Jika jumlah pelanggan di meja sudah mencapai batas hari ini (misal Hari 1 batasnya 1), tahan dulu!
        if (pelangganAktif >= batasPelanggan) return;

        if (slotKosong.Count > 0) {
            int randomIndex = Random.Range(0, slotKosong.Count);
            int slotTerpilih = slotKosong[randomIndex];

            int prefabAcak = Random.Range(0, customerPrefabs.Length);
            int munculAcak = Random.Range(0, titikMuncul.Length);

            GameObject pelangganBaru = Instantiate(customerPrefabs[prefabAcak], titikMuncul[munculAcak].position, Quaternion.identity);
            
            Customer scriptPelanggan = pelangganBaru.GetComponent<Customer>();
            scriptPelanggan.spawner = this; 
            scriptPelanggan.slotIndex = slotTerpilih; 
            scriptPelanggan.targetPosisi = titikAntrean[slotTerpilih].position; 

            slotTerisi[slotTerpilih] = true;
        }
    }

    public void KosongkanSlot(int index) {
        slotTerisi[index] = false;
    }
}