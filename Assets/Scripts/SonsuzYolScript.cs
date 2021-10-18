using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Infinite road yapmayı görev edinmiş kahraman script!
// Oyunun en başında ufak bir yol rastgele bir şekilde oluşturulur 
// ve karakter kavşaklardan sağa/sola saptıkça yolun devamı bu
// script vasıtasıyla oluşturulmaya devam edilir. Böylece infinite road
// atmosferi oluşturulur oysa Scene panelinden bakılacak olursa oyun
// ilerledikçe eski yolun yok olup da yeni yolun sonradan oluşturulduğu
// kolayca gözlemlenebilir
// Bu script düzgün çalışmak için ObjeHavuzu scriptine (havuz)(pool) ihtiyaç
// duyar (RequireComponent)
[RequireComponent( typeof( ObjeHavuzu ) )]
public class SonsuzYolScript : MonoBehaviour
{
	// Düz bir yolun sahip olacağı minimum zemin objesi sayısı
	public int yolMinimumUzunluk = 10;
	// Düz bir yolun sahip olacağı maksimum zemin objesi sayısı
	public int yolMaksimumUzunluk = 20;
	// Puan objeleri yolda birbiri ardına dizilirler (dizi şeklinde) ve bu
	// değişken bir dizinin sahip olacağı puan objesi sayısını depolar
	public int ardArdaDiziliPuanObjesiSayisi = 6;
	
	// Puan objesi prefab'ını depolayan değişken
	public GameObject puanPrefab;

	// Zemin prefab'larını depolayan array
	public GameObject[] ileriYolObjeleri;
	// Her bir zemin prefab'ının kaç metre uzunluğunda olduğunu
	// depolayan array
	public float[] ileriYolObjeleriUzunluklar;
	
	// Sağa dönmeye yarayan kavşak prefab'ı
	public GameObject sagaDonus;
	// Bu kavşak prefab'ının metre cinsinden ebatları
	public Vector3 sagaDonusEbatlar;
	
	// Sola dönmeye yarayan kavşak prefab'ı
	public GameObject solaDonus;
	// Bu kavşak prefab'ının metre cinsinden ebatları
	public Vector3 solaDonusEbatlar;
	
	// Sağa veya sola dönmeye yarayan kavşak prefab'ı
	public GameObject ikiYoneDonus;
	// Bu kavşak prefab'ının metre cinsinden ebatları
	public Vector3 ikiYoneDonusEbatlar;
	
	// karakterin üzerinde koşmakta olduğu yol (ileri yöndeki yol)
	private YolContainer ileriYol;
	// ileriYol'un ucundaki kavşaktan sağa sapınca gireceğimiz yol
	private YolContainer sagYol;
	// ileriYol'un ucundaki kavşaktan sola sapınca gireceğimiz yol
	private YolContainer solYol;
	
	// Havuz objesi
	private ObjeHavuzu havuz;
	
	/* Sistemi elle test etmek istersen:
	 * 1- Oyunu başlat
	 * 2- Menüyü kapatmadan Scene paneline geçiş yap
	 * 3- Inspector'dan Main Camera'yı seç
	 * 4- SonsuzYolScript component'indeki bu sol ve sag 
	 * değişkenlerin tekini işaretleyip 2 saniye bekle
	 * 5- Eğer düzgün bir yön seçtiysen, yol
	 * o yönde kendini güncelleyecektir
	 */
	public bool sol = false;
	public bool sag = false;
	
	void Start() 
	{
		if( ileriYolObjeleri.Length != ileriYolObjeleriUzunluklar.Length )
		{
			// Eğer her bir zemin prefab'ının uzunluğu tek tek girilmemişse hata mesajı verip
			// component'i objeden at (yani oyunu oynanmaz hale getir)
			Debug.LogError( "HATA: SonsuzYolScript'teki Duz Yol Objeleri ile " +
							"Duz Yol Objeleri Uzunluklar'ın boyutu (size) " +
							"aynı olmak zorundadır." );
							
			Destroy( this );
		}
		else
		{
			// Havuzu (pool) doldur
			havuz = GetComponent<ObjeHavuzu>();
			
			int havuzdakiObjeSayisi = yolMaksimumUzunluk / 3;
			havuz.HavuzuDoldur( ileriYolObjeleri, solaDonus, sagaDonus, 
								ikiYoneDonus, puanPrefab, havuzdakiObjeSayisi, 
								ardArdaDiziliPuanObjesiSayisi );
			// Havuzu doldurduk!
			
			// Rastgele bir yol oluştur
			ileriYol = new YolContainer();
			sagYol = new YolContainer();
			solYol = new YolContainer();
			
			YolOlustur( ileriYol, Vector3.forward * 15, Vector3.forward );
			DonemecOlustur( ileriYol.BitisNoktasi(), ileriYol.Yon() );
		}
	}
	
