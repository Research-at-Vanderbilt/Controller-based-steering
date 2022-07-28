using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Game_Controller : MonoBehaviour
{
    public Modified_Steering_Controller MSC;
    public XRController LeftController, RightController;
    public InputHelpers.Button PrimaryButtonOfTheInactiveHand;
    public float activationThresold = 0.1f;
    private bool updateStopper;
    [HideInInspector]
    public bool Other_Controller_Activated;
    [HideInInspector]
    public bool Calibration_Done;
    [HideInInspector]
    public int Which_Handed_Person;
    public GameObject Canvas, EventSystem, Objects;

    // Start is called before the first frame update
    void Start()
    {
        Objects.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Which_Handed_Person == 1)
        {
            InitialSetupper();
            Other_Controller_Activated = Check_If_Activated(LeftController); //Recalibration with primary button of the non-dominant hand
        }
        else
        {
            InitialSetupper();
            Other_Controller_Activated = Check_If_Activated(RightController); //Recalibration with primary button of the non-dominant hand
        }

        if (Other_Controller_Activated == true || Input.GetKey(KeyCode.Tab) /*This is for testing purposes only*/)
        {
            if (Calibration_Done == false)
            {
                MSC.Controller_Calibrate();
                Calibration_Done = true;
            }
        }
        else
            Calibration_Done = false;
    }

    private void InitialSetupper()
    {
        if (updateStopper == false)
        {
            MSC.initialCheck = true;
            updateStopper = true;
        }
    }

    private void PostButtonClicked()
    {
        LeftController.gameObject.GetComponent<XRRayInteractor>().enabled = false;
        LeftController.gameObject.GetComponent<LineRenderer>().enabled = false;
        LeftController.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        RightController.gameObject.GetComponent<XRRayInteractor>().enabled = false;
        RightController.gameObject.GetComponent<LineRenderer>().enabled = false;
        RightController.gameObject.GetComponent<XRInteractorLineVisual>().enabled = false;
        Destroy(Canvas);
        Destroy(EventSystem);
        Objects.SetActive(true);
    }

    public bool Check_If_Activated(XRController controller)
    {
        InputHelpers.IsPressed(controller.inputDevice, PrimaryButtonOfTheInactiveHand, out bool isActivated, activationThresold);
        return isActivated;
    }

    public void RightHandButtonClicked()
    {
        Which_Handed_Person = 1;
        PostButtonClicked();
    }

    public void LeftHandButtonClicked()
    {
        Which_Handed_Person = 2;
        PostButtonClicked();
    }
}
