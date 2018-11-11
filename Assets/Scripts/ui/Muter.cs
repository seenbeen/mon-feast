using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Muter : MonoBehaviour {
    [SerializeField]
    Sprite OnSprite;
    [SerializeField]
    Sprite OffSprite;
    Image muteButtonImage;
    public Color color = new Color(1,1,1,0);
    public bool isPaused = false;
    private void Start()
    {
        muteButtonImage = GetComponent<Image>();
        muteButtonImage.color = color;
        if (AudioListener.volume == 0.0f)
        {
            muteButtonImage.sprite = OffSprite;
        }
    }
    // Update is called once per frame
    void Update () {
        muteButtonImage.material.color = color;
		if (isPaused)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }
	}

    public void ToggleMute()
    {
        if (AudioListener.volume > 0)
        {
            AudioListener.volume = 0.0f;
            muteButtonImage.sprite = OffSprite;
        } else
        {
            AudioListener.volume = 1.0f;
            muteButtonImage.sprite = OnSprite;
        }
    }
}