	// Random bir şekilde düz bir yol oluşturmaya yarayan fonksiyon
	void YolOlustur( YolContainer c, Vector3 baslangicNoktasi, Vector3 ileriYon )
	{
		// yolun uzunluğunu rastgele olarak belirle
		int yolUzunluk = Random.Range( yolMinimumUzunluk, yolMaksimumUzunluk + 1 );
		
		// yolu oluştur (yola zeminleri dik)
		c.YolOlustur( havuz, ileriYolObjeleriUzunluklar, baslangicNoktasi,
					  ileriYon, yolUzunluk );
		// yola puan objelerini diz
		c.YolaPuanObjeleriDiz( havuz, ardArdaDiziliPuanObjesiSayisi );
	}
	
	// Kavşak ve bu kavşağa bağlı düz yollar oluşturmaya yarayan fonksiyon
	void DonemecOlustur( Vector3 baslangicNoktasi, Vector3 ileriYon )
	{
		// Kavşak objesinin sahip olacağı rotation değerini bul
		Vector3 egim = SonsuzYolScript.EgimiBul( ileriYon );
		
		// [0-2] aralığında rastgele bir integer döndürülür ve:
		// 0- sol kavşak oluşturulur
		// 1- sağ kavşak oluşturulur
		// 2- iki yönlü kavşak oluşturulur
		switch( Random.Range( 0, 3 ) )
		{
			case 0:
				// sadece sola dönemeç oluştur
				// sol kavşak objesini havuzdan çek ve oyun alanına yerleştir
				Transform solaDonusObjesi = havuz.SolDonemec();
				solaDonusObjesi.position = baslangicNoktasi;
				solaDonusObjesi.eulerAngles = egim;
				solaDonusObjesi.gameObject.SetActive( true );
				
				// sola dön
				if( ileriYon == Vector3.forward )
				{
					ileriYon = Vector3.left;
					baslangicNoktasi += new Vector3( -solaDonusEbatlar.x / 2, 0, solaDonusEbatlar.z / 2 );
				}
				else if( ileriYon == Vector3.left )
				{
					ileriYon = Vector3.back;
					baslangicNoktasi += new Vector3( -solaDonusEbatlar.z / 2, 0, -solaDonusEbatlar.x / 2 );
				}
				else if( ileriYon == Vector3.back )
				{
					ileriYon = Vector3.right;
					baslangicNoktasi += new Vector3( solaDonusEbatlar.x / 2, 0, -solaDonusEbatlar.z / 2 );
				}
				else
				{
					ileriYon = Vector3.forward;
					baslangicNoktasi += new Vector3( solaDonusEbatlar.z / 2, 0, solaDonusEbatlar.x / 2 );
				}
				
				// kavşağın ucunda yeni bir düz yol oluştur
				YolOlustur( solYol, baslangicNoktasi, ileriYon );
				break;
			case 1:
				// sadece sağa dönemeç oluştur
				// sağ kavşak objesini havuzdan çek ve oyun alanına yerleştir
				Transform sagaDonusObjesi = havuz.SagDonemec();
				sagaDonusObjesi.position = baslangicNoktasi;
				sagaDonusObjesi.eulerAngles = egim;
				sagaDonusObjesi.gameObject.SetActive( true );
				
				// sağa dön
				if( ileriYon == Vector3.forward )
				{
					ileriYon = Vector3.right;
					baslangicNoktasi += new Vector3( sagaDonusEbatlar.x / 2, 0, sagaDonusEbatlar.z / 2 );
				}
				else if( ileriYon == Vector3.left )
				{
					ileriYon = Vector3.forward;
					baslangicNoktasi += new Vector3( -sagaDonusEbatlar.z / 2, 0, sagaDonusEbatlar.x / 2 );
				}
				else if( ileriYon == Vector3.back )
				{
					ileriYon = Vector3.left;
					baslangicNoktasi += new Vector3( -sagaDonusEbatlar.x / 2, 0, -sagaDonusEbatlar.z / 2 );
				}
				else
				{
					ileriYon = Vector3.back;
					baslangicNoktasi += new Vector3( sagaDonusEbatlar.z / 2, 0, -sagaDonusEbatlar.x / 2 );
				}
				
				// kavşağın ucunda yeni bir düz yol oluştur
				YolOlustur( sagYol, baslangicNoktasi, ileriYon );
				break;
			case 2:
				// hem sola hem sağa dönemeç oluştur
				// iki yönlü kavşak objesini havuzdan çek ve oyun alanına yerleştir
				Transform ikiYoneDonusObjesi = havuz.SolVeSagDonemec();
				ikiYoneDonusObjesi.position = baslangicNoktasi;
				ikiYoneDonusObjesi.eulerAngles = egim;
				ikiYoneDonusObjesi.gameObject.SetActive( true );
				
				// hem sol hem de sağ yönü birer değişkende depola
				Vector3 baslangicNoktasi2 = baslangicNoktasi;
				if( ileriYon == Vector3.forward )
				{
					ileriYon = Vector3.left;
					baslangicNoktasi += new Vector3( -ikiYoneDonusEbatlar.x / 2, 0, ikiYoneDonusEbatlar.z / 2 );
					baslangicNoktasi2 += new Vector3( ikiYoneDonusEbatlar.x / 2, 0, ikiYoneDonusEbatlar.z / 2 );
				}
				else if( ileriYon == Vector3.left )
				{
					ileriYon = Vector3.back;
					baslangicNoktasi += new Vector3( -ikiYoneDonusEbatlar.z / 2, 0, -ikiYoneDonusEbatlar.x / 2 );
					baslangicNoktasi2 += new Vector3( -ikiYoneDonusEbatlar.z / 2, 0, ikiYoneDonusEbatlar.x / 2 );
				}
				else if( ileriYon == Vector3.back )
				{
					ileriYon = Vector3.right;
					baslangicNoktasi += new Vector3( ikiYoneDonusEbatlar.x / 2, 0, -ikiYoneDonusEbatlar.z / 2 );
					baslangicNoktasi2 += new Vector3( -ikiYoneDonusEbatlar.x / 2, 0, -ikiYoneDonusEbatlar.z / 2 );
				}
				else
				{
					ileriYon = Vector3.forward;
					baslangicNoktasi += new Vector3( ikiYoneDonusEbatlar.z / 2, 0, ikiYoneDonusEbatlar.x / 2 );
					baslangicNoktasi2 += new Vector3( ikiYoneDonusEbatlar.z / 2, 0, -ikiYoneDonusEbatlar.x / 2 );
				}
				
				// kavşağın iki ucunda da yeni birer düz yol oluştur
				YolOlustur( solYol, baslangicNoktasi, ileriYon );
				YolOlustur( sagYol, baslangicNoktasi2, -ileriYon );
				break;
		}
	}
	
	void Update()
	{
		if( sol )
		{
			// Eğer ileriYol'un ucundaki kavşaktan sola dönme talimatı verilmişse:
			sol = false;
			
			// eğer elimizde bir solYol varsa (yani bu kavşaktan sola dönüş mümkünse):
			if( solYol.Uzunluk() > 0 )
			{
				// karakteri (player) 90 derece sola döndür
				Player player = GameObject.FindWithTag( "Player" ).GetComponent<Player>();
				player.transform.Rotate( Vector3.down * 90f );
				
				// player'ın yön değişkenlerini ayarla
				Vector3 bn = solYol.BitisNoktasi();
				Vector3 iy = solYol.Yon();
				player.ileriYon = iy;
				player.sagYon = solYol.SagYon();
				
				// player'ın yatay eksende gidebileceği minimum ve maksimum limitleri
				// ayarla (konseptle alakalı detaylı açıklama Player scriptinde mevcut)
				if( iy == Vector3.forward || iy == Vector3.back )
				{
					player.limitMinDeger = bn.x - 2.75f;
					player.limitMaxDeger = bn.x + 2.75f;
				}
				else
				{
					player.limitMinDeger = bn.z - 2.75f;
					player.limitMaxDeger = bn.z + 2.75f;
				}
				
				// sol yolu artık ileri yol (üzerinde koşulan yol) olarak ata
				YolContainer temp = ileriYol;
				ileriYol = solYol;
				solYol = temp;
				
				// yolun devamını oluştur
				StartCoroutine( YoluGuncelle() );
			}
		}
		else if( sag )
		{
			// Eğer ileriYol'un ucundaki kavşaktan sağa dönme talimatı verilmişse:
			sag = false;
			
			// eğer elimizde bir sagYol varsa (yani bu kavşaktan sağa dönüş mümkünse):
			if( sagYol.Uzunluk() > 0 )
			{
				// karakteri (player) 90 derece sağa döndür
				Player player = GameObject.FindWithTag( "Player" ).GetComponent<Player>();
				player.transform.Rotate( Vector3.up * 90f );
				
				// player'ın yön değişkenlerini ayarla
				Vector3 bn = sagYol.BitisNoktasi();
				Vector3 iy = sagYol.Yon();
				player.ileriYon = iy;
				player.sagYon = sagYol.SagYon();
				
				// player'ın yatay eksende gidebileceği minimum ve maksimum limitleri
				// ayarla (konseptle alakalı detaylı açıklama Player scriptinde mevcut)
				if( iy == Vector3.forward || iy == Vector3.back )
				{
					player.limitMinDeger = bn.x - 2.75f;
					player.limitMaxDeger = bn.x + 2.75f;
				}
				else
				{
					player.limitMinDeger = bn.z - 2.75f;
					player.limitMaxDeger = bn.z + 2.75f;
				}
				
				// sağ yolu artık ileri yol (üzerinde koşulan yol) olarak ata
				YolContainer temp = ileriYol;
				ileriYol = sagYol;
				sagYol = temp;
				
				// yolun devamını oluştur
				StartCoroutine( YoluGuncelle() );
			}
		}
	}
	
	// Mevcut yolun ucuna yeni bir kavşak koyup bu kavşağın ucundan
	// yeni düz yol(lar) çıkarmaya yarayan fonksiyon
	IEnumerator YoluGuncelle()
	{
		// 2 saniye bekle çünkü player kavşaktan tam döndüğü anda 
		// eski yolu yok edersek player büyük olasılıkla aşağı düşer
		yield return new WaitForSeconds( 2f );
		
		// arkada kalan yolları yok et (içeriklerini havuza yolla)
		solYol.YoluYokEt( havuz );
		sagYol.YoluYokEt( havuz );
		
		// az önce döndüğümüz kavşağı deaktif et (görünmez kıl)
		havuz.SolDonemec().gameObject.SetActive( false );
		havuz.SagDonemec().gameObject.SetActive( false );
		havuz.SolVeSagDonemec().gameObject.SetActive( false );
		
		// yolun ucuna yeni bir kavşak ve o kavşaktan çıkan yeni
		// yol(lar) oluştur
		DonemecOlustur( ileriYol.BitisNoktasi(), ileriYol.Yon() );
	}
	
	// bir puan objesini havuza (pool) yollamaya yarayan fonksiyon
	// normalde puan objeleri eski yol yok edilirken otomatik olarak havuza eklenir
	// ama player eğer ki bir puan objesini toplarsa o puan objesini havuza daha erken
	// (toplandığı anda) yollamamız gerekir elbette ki
	public void PuanObjesiniHavuzaYolla( Transform obje )
	{
		havuz.HavuzaPuanObjesiEkle( obje );
		
		// puan objesi hangi yoldaysa puan objesini o yoldan sil ki aynı obje
		// havuza iki kere eklenmesin (bir kere player toplayınca, bir kere de
		// yolu yokederken)
		ileriYol.PuanObjesiniHavuzaEkle( obje );
		sagYol.PuanObjesiniHavuzaEkle( obje );
		solYol.PuanObjesiniHavuzaEkle( obje );
	}
	
	// Girilen yönde dizilen zemin objelerinin sahip olması gereken eğimi
	// bulmaya yarayan fonksiyon
	public static Vector3 EgimiBul( Vector3 ileriYon )
	{
		if( ileriYon == Vector3.forward )
			return Vector3.zero;
		else if( ileriYon == Vector3.right )
			return Vector3.up * 90;
		else if( ileriYon == Vector3.left )
			return Vector3.up * 270;
		else if( ileriYon == Vector3.back )
			return Vector3.up * 180;
		else
		{
			// parametre olarak sadece ileri, geri, sağ veya sol yönleri
			// kabul et, bunların dışındaki yönlerde hata mesajı göster
			// (Player bu oyunda çapraz bir yönde hareket etmiyor)
			Debug.LogError( "HATA: EgimiBul() fonksiyonu bu satıra girmemeliydi!" );
			return Vector3.zero;
		}
	}
}
