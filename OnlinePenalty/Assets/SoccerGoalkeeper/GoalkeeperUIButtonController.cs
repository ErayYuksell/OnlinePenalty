using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OnlinePenalty
{
    public class GoalkeeperUIButtonController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        [SerializeField] GameObject goalkeeperAreaPanel;
        [SerializeField] RectTransform buttonRectTransform; // Butonun RectTransform'u
        [SerializeField] RectTransform sliderRectTransform; // Slider'ýn RectTransform'u
        [SerializeField] Transform yellowAreaParentTransform;

        private Vector2 initialButtonPosition;
        private Vector2 buttonStartPosition;

        bool isDrag = true;

        private void Start()
        {
            initialButtonPosition = buttonRectTransform.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                buttonStartPosition = buttonRectTransform.anchoredPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                Vector2 newPosition = buttonRectTransform.anchoredPosition + new Vector2(eventData.delta.x, 0);
                newPosition.x = Mathf.Clamp(newPosition.x, -sliderRectTransform.rect.width / 2, sliderRectTransform.rect.width / 2);
                buttonRectTransform.anchoredPosition = newPosition;

                if (GoalkeeperController.Instance != null)
                {
                    float rotationFactor = newPosition.x / (sliderRectTransform.rect.width / 2);
                    GoalkeeperController.Instance.RotateYellowArea(rotationFactor); // GoalKeeperController'daki RotateYellowArea fonksiyonunu çaðýr
                }
                else
                {
                    Debug.LogError("GoalKeeperController instance is null.");
                }
            }
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            goalkeeperAreaPanel.SetActive(false); // kaleci atlamadan paneli kapa 
            isDrag = false;
            buttonRectTransform.gameObject.GetComponent<Button>().interactable = false;

            if (MultiplayerController.Instance.IsPlayerControllingGoalkeeper())
            {
                MultiplayerController.Instance.IsPlayerControllingGoalkeeperButtonDone();
                Debug.Log("Goalkeeper Player tap to button");
            }
        }
    }

}
