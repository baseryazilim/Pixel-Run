using UnityEngine;

// Skoru depolayan ve yüksekskoru diske kaydetmeye yarayan script
public class SkorScript : MonoBehaviour
{
	private float skor = 0f;
	
	public UnityEngine.UI.Text textPanel;
	
	void Start()
	{
		// Oyunun başında hem bu script hem de skoru çizdirmeye yarayan
		// UI elemanı disable vaziyette oluyor. Oyuna başladığımızda AnaMenu 
		// scripti bu scripti aktif ediyor ve bu scriptin Start fonksiyonu da 
		// skoru çizdiren UI elemanını aktif ediyor
		textPanel.enabled = true;
	}
	
	// skoru integer olarak döndürmeye yarayan fonksiyon
	public int SkoruAl()
	{
		return (int) skor;
	}
	
	public void SkoruGuncelle( float delta )
	{
		// skoru artır/azalt
		skor += delta;
		
		// skoru çizdiren UI elemanını güncelle
		// burada skor'u <color=yellow> </color> etiketleri arasına alarak
		// UI elemanında skor'un sarı renkle çizdirilmesini sağlıyoruz
		textPanel.text = "Skor: <color=white>" + (int) skor + "</color>";
	}
	
	public void YuksekSkorKaydet()
	{
		// eğer elde ettiğimiz skor yüksekskordan daha iyiyse bu skoru
		// yüksekskor olarak diske kaydet
		if( skor > PlayerPrefs.GetInt( "YuksekSkor" ) )
		{
			PlayerPrefs.SetInt( "YuksekSkor", (int) skor );
			PlayerPrefs.Save();
		}
	}
}
