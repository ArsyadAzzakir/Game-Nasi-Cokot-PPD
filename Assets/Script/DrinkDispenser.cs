using UnityEngine;

public class DrinkDispenser : MonoBehaviour {
    public Sprite gambarBotolSatuan; // Masukkan air mineral single.png ke sini
    private GameObject botolMelayang;

    void OnMouseDown() {
        botolMelayang = new GameObject("Dragging_AirMineral");
        SpriteRenderer sr = botolMelayang.AddComponent<SpriteRenderer>();
        sr.sprite = gambarBotolSatuan;
        sr.sortingOrder = 20;

        botolMelayang.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        botolMelayang.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
        UpdatePosisi();
    }

    void OnMouseDrag() {
        if (botolMelayang != null) UpdatePosisi();
    }

    void OnMouseUp() {
        if (botolMelayang != null) {
            Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

            foreach (Collider2D hit in hits) {
                // 1. Cek apakah dimasukkan ke dalam Paper Bag
                PaperBagManager kantong = hit.GetComponent<PaperBagManager>();
                if (kantong != null && kantong.MasukkanBarang("Minum_Air")) {
                    break;
                }

                // 2. Atau diserahkan langsung ke Pelanggan
                Customer pelanggan = hit.GetComponent<Customer>();
                if (pelanggan != null) {
                    pelanggan.TerimaPesanan("Minum_Air");
                    break;
                }
            }
            Destroy(botolMelayang);
        }
    }

    void UpdatePosisi() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        botolMelayang.transform.position = mousePos;
    }
}