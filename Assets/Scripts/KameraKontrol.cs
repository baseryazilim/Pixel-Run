using UnityEngine;


// Kameranın karakteri (hedef) takip etmesini sağlayan script
public class KameraKontrol : MonoBehaviour 
{
	// Takip edilecek obje
	public Transform hedef;
	
	// Kamera objesinin kendi Transform'u
	private Transform tr;
	
	// Kamerayla hedef arasındaki istenen uzaklık (yükseklik farkını hesaba katmadan)
	public float uzaklik = 6f;
	// Kamerayla hedef arasındanki istenen yükseklik farkı
	public float yukseklik = 5f;
	
	void Start()
	{
		tr = transform;
	}

	
	void FixedUpdate()
	{
		// Kameranın gitmesi gereken pozisyon
		Vector3 hedefKonum = hedef.position - hedef.forward * uzaklik + Vector3.up * yukseklik;
		
		// Player sahneden aşağı düşse de kamera belli bir y değerinin altına inmesin
		if( hedefKonum.y < yukseklik )
			hedefKonum.y = yukseklik;
			
		// Kamerayı hedefKonum'a doğru "yumuşak" bir şekilde hareket ettir
		transform.position = Vector3.Slerp( transform.position, hedefKonum, 0.1f );
		
		// Kamerayı "yumuşak" bir şekilde hedef'e doğru döndür
		Quaternion rot = Quaternion.LookRotation( hedef.position - tr.position );
		tr.rotation = Quaternion.Slerp(tr.rotation, rot, 0.1f );
	}


}
