using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject[] canvases;

    [Header(" Home Screen ")]
    [SerializeField] private Button noviceBtn, expertBtn;

    private int canvasIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        MenuChange(0); // Menu Page

        noviceBtn.onClick.AddListener(

            () => { MenuChange(1); CustomEvents.EnableNoviceView?.Invoke(); }
        );

        expertBtn.onClick.AddListener(

            () => { MenuChange(1); CustomEvents.EnableExpertView?.Invoke(); }
        );
    }

    private void MenuChange(int val)
    {
        canvases[canvasIndex].SetActive(false);
        canvases[val].SetActive(true);

        canvasIndex = val;

        switch (val)
        {
            case 0:
                CustomEvents.CanvasChanged?.Invoke(Canvases.HomeCanvas);
                break;

            case 1:
                CustomEvents.CanvasChanged?.Invoke(Canvases.VideoCanvas);
                break;

            default:
                break;
        }
    }
}

public enum Canvases { HomeCanvas, VideoCanvas }