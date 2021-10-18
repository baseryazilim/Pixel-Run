using UnityEngine;

// Player'ın Rigidbody'sini enable/disable etmeye yarayan script
// Player'ın ZiplamaAnim isimli animasyonu bu scriptteki fonksiyonları kullanmakta
public class RigidbodyKontrol : MonoBehaviour 
{
	private Rigidbody r;
	
	void Start()
	{
		// Player'ın Rigidbody'sini değişkene at
		r = transform.parent.GetComponent<Rigidbody>();
	}
	
	void RigidbodyAc()
	{
		// Rigidbody'i aktifleştir
		r.isKinematic = false;
		
		// Player'ın zıplama animasyonu biterken Player aşağı iniyor vaziyette oluyor
		// Eğer ki rigidbody'e aşağı yönde güç vermezsek animasyon bitince player y ekseninde
		// önce bir duraksıyor (eğer boşluğa denk geldiyse), ondan sonra yerçekimi etkisiyle
		// aşağı yavaşça düşmeye başlıyor
		r.velocity = new Vector3( r.velocity.x, -10f, r.velocity.z );
	}
	
	void RigidbodyKapa()
	{
		// Rigidbody'i disable et
		r.isKinematic = true;
	}
}
