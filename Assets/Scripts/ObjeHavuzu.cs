using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* 
 * Bu örnekte pooling adı verilen bir teknikten (pattern) faydalanıyoruz.
 * Bu pattern'in hedefi oldukça basit: eğer ki bir objeyi oyun boyunca
 * defalarca kez Instantiate ve Destroy ediyorsak bunun yerine o objeyi
 * Destroy etmiyor ama havuz adı verilen bir yerde depoluyoruz ve
 * o objeye tekrar ihtiyacımız olduğunda objeyi direkt havuzdan çekiyoruz,
 * yani Instantiate ile uğraşmıyoruz.
 *
 * Bu pattern bir infinite runner oyunu için kritik öneme sahip çünkü
 * bu tür oyunlarda yol prosedürel olarak oluşturuluyor, sonsuza kadar gidiyor
 * ama genel olarak aynı obje oyun boyunca defalarca kez kullanılıyor. Eğer ki
 * yeni yol oluşturma işlemini Instantiate ve Destroy'lar ile yapsaydık tek bir
 * seferde belki onlarca, belki yüzlerce objenin Instantiate ve(ya) Destroy edilmesi
 * gerekecekti ve bu da FPS'te o an ciddi bir düşüşe, belki de bir iki saniyelik ciddi
 * takılmalara sebep olacaktı.
*/

// Infinite yolumuzda defalarca kullandığımız objeleri depolayan havuz scripti
public class ObjeHavuzu : MonoBehaviour
{
	// Zemin prefab'larını depolayan array
	// (Bazen havuzda bir objeden yeterli sayıda olmaz ve bu durumda
	// mecburen o objeyi Instantiate edip havuza eklemek zorunda kalırız)
	private GameObject[] ileriYolObjeleri;
	
	// Puan prefab'ını depolayan değişken
	private GameObject puanObjesi;
	
	// - HAVUZ -
	// Zemin objelerini depolayan havuz (ileriYolObjeleriHavuzu)
	// Bu değişkenin türüne dikkat edin: List<Transform>[]
	// Yani bu bir List<Transform> arrayi 
	// List<Transform> dediğimiz şey Transform türündeki bir List'tir
	// List veri türü array gibi içerisinde birden çok veriyi depolamaya yarar
	// List'in array'den en önemli farkı List'in boyutunun (size) belli olmamasıdır
	// Yani List'e istediğimiz sayıda obje atabiliriz ama bunu array'de yapamayız
	// List<Transform> veri türümüz belli bir zemin prefab'ından oluşturulmuş tüm
	// klonları havuzda tutmaya yararken içerisinde List<Transform> barındıran array
	// ise her bir zemin prefab'ı için ayrı birer havuz olmasını sağlamakta. Bu sayede
	// her prefab'ın klonları ayrı havuzlarda tutulmakta ve iki farklı klon aynı havuza
	// karışmamakta. Böylece de hangi zemin türünden bir objeye ihtiyaç duyuyorsak
	// array'in o index'indeki havuza (List<Transform>) erişiyor ve ondan objemizi çekiyoruz
	private List<Transform>[] ileriYolObjeleriHavuzu;
	
	// Sadece sola dönmeye yarayan kavşak objesi
	private Transform solDonemec;
	
	// Sadece sağa dönmeye yarayan kavşak objesi
	private Transform sagDonemec;
	
	// Hem sola hem de sağa dönmeye yarayan (gidilecek yönü oyuncu seçer) kavşak objesi
	private Transform solVeSagDonemec;
	
	// DİKKAT: Kavşak objelerinden havuzda sadece birer tane tutulmakta çünkü
	// oyun esnasında aynı anda birden çok yol objesi görmemiz mümkün iken aynı anda
	// birden çok kavşak görmemiz gibi bir durum söz konusu değil
	
	// Puan objelerini depolayan havuz
	// Farkettiyseniz bunun da türü List<Transform> ama bu aynı zamanda bir 
	// array değil (ileriYolObjeleriHavuzu'nun aksine) çünkü oyunda sadece tek bir çeşit
	// puan prefab'ı bulunmakta
	private List<Transform> puanObjeleriHavuzu;
	// - HAVUZ -
	
