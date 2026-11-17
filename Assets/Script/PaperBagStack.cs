using UnityEngine;

public class PaperBagStack : MonoBehaviour {
    public PaperBagManager kantongMeja; // Tarik objek kemasan.png di meja ke slot ini

    void OnMouseDown() {
        if (kantongMeja != null && !kantongMeja.gameObject.activeSelf) {
            kantongMeja.AktifkanKantongBaru();
        }
    }
}