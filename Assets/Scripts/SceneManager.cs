using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneManager : MonoBehaviour
{
    private Vector2 touchOrigin = -Vector2.one;
    private float m_forwardSpeed = 3.0f;

    private AudioSource microphoneAudioSource = null;
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
        microphoneAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // get the player object, as we often do things with it...
        var player = GameObject.Find("Player");
        float deadZone = 0.15f; // used to change speed of the player.

        bool bStartSomething = false;
        bool bEndSomething = false;

        if (GvrController.AppButtonDown)
            bStartSomething = true;

        if (GvrController.AppButtonUp)
            bEndSomething = true;

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
            microphoneAudioSource.clip = Microphone.Start(null, true, 10, 16000);
            microphoneAudioSource.loop = true;
            if (Microphone.GetPosition(null) > 0)
                microphoneAudioSource.Play();
        }

        if (Microphone.IsRecording(null))
        {
            DisplayMessage("Recording");
        }

        if (bEndSomething)
        {
            int recordingPosition = Microphone.GetPosition(null);
            Microphone.End(null);
            DisplayMessage("Stopping recording");
            microphoneAudioSource.Play();
        }

        // rotate the cube.
        var theCube = GameObject.Find("Cube");
        theCube.transform.Rotate(Vector3.up, 10f * Time.deltaTime);

        // allow the player to move forward.
        if (GvrController.ClickButton)
        {
            // get the player and the camera to use for moving the player forward.
            var camera = GameObject.Find("Main Camera");

            // let's translate the camera forward
            player.transform.position += (camera.transform.rotation * Vector3.forward * Time.deltaTime) * m_forwardSpeed; // using deltatime means I'm moving at 1 meter/s

            // and keep the world canvas in scene.
            var mainTextCanvas = GameObject.Find("mainTextCanvas");
            var controllerPointer = GameObject.Find("GvrControllerPointer");

            mainTextCanvas.transform.position = controllerPointer.transform.position;
            mainTextCanvas.transform.position += (GvrController.Orientation * Vector3.forward) * 6;
            mainTextCanvas.transform.rotation = GvrController.Orientation;
        }

        // handle acceleration of the player.
        if (GvrController.IsTouching)
        {
            if (GvrController.TouchPos.y < .5 - deadZone)
            {
                // Should be accelerating
                m_forwardSpeed += 0.2f;
            }
            else if (GvrController.TouchPos.y > .5 + deadZone)
            {
                //Should be deaccelerating
                m_forwardSpeed -= 0.2f;
            }
        }

        // reset the speed when done moving.
        if (GvrController.TouchUp)
        {
            m_forwardSpeed = 3.0f;
        }

    }
}
