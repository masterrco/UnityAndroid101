using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CubeScript : MonoBehaviour
{
    private Vector2 touchOrigin = -Vector2.one;

    private AudioSource source = null;
    public void DisplayMessage(string message)
    {
        var theTextGameObject = GameObject.Find("txtMainData");
        UnityEngine.UI.Text theTextComponent = theTextGameObject.GetComponent<UnityEngine.UI.Text>();
        theTextComponent.text = message;

    }

    // Use this for initialization
    void Start()
    {
        DisplayMessage("Start Success");
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        bool bStartSomething = false;
        bool bEndSomething = false;
        if (Input.GetKeyDown(KeyCode.Space))
            bStartSomething = true;

        if (Input.GetKeyUp(KeyCode.Space))
            bEndSomething = true;

        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];
            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
                bStartSomething = true;
            }
            if (myTouch.phase == TouchPhase.Ended)
            {
                bEndSomething = true;
            }
        }

        if (bStartSomething)
        {
            DisplayMessage("Starting recording");
            source.clip = Microphone.Start(null, true, 10, 16000);
            source.loop = true;
            if (Microphone.GetPosition(null) > 0)
                source.Play();
        }

        if (Microphone.IsRecording(null))
        {
            DisplayMessage("Recording");
        }

        if (bEndSomething)
        {
            DisplayMessage("Stopping recording");
            Microphone.End(null);
        }
    }
}
