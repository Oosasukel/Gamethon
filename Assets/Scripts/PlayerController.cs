using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float distanceToInteract = 5.0f;
    private GameManager gameManager;

    private Transform _selection;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager.gameOver) return;

        if (_selection != null)
        {
            var interactable = _selection.GetComponent<IInteractable>();
            if (interactable != null) interactable.HideTips();

            _selection = null;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform;
            if (selection.CompareTag("Interactable") && hit.distance <= distanceToInteract)
            {
                var interactable = selection.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (Input.GetKeyDown(KeyCode.E)) interactable.Buy();
                    else if (Input.GetKeyDown(KeyCode.Q)) interactable.Sell();

                    interactable.ShowTips();

                    _selection = selection;
                }
            }
        }
    }
}
