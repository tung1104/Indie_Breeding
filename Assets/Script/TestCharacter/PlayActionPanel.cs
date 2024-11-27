using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayActionPanel : MonoBehaviour
{
    public JoystickPad joystickPad;
    public Character playerCharacter;
    public CharMoveIndicator charMoveIndicator;


    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        //TopdownCameraController.Instance.UpdateCameraPosition(0);
    }

    public void SetPlayerCharacter(Character playerChar)
    {
        playerCharacter = playerChar;
        playerChar.movement.isKinematic = false;
        playerChar.perception.boostUpdateNeighbours = true;
        TopdownCameraController.Instance.target = playerChar.gameObject;
        TopdownCameraController.Instance.targetBounds = playerChar.bounds;
        charMoveIndicator.movement = playerChar.movement;
    }

    private void OnDisable()
    {
        if (playerCharacter)
        {
            playerCharacter.movement.input = new MovementHandler.InputData()
            {
                moveSpeedLevel = -1
            };
        }
    }

    void FixedUpdate()
    {
        if (!playerCharacter) return;
        var inputMove = new Vector3(joystickPad.Value.x, 0, joystickPad.Value.y) +
                        new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        var willMove = inputMove != Vector3.zero;

        var input = playerCharacter.movement.input;
        if (willMove)
        {
            input.moveDirectionYaw = Quaternion.LookRotation(inputMove).eulerAngles.y;
        }

        input.jump = Input.GetKey(KeyCode.Space);
        input.moveSpeedLevel = willMove
            ? (Input.GetKey(KeyCode.LeftShift) ? 2 :
                Input.GetKey(KeyCode.LeftControl) ? 0 : 1)
            : -1;

        playerCharacter.movement.input = input;

        if (Input.GetKey(KeyCode.F))
            playerCharacter.attackTrigger = true;
    }
}