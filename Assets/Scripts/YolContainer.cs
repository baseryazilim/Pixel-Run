using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Düz bir yolun içeriğini depolayan script
// Düz bir yolda genelde birden çok zemin prefab'ının klonları ve
// birden çok puan objesi klonu yer alır
// Örneğin oyunun en başında elimizde düz bir yol, yolun ucunda iki yönlü
// bir kavşak ve kavşağın iki ucundan çıkan iki ayrı düz yol daha olsun.
// Bu durumda elimizde toplam 3 adet YolContainer objesi olmuş olur.
public class YolContainer
{
	// Bu yolda yer alan tüm zemin objelerini depolayan array (dizi)
	private Transform[] yol;
	// Bu zemin objelerinin sırayla hangi prefab'ın klonu olduğunu belirleyen
	// ikinci bir array
	private int[] yolIndexler;
	// Bu yolda yer alan tüm puan objelerini depolayan list
	private List<Transform> puanObjeleri;
	
	// yolun bitiş noktası
	private Vector3 bitisNoktasi;
	// yolun yönü (ileri yön, sağ yön, geri yön veya sol yön)
	private Vector3 yon;
	
	// Düz yolu oluşturmaya yarayan fonksiyon:
	// - Yolda kullanacağımız zeminleri ve puan objelerini çekmek için havuzu kullanıyoruz. 
	// - objeUzunluklar array'i her bir zemin prefab'ının kaç metre uzunluğunda olduğunu depolar. 
	// - baslangicNoktasi yolun hangi koordinattan başlayacağını belirler
	// - ileriYon yolun hangi yönde gideceğini belirler
	// - uzunluk yolun kaç zemin klonundan oluşacağını belirler
	public void YolOlustur( ObjeHavuzu havuz, float[] objeUzunluklar,
							Vector3 baslangicNoktasi, Vector3 ileriYon, int uzunluk )
	{
		yol = new Transform[ uzunluk ];
		yolIndexler = new int[ uzunluk ];
		puanObjeleri = new List<Transform>();
		
		// yola dizeceğimiz zemin objelerinin sahip olacağı rotation'ı bulup egim'de depoluyoruz
		Vector3 egim = SonsuzYolScript.EgimiBul( ileriYon );
	
		for( int i = 0; i < uzunluk; i++ )
		{
			// Rastgele bir zemin prefab'ı seçiyor ve o prefab'ın bir klonunu havuzdan çekiyoruz
			int randomIndex = Random.Range( 0, objeUzunluklar.Length );
			Transform obje = havuz.HavuzdanYolObjesiCek( randomIndex );
			
			// bu zemin klonunun konumunu ve eğimini ayarlıyor, ardından zemini aktif hale getiriyoruz
			obje.position = baslangicNoktasi;
			obje.eulerAngles = egim;
			obje.gameObject.SetActive( true );
			
			// zemini ve zeminin çıktığı prefab'ın index'ini ilgili array'lerimizde depoluyoruz
			yol[i] = obje;
			yolIndexler[i] = randomIndex;
		
			// bir sonraki zemin klonunu bu zeminin uzunluğu kadar ileride oluşturuyoruz ki
			// sonraki iki zemin objesi üst üste binmesin
			baslangicNoktasi += ileriYon * objeUzunluklar[randomIndex];
		}
		
		// yolun bitiş noktasını ve yönünü ayarlıyoruz
		bitisNoktasi = baslangicNoktasi;
		yon = ileriYon;
	}
	
