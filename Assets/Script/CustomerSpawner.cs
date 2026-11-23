using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour {
    [Header("Referensi Prefab & Slot")]
    public GameObject[] prefabPelanggan;
    public Transform[] titikSlotPelanggan;
    public float jarakSpawnLuarLayar = 12f;

    private bool[] slotTerisi;
    private int kuotaTotalHariIni = 10;
    private int maxPelangganBersamaan = 1;
    private int sudahSpawn = 0;
    private int pelangganAktif = 0;
    private float jedaSpawn = 3f;
    private float kesabaranHariIni = 25f;
    private bool hariSelesaiDipicu = false;

    void Start() {
        slotTerisi = new bool[titikSlotPelanggan.Length];
        maxPelangganBersamaan = Mathf.Max(1, titikSlotPelanggan.Length);

        if (GameManager.instance != null) {
            PengaturanHari hari = GameManager.instance.AmbilHariIni();
            kuotaTotalHariIni = hari.TotalPelangganDatang;
            jedaSpawn = hari.jedaSpawnPelanggan;
            kesabaranHariIni = hari.kesabaranPelanggan;

            int batasHariIni = hari.maxPelangganBersamaan > 0 ? hari.maxPelangganBersamaan : 1;
            maxPelangganBersamaan = Mathf.Clamp(batasHariIni, 1, titikSlotPelanggan.Length);
        }

        StartCoroutine(LoopSpawnPelanggan());
    }

    IEnumerator LoopSpawnPelanggan() {
        yield return new WaitForSeconds(1f);

        while (sudahSpawn < kuotaTotalHariIni) {
            if (pelangganAktif < maxPelangganBersamaan) {
                int slotKosong = CariSlotKosong();
                if (slotKosong != -1) {
                    SpawnDiSlot(slotKosong);
                    yield return new WaitForSeconds(jedaSpawn);
                    continue;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    int CariSlotKosong() {
        // Jika batas hari ini cuma 1 pelanggan, taruh di slot paling tengah
        if (maxPelangganBersamaan == 1 && titikSlotPelanggan.Length > 0) {
            int slotTengah = -1;
            float jarakTerkecilKeTengah = float.MaxValue;

            for (int i = 0; i < slotTerisi.Length; i++) {
                if (!slotTerisi[i]) {
                    float jarakX = Mathf.Abs(titikSlotPelanggan[i].position.x);
                    if (jarakX < jarakTerkecilKeTengah) {
                        jarakTerkecilKeTengah = jarakX;
                        slotTengah = i;
                    }
                }
            }
            if (slotTengah != -1) return slotTengah;
        }

        for (int i = 0; i < slotTerisi.Length; i++) {
            if (!slotTerisi[i]) return i;
        }
        return -1;
    }

    void SpawnDiSlot(int indexSlot) {
        if (prefabPelanggan == null || prefabPelanggan.Length == 0) return;

        slotTerisi[indexSlot] = true;
        sudahSpawn++;
        pelangganAktif++;

        Transform target = titikSlotPelanggan[indexSlot];
        float spawnX = target.position.x < 0 ? -jarakSpawnLuarLayar : jarakSpawnLuarLayar;
        Vector3 posisiAwal = new Vector3(spawnX, target.position.y, target.position.z);

        GameObject prefabAcak = prefabPelanggan[Random.Range(0, prefabPelanggan.Length)];
        GameObject obj = Instantiate(prefabAcak, posisiAwal, Quaternion.identity);

        Customer cust = obj.GetComponent<Customer>();
        if (cust != null) {
            cust.spawner = this;
            cust.slotIndex = indexSlot;
            cust.targetPosisi = target.position;
            if (kesabaranHariIni > 0f) cust.kesabaran = kesabaranHariIni;
        }
    }

    public void KosongkanSlot(int indexSlot) {
        if (indexSlot >= 0 && indexSlot < slotTerisi.Length) {
            slotTerisi[indexSlot] = false;
        }

        pelangganAktif = Mathf.Max(0, pelangganAktif - 1);

        if (sudahSpawn >= kuotaTotalHariIni && pelangganAktif <= 0 && !hariSelesaiDipicu) {
            hariSelesaiDipicu = true;
            StartCoroutine(TungguLaluAkhiriHari());
        }
    }

    IEnumerator TungguLaluAkhiriHari() {
        yield return new WaitForSeconds(1.5f);
        if (GameManager.instance != null) {
            GameManager.instance.AkhiriHari();
        }
    }
}