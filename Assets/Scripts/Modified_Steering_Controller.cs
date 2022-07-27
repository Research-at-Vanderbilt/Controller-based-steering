using TMPro;
using UnityEngine;
using UnityEngine.XR;


public class Modified_Steering_Controller : MonoBehaviour
{
    //[HideInInspector]
    public float speed = 1;
    private CharacterController character;
    private Transform headRig, LeftHandRig, RightHandRig, LeftHandModel, RightHandModel;
    private float gravity = -9.81f;
    private float fallingSpeed;
    //[HideInInspector]
    public LayerMask groundLayer; // For ease of use, comment out the [HideInInspector] command -> save the script -> go back to Unity -> Let it load for the update ->
                                  // Then choose "Everything" from the dropdown menu of this variable in the inspector!
    private float additionalHeight = 0.2f;
    private float inputButtonValue;
    public Game_Controller GameController;
    private bool update_Stopper;
    private Quaternion Yaw = Quaternion.Euler(Vector3.zero);
    private float previous_y_value = 0, yRotHand = 0, value = 0;
    [HideInInspector]
    public bool initialCheck;
    private Vector3 direction;
    [HideInInspector]
    public bool Final_UI_Activated;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
        headRig = GameObject.Find("XR Origin/Camera Offset/Main Camera").transform;
        LeftHandRig = GameObject.Find("XR Origin/Camera Offset/LeftHand Controller").transform;
        RightHandRig = GameObject.Find("XR Origin/Camera Offset/RightHand Controller").transform;
        LeftHandModel = GameObject.Find(LeftHandRig.gameObject.name + "/Left Hand Presence").transform; // I used the hand representation or you can use any controller representation for this purpose.
                                                                                                        // The necessity of this is that, the model has a physical representation in the virtual environment.
                                                                                                        // However, it would be really hard to debug if you use the simple controller, because it is an empty game object,
                                                                                                        // and there is no way to see that gameobject through the Quest 2 when you are debugging the program!
        RightHandModel = GameObject.Find(RightHandRig.gameObject.name + "/Right Hand Presence").transform; // Same as above!
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Which_Handed_Person == 1) // This part is basically checking which is the active hand of the participant.
                                            // So, the other hand will be used as the body tracker. I used an integer variable in the Game Controller Script named
                                            // as Which_Handed_Person to do this checking. If it is 1, then the person is a right handed person, or else he/she is a left handed person.
                                            // So, in the first case, the body tracker would be left hand tracker, and for the second case, it would be the right hand tracker!
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            device.TryGetFeatureValue(CommonUsages.trigger, out inputButtonValue);
            yRotHand = LeftHandModel.eulerAngles.y;
        }
        else if (GameController.Which_Handed_Person == 2)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            device.TryGetFeatureValue(CommonUsages.trigger, out inputButtonValue);
            yRotHand = RightHandModel.eulerAngles.y;
        }

        if (initialCheck == true) // From another script, this is the first boolean variable that you need to call to start this code properly.
                                    // You need to set the value of this variable from false to true, in your other script.
        {
            Main_Calibrator();
            initialCheck = false;
        }
    }

    public void Controller_Calibrate() // Finally, whenever you need to use the calibration button of the body tracker to recalibrate, or whenever you want to start a new trial
                                       // you need to call this public function from your other script!
    {
        Main_Calibrator();
    }

    private void Main_Calibrator()
    {
        GameObject xa = new GameObject();
        Vector3 pos = headRig.transform.position;
        pos.y = 0;
        Vector3 rot = headRig.transform.rotation.eulerAngles;
        rot.x = 0;
        rot.z = 0;
        xa.transform.position = pos;
        xa.transform.rotation = Quaternion.Euler(rot);

        Vector3 rot1 = xa.transform.forward;
        Vector3 rot2 = this.transform.forward;
        float angularDifference = Vector3.Angle(rot2, rot1);
        Vector3 cross = Vector3.Cross(rot2, rot1);

        if (cross.y < 0)
            angularDifference = -angularDifference;

        Yaw = Quaternion.Euler(new Vector3(0, angularDifference, 0));
        //direction = rot;
        direction = Yaw * this.transform.forward;
        update_Stopper = true;
        previous_y_value = yRotHand;
        Destroy(xa);
    }

    private void FixedUpdate()
    {
        CapsuleFollowHeadSet();

        if (inputButtonValue > 0)
        {
            if (update_Stopper == true)
            {
                character.Move(direction * Time.fixedDeltaTime * speed);
                update_Stopper = false;
            }
            else
            {
                value = yRotHand - previous_y_value;
                Yaw = Quaternion.Euler(new Vector3(0, value, 0));
                Vector3 dir = Yaw * direction;
                character.Move(dir * Time.fixedDeltaTime * speed);
            }
        }

        //gravity
        bool isGrounded = CheckIfGrounded();

        if (isGrounded)
            fallingSpeed = 0;
        else
            fallingSpeed += gravity * Time.fixedDeltaTime;

        character.Move(Vector3.up * fallingSpeed * Time.fixedDeltaTime);
    }

    private void CapsuleFollowHeadSet()
    {
        character.height = headRig.position.y + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(headRig.position);
        character.center = new Vector3(capsuleCenter.x, character.height / 2 + character.skinWidth, capsuleCenter.z);
    }

    private bool CheckIfGrounded()
    {
        // tells us if on ground
        Vector3 rayStart = transform.TransformPoint(character.center);
        float rayLength = character.center.y + 0.01f;
        bool hasHit = Physics.SphereCast(rayStart, character.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
        return hasHit;
    }

    // From the other script how can you call the variables and functions? At first you need to create a variable in your other script which will be of type this script.
    // Up Next you need to set the "initialCheck" variable of this script to "true" in your other script, so that this script will start to execute properly.
    // Then you need to check the "Primary Button" of which controller has been pressed by the participant!
    // The code is -

    // if (GC.BD.Which_Handed_Person == 1)
    //      ControllerActivated = CheckIfActivated(RightController);
    // else
    //      ControllerActivated = CheckIfActivated(LeftController);
    //
    // if (ControllerActivated == true || Input.GetKey(KeyCode.Space) /*This is for testing purposes only*/)
    // {
    //      if (updateStopper == 0)
    //      {
    //          MSC.Controller_Calibrate();
    //          updateStopper = 1;
    //      }
    // }
    // else
    //      updateStopper = 0;

    // Do not worry about the "BD" variable! It is just a structure variable where I kept every variable in the "GC" script so that it would be easier for me to access them from other scripts!
    // "ControllerActivated" is a boolean variable. It will return true, if the active hand controller's "Primary Button" has been pressed.
    // Similarly, "updateStopper" is also like a boolean variable. However, I used it as an integer variable.You can definitely use it as a boolean variable! The purpose of this variable
    // is to stop executing the checking multiple times. As, you will be defining these lines of codes in the Update method, you have to execute this code only once when the button is being pressed
    // by the participant. Otherwise, the code will be executing in every frame, and you will get errorprone steering movements!
    // "MSC" is the variable that I used in my "GC" script to refer this steering script in that script.
    // When the participant will release the button, this variable will automatically resets itself!

    // The "CheckIfActivated" method is defined below -

    // public bool CheckIfActivated(XRController controller)
    // {
    //     InputHelpers.IsPressed(controller.inputDevice, teleportActivationButton, out bool isActivated, activationThresold);
    //     return isActivated;
    // }

    // This above method is responsible for detecting whether the "Primary Button" of the active hand's controller has been pressed or not!
    // For executing this method, you need to define some variables on the top of your othe script which are -

    // public XRController LeftController, RightController;
    // Drag and Drop the LeftHand Controller GameObject to the LeftController variable, and RightHand Controller GameObject to the RightController variable from the XR Origin, in the inspector window.

    // public InputHelpers.Button teleportActivationButton;
    // Choose Primary Button from the Dropdown menu of this variable in the inspector window.

    // public float activationThresold = 0.1f;

    // Also, you have to import a namespace on the top of your other script so that the XRController class is being detected properly by the compiler.
    // The namespace is "using UnityEngine.XR.Interaction.Toolkit;"

    // You must have XR Interaction ToolKit installed in order to use this namespace in your script. If you are using Unity 2020 or before, you can go to Package Manager,
    // change the package to "Unity Registry", and from there, you can search XR Interaction ToolKit and install it in your project.
    // However, from Unity 2021, the XR Interaction ToolKit has been deprecated from Package Manager. Now, to get that, again go to the Package Manager, and this time
    // click on the "+" icon on the top left corner of package manager, and from there, choose "add package from git URL"
    // then in the given text box type in -
    // "com.unity.xr.interaction.toolkit" without the inverted commas, and press enter or return key in order to install it in your project.

    // If the participant thinks that he/she is moving to a differnet way, then he/she needs to recalibrate him/herself by pressing the "Primary Button" of the non-active hand's
    // controller or the body tracker. Here is the code to do that -

    // if (BD.Which_Handed_Person == 1)
    //      Other_Controller_Activated = CheckIfActivated(LeftController);
    // else
    //      Other_Controller_Activated = CheckIfActivated(RightController);
    // 
    // if (Other_Controller_Activated == true || Input.GetKey(KeyCode.Tab) /*This is for testing purposes only*/)
    // {
    //     if (Calibration_Done == false)
    //     {
    //          MSC.Controller_Calibrate();
    //          Calibration_Done = true;
    //     }
    // }
    // else
    //    Calibration_Done = false;

    // Like the previous example, here also, "Other_Controller_Activated" and "Calibration_Done" are two boolean variables.
    // The only difference between these two scripts are, the parameters passed through the "CheckIfActivated" methods.
    // In the previous example, if the active hand was right hand, then the parameter of this method was right hand.
    // However, in this case, it would be left hand, as the participant needs to recalibrate him/herself by pressing the "Primary Button" of the other controller!

    // Finally if you want to do some data recording stuffs or like showing some UI elements to the participants, you may want to restrict the participant from steering.
    // To do that you need to write this code in your other script -

    // MSC.Final_UI_Activated = true;
    // Making this boolean variable to true would stop the whole code from being executed in the Update method of this steering script.

    // To make this script work again, you need to turn this variable back on in your other script by writing this piece of code -

    // MSC.Final_UI_Activated = false;

    // At the end, do not forget to call the "MSC.Controller_Calibrate()" method once you finish a trial and start the next trial. This will make sure that the participant would
    // definitely move forward with whatever rotation of their body tracker is at that given point of time!

    // I hope all this guide will help you to setup this code properly with your project.
    // Please feel free to contact me, if you have any question regarding the guide, and setting up this script with your current project!
}

