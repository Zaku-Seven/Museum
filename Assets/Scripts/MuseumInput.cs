using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shared keyboard + gamepad input for museum interactions.
/// </summary>
public static class MuseumInput
{
    public static bool InteractPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
    }

    public static bool ThrowPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
    }

    public static bool PlacePressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
    }

    public static bool UndoPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
    }

    public static bool PausePressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
    }

    /// <summary>Space on keyboard, left stick click (L3) on gamepad — A is reserved for interact.</summary>
    public static bool JumpPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.leftStickButton.wasPressedThisFrame;
    }

    /// <summary>Shift on keyboard, left trigger on gamepad.</summary>
    public static bool IsSprinting()
    {
        if (Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.5f;
    }

    /// <summary>Tab on keyboard, View/Select on gamepad — hold to inspect art.</summary>
    public static bool IsInspecting()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.isPressed)
        {
            return true;
        }

        return Gamepad.current != null && Gamepad.current.selectButton.isPressed;
    }

    /// <summary>J — toggle collection journal (pause → Collection for gamepad).</summary>
    public static bool JournalPressedThisFrame()
    {
        return Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
    }

    /// <summary>Mouse wheel Y, or gamepad bumpers / d-pad for stack selection.</summary>
    public static float StackScrollDelta()
    {
        float scroll = 0f;
        if (Mouse.current != null)
        {
            scroll = Mouse.current.scroll.ReadValue().y;
        }

        if (Gamepad.current == null)
        {
            return scroll;
        }

        if (Gamepad.current.leftShoulder.wasPressedThisFrame || Gamepad.current.dpad.up.wasPressedThisFrame)
        {
            return 1f;
        }

        if (Gamepad.current.rightShoulder.wasPressedThisFrame || Gamepad.current.dpad.down.wasPressedThisFrame)
        {
            return -1f;
        }

        return scroll;
    }

    public static Vector2 MoveInput()
    {
        Vector2 move = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                move.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                move.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                move.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                move.y += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > move.sqrMagnitude)
            {
                move = stick;
            }
        }

        return move.sqrMagnitude > 1f ? move.normalized : move;
    }

    public static Vector2 LookDelta()
    {
        Vector2 look = Vector2.zero;
        if (Mouse.current != null)
        {
            look = Mouse.current.delta.ReadValue();
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            if (Gamepad.current.rightStick.IsActuated())
            {
                float gamepadLookScale = PlayerSettingsStore.GamepadLookSensitivity;
                look += stick * (gamepadLookScale * 100f * Time.deltaTime);
            }
        }

        return look;
    }
}
