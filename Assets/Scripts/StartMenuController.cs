using UnityEngine;

public class StartMenuController : MonoBehaviour
{
   public void OnStartClick()
   {
       // Load the first level of the game
       UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
   }

   public void OnExitClick()
   {
       #if UNITY_EDITOR
         // If we are in the editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
       #endif 
         // If we are in a built application, quit the application
            Application.Quit();
   }
   
}
