using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : MonoBehaviour
{    
    public string id; 
    public enum InteractionType { NONE, PickUp, Examine,GrabDrop }
    public enum ItemType { Staic, Consumables}
    [Header("Attributes")]
    public InteractionType interactType;
    public ItemType type;
    [Header("Exmaine")]
    public string descriptionText;
    [Header("Custom Events")]
    public UnityEvent customEvent;
    public UnityEvent consumeEvent;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.layer = 10;
    }

    public void Interact()
    {
        switch(interactType)
        {
            case InteractionType.PickUp:
                //Add the object to the PickedUpItems list
                FindObjectOfType<InventorySystem>().PickUp(gameObject);
                //Disable
                gameObject.SetActive(false);
                // if (CompareTag("NextLevel"))
                // {
                //     SceneManager.LoadScene("Scene3 1");
                // }
                // ... inside the Interact() method
                if (CompareTag("NextLevel"))
                {
                    string currentScene = SceneManager.GetActiveScene().name;
                    // Match the number at the end of the scene name
                    Match match = Regex.Match(currentScene, @"(\d+)$");
                    if (match.Success)
                    {
                        int sceneNumber = int.Parse(match.Value);
                        string nextScene = currentScene.Substring(0, match.Index) + (sceneNumber + 1);
                        SceneManager.LoadScene(nextScene);
                    }
                    else
                    {
                        Debug.LogWarning("Current scene name does not end with a number.");
                    }
                }
                break;
            case InteractionType.Examine:
                //Call the Examine item in the interaction system
                FindObjectOfType<InteractionSystem>().ExamineItem(this);                
                break;
            case InteractionType.GrabDrop:
                //Grab interaction
                FindObjectOfType<InteractionSystem>().GrabDrop();
                break;
            default:
                Debug.Log("NULL ITEM");
                break;
        }

        //Invoke (call) the custom event(s)
        customEvent.Invoke();
    }
}
