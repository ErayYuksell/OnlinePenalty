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
    [SerializeField] GameObject resultMultiCanvas;
    [SerializeField] GameObject ShootControllCanvas;
    [SerializeField] GameObject GoalkeeperControllCanvas;
    [SerializeField] GameObject targetObj;
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
    #region End game panels
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
        yield return new WaitForSeconds(1);
        goalCanvas.SetActive(true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Game");
    }
    IEnumerator IEOpenFailCanvas()
    {
        ShootControllCanvas.SetActive(false);
        resultCanvas.SetActive(false);
        yield return new WaitForSeconds(1);
        failCanvas.SetActive(true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Game");
    }
    public void CloseShootButton()
    {
        shootButton.interactable = false;
    }
    #endregion

    #region Multiplayer
    public void Player1Panels()
    {
        ShootControllCanvas.SetActive(true);
        targetObj.SetActive(true);
        GoalkeeperControllCanvas.SetActive(false);
    }
    public void Player2Panels()
    {
        GoalkeeperControllCanvas.SetActive(true);
        ShootControllCanvas.SetActive(false);
        targetObj.SetActive(false);
    }

    public void MultiplayerResultCanvas()
    {
        resultCanvas.SetActive(false);
        resultMultiCanvas.SetActive(true);
    }
    #endregion
}
