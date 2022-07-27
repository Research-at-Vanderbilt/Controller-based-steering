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
    public int Which_Handed_Person = 1; // For Right Hand as a Dominant Hand Only

    // Update is called once per frame
    void Update()
    {
        if (Which_Handed_Person == 1)
        {
            if (updateStopper == false)
            {
                MSC.initialCheck = true;
                updateStopper = true;
            }
            Other_Controller_Activated = Check_If_Activated(LeftController); //Recalibration with primary button of the non-dominant hand
        }
        else
        {
            if (updateStopper == false)
            {
                MSC.initialCheck = true;
                updateStopper = true;
            }
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

    public bool Check_If_Activated(XRController controller)
    {
        InputHelpers.IsPressed(controller.inputDevice, PrimaryButtonOfTheInactiveHand, out bool isActivated, activationThresold);
        return isActivated;
    }
}
