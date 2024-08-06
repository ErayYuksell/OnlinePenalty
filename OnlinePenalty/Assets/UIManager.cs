using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance;
    [SerializeField] GameObject goalCanvas;
    [SerializeField] GameObject failCanvas;
    [SerializeField] GameObject resultCanvas;
    [SerializeField] GameObject ShootControllCanvas;
    [SerializeField] GameObject GoalkeeperControllCanvas;
    [SerializeField] Button shootButton;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void OpenGoalCanvas()
    {
        StartCoroutine(IEOpenGoalCanvas());
    }
    public void OpenFailCanvas()
    {
        StartCoroutine(IEOpenFailCanvas());
    }
    IEnumerator IEOpenGoalCanvas()
    {
        ShootControllCanvas.SetActive(false);
        resultCanvas.SetActive(false);
        goalCanvas.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Game");
    }
    IEnumerator IEOpenFailCanvas()
    {
        ShootControllCanvas.SetActive(false);
        resultCanvas.SetActive(false);
        failCanvas.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Game");
    }
    public void CloseShootButton()
    {
        shootButton.interactable = false;
    }

}