	// Düz yola puan objeleri dizmeye yarayan fonksiyon:
	// - puan objelerini havuzdan (pool) çekiyoruz
	// - artarda kaç puan objesi dizileceği bilgisini birDizidekiPuanObjesiSayisi vasıtasıyla dışarıdan alıyoruz
	public void YolaPuanObjeleriDiz( ObjeHavuzu havuz, int birDizidekiPuanObjesiSayisi )
	{
		// üzerinde çalıştığımız diziye dizilmiş puan objesi sayısını sıfırlıyoruz
		int diziliPuanObjesi = 0;
		// iki dizi arasında kaç zeminlik aralık olacağını belirliyoruz
		// (aslında bu değeri de dışarıdan parametre olarak almam uygun olurdu)
		int puanObjeleriArasiAralik = 1;
		
		// 0- sol
		// 1- sağ
		// puan objelerinin yolun hangi tarafına dizileceğini belirleyen değişken
		int yon = Random.Range( 0, 2 );
		
		for( int i = 0; i < yol.Length; i++ )
		{
			int puanSpawnNoktasiSayisi;
			Transform parentObje;
			
			// yolun puan objesi dizilebilecek noktalarını (spawn noktalarını) alıyoruz
			if( yon == 0 )
			{
				parentObje = yol[i].Find( "PuanSpawnPointSol" );
			}
			else
			{
				parentObje = yol[i].Find( "PuanSpawnPointSag" );
			}
			
			// bu zeminde yer alan puan spawn noktası sayısını değişkende tutuyoruz
			puanSpawnNoktasiSayisi = parentObje.childCount;
			
			// Spawn noktalarının sonuna ulaşmadığımız ve bir dizi puan objesi dizmediğimiz müddetçe:
			for( int j = 0; j < puanSpawnNoktasiSayisi &&
				 diziliPuanObjesi < birDizidekiPuanObjesiSayisi; j++ )
			{
				// havuzdan (pool) bir puan objesi klonu çekiyoruz
				Transform obje = havuz.HavuzdanPuanObjesiCek();
				
				// puan objesini spawn noktasına konumlandırıyor ve aktif hale getiriyoruz
				obje.position = parentObje.GetChild( j ).position;
				obje.gameObject.SetActive( true );
				
				// puan objesini ilgili list'e ekliyoruz
				puanObjeleri.Add( obje );
				
				// bu dizideki puan objesi sayısını 1 artırıyoruz
				diziliPuanObjesi++;
			}
			
			// eğer ki diziyi tamamlamışsak (artarda birDizidekiPuanObjesiSayisi kadar
			// puan objesi spawn etmişsek):
			if( diziliPuanObjesi == birDizidekiPuanObjesiSayisi )
			{
				// diziyi sıfırla, puanObjeleriArasiAralik kadar zemini atla ve
				// yeni dizideki puanların spawn olacağı yönü tekrar rastgele bir şekilde seç
				diziliPuanObjesi = 0;
				i += puanObjeleriArasiAralik;
				yon = Random.Range( 0, 2 );
			}
		}
	}
	
	// Eğer ki parametre olarak girilen puan objesi bu yolda yer alıyorsa
	// onu yoldan siliyoruz (player yoldaki bir puanı toplayınca bu fonksiyon çağrılır)
	public void PuanObjesiniHavuzaEkle( Transform obje )
	{
		if( puanObjeleri != null )
			puanObjeleri.Remove( obje );
	}
	
	// Bu yolu yok ederken (oyuncu artık bu yolda koşmayı bitirip başka yola saptığında)
	// çağrılan ve yoldaki puan objeleri ile zemin objelerini havuza geri eklemeye yarayan
	// fonksiyon (yani Destroy yapmıyoruz)
	public void YoluYokEt( ObjeHavuzu havuz )
	{
		if( yol != null )
		{
			// zemin objelerini deaktif et ve havuza ekle (bu esnada hangi zemin objesi klonunun
			// hangi prefab'tan çıktığını dikkate al (yolIndexler vasıtasıyla))
			for( int i = 0; i < yol.Length; i++ )
			{
				Transform obje = yol[i];
				obje.gameObject.SetActive( false );
				havuz.HavuzaYolObjesiEkle( yolIndexler[i], obje );
			}
			
			// puan objelerini deaktif et ve havuza ekle
			while( puanObjeleri.Count > 0 )
			{
				Transform obje = puanObjeleri[0];
				obje.gameObject.SetActive( false );
				puanObjeleri.RemoveAt( 0 );
				havuz.HavuzaPuanObjesiEkle( obje );
			}
			
			// yolu yok et
			yol = null;
		}
	}
	
	// yolun bitiş noktasını döndüren fonksiyon
	public Vector3 BitisNoktasi()
	{
		return bitisNoktasi;
	}
	
	// yolun yönünü döndüren fonksiyon
	public Vector3 Yon()
	{
		return yon;
	}
	
	// yolun ileri yönünün doğusuna denk gelen yönü
	// döndüren fonksiyon
	public Vector3 SagYon()
	{
		if( yon == Vector3.forward )
			return Vector3.right;
			
		if( yon == Vector3.right )
			return Vector3.back;
			
		if( yon == Vector3.back )
			return Vector3.left;
			
		return Vector3.forward;
	}
	
	// yolun kaç zeminden oluştuğunu döndüren fonksiyon
	public int Uzunluk()
	{
		if( yol == null )
			return 0;
		
		return yol.Length;
	}
}
