using UnityEngine;

// static bir değişken olan destroy boolean'ı true olduğunda
// bu scripte sahip objeler 4 saniye içinde yok edilir
// (oyunun başında scene'de yer alan BaslangicZemin
// objelerini yok etmek için kullanıyoruz)
public class DestroyMe : MonoBehaviour 
{
	public static bool destroy;
	private bool yokEt = false;
	
	void Start()
	{
		// scene açıldığında destroy'u false yap
		destroy = false;
	}
	
	void Update()
	{
		// destroy true olduğu andan 4 saniye sonra scripte sahip
		// olan objeyi yok et
		if( destroy && !yokEt )
		{
			yokEt = true;
			Destroy( transform.gameObject, 4f );
		}
	}
}