	// Oyunun en başında havuzları hatırı sayılır miktarda objelerle doldurmaya yarayan fonksiyon
	public void HavuzuDoldur( GameObject[] ileriYolObjeleri, GameObject solaDonus, 
							  GameObject sagaDonus, GameObject ikiYoneDonus,
							  GameObject puanObjesiPrefab, int size, 
							  int ardArdaDiziliPuanObjesiSayisi )
	{
		this.ileriYolObjeleri = ileriYolObjeleri;
		puanObjesi = puanObjesiPrefab;
		
		// Fonksiyonun geri kalan kısmı hep havuzu (pool) doldurmakla alakalı
		Vector3 pos = Vector3.zero;
		Quaternion egim = Quaternion.identity;
		GameObject obje;
		
		obje = Instantiate( solaDonus, pos, egim ) as GameObject;
		obje.SetActive( false );
		solDonemec = obje.transform;
		
		obje = Instantiate( sagaDonus, pos, egim ) as GameObject;
		obje.SetActive( false );
		sagDonemec = obje.transform;
		
		obje = Instantiate( ikiYoneDonus, pos, egim ) as GameObject;
		obje.SetActive( false );
		solVeSagDonemec = obje.transform;
		
		ileriYolObjeleriHavuzu = new List<Transform>[ileriYolObjeleri.Length];
		puanObjeleriHavuzu = new List<Transform>();
		
		for( int i = 0; i < ileriYolObjeleri.Length; i++ )
		{
			ileriYolObjeleriHavuzu[i] = new List<Transform>();
			
			for( int j = 0; j < size; j++ )
			{
				obje = Instantiate( ileriYolObjeleri[i], pos, egim ) as GameObject;
				obje.SetActive( false );
				ileriYolObjeleriHavuzu[i].Add( obje.transform );
			}
		}
		
		int olusturulacakPuanObjesiSayisi = (int)( ardArdaDiziliPuanObjesiSayisi * size * 2.5f );
		for( int i = 0; i < olusturulacakPuanObjesiSayisi; i++ )
		{
			obje = Instantiate( puanObjesiPrefab, pos, egim ) as GameObject;
			obje.SetActive( false );
			puanObjeleriHavuzu.Add( obje.transform );
		}
		// Havuzu doldurduk!
	}
	
	// Bir yol objesini havuza eklemeye yarayan fonksiyon
	// Buradaki index, yol objesinin hangi yol prefab'ının klonu olduğunu belirtiyor
	public void HavuzaYolObjesiEkle( int index, Transform obje )
	{
		// Bir List'e yeni bir eleman eklerken Add fonksiyonu kullanılır
		ileriYolObjeleriHavuzu[index].Add( obje );
	}

	// Bir puan objesini havuza eklemeye yarayan fonksiyon
	public void HavuzaPuanObjesiEkle( Transform obje )
	{
		// Bir List'e yeni bir eleman eklerken Add fonksiyonu kullanılır
		puanObjeleriHavuzu.Add( obje );
	}
	
	// Havuzdan bir yol objesi klonu almaya yarayan fonksiyon
	// Buradaki index, hangi yol prefab'ının bir klonunu çekmek istediğimizi belirtiyor
	public Transform HavuzdanYolObjesiCek( int index )
	{
		Transform obje;
		
		if( ileriYolObjeleriHavuzu[index].Count <= 0 )
		{
			// Eğer ki o prefab için olan havuz boşsa o prefab'dan yeni bir
			// klon oluştur ve onu döndür
			// print( "Yol havuzuna şu indexten yeni obje eklendi: " + index );
			obje = ( Instantiate( ileriYolObjeleri[index], Vector3.zero, Quaternion.identity ) as GameObject ).transform;
		}
		else
		{
			// Eğer ki o prefab için olan havuz boş değilse, o havuzun
			// ilk elemanını döndür ve o elemanı havuzdan sil
			// Neden çektiğimiz elemanı List'ten (havuz) siliyoruz? Çünkü
			// eğer elemanı silmezsek havuzdan yeni bir eleman çekince yine
			// aynı objeyi döndürmüş oluruz ve bu da o yol objesinin aniden tekrar
			// konumlandırılması anlamına gelir. Biz bunu istemiyoruz. Biz o yol
			// objesine tekrar erişmenin ancak o yol objesiyle işimiz bittiğinde
			// gerçekleşebilmesini istiyoruz
			obje = ileriYolObjeleriHavuzu[index][0];
			ileriYolObjeleriHavuzu[index].RemoveAt(0);
		}
		
		return obje;
	}
	
	// Havuzdan bir puan objesi klonu almaya yarayan fonksiyon
	public Transform HavuzdanPuanObjesiCek()
	{
		Transform obje;
		
		if( puanObjeleriHavuzu.Count <= 0 )
		{
			// eğer ki puan objesi havuzu boşsa yeni bir puan objesi 
			// Instantiate et ve onu döndür
			// print( "Puan havuzuna yeni obje eklendi." );
			obje = ( Instantiate( puanObjesi, Vector3.zero, Quaternion.identity ) as GameObject ).transform;
		}
		else
		{
			// eğer ki puan objesi havuzu boş değilse havuzdaki 
			// ilk elemanı döndür ve o elemanı havuzdan çıkar
			obje = puanObjeleriHavuzu[0];
			puanObjeleriHavuzu.RemoveAt(0);
		}
		
		return obje;
	}
	
	// sol kavşak objesini döndür
	public Transform SolDonemec()
	{
		return solDonemec;
	}
	
	// sağ kavşak objesini döndür
	public Transform SagDonemec()
	{
		return sagDonemec;
	}
	
	// iki yönlü kavşak objesini döndür
	public Transform SolVeSagDonemec()
	{
		return solVeSagDonemec;
	}
}
