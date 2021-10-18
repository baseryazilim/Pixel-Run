using UnityEngine;

// oyun bitince beliren Game Over menüsünün
// yararlandığı script
public class GameOverMenu : MonoBehaviour 
{
	public SkorScript skorScript;
	public UnityEngine.UI.Text skorText;
	
	void Start()
	{
		// Game Over menüsünde gösterilecek skoru ve 
		// yüksekskoru SkorScript'ten çek
		skorText.text = "Skor : " + skorScript.SkoruAl() + "\nYüksekskor : " + PlayerPrefs.GetInt( "YuksekSkor" );
	}
	
	// Ana Menü butonuna tıklayınca yapılacaklar
	public void Restart()
	{
		// Bölüme restart at (ana menü bölümün en başında gösteriliyor)
		Application.LoadLevel( Application.loadedLevel );
	}
}
