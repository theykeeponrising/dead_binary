//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/Input/PlayerInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInput : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""Controls"",
            ""id"": ""0c8a2bc4-59f9-4434-aa48-d1c485e68a3b"",
            ""actions"": [
                {
                    ""name"": ""InputPrimary"",
                    ""type"": ""Button"",
                    ""id"": ""873cb2ef-7ff4-4439-bd52-7b8ee5748b29"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InputSecondary"",
                    ""type"": ""Button"",
                    ""id"": ""dda10a2e-c663-4aac-8831-8d1054ee6fc0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ActionButton_1"",
                    ""type"": ""Button"",
                    ""id"": ""bde8fa6e-637c-4e3a-8b8f-9aad20ed09c2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ActionButton_2"",
                    ""type"": ""Button"",
                    ""id"": ""465990b5-7d31-469e-9fdf-964a70782140"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ActionButton_3"",
                    ""type"": ""Button"",
                    ""id"": ""2d06cda6-4d1c-4c92-9f89-fc24fea780b5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ActionButton_4"",
                    ""type"": ""Button"",
                    ""id"": ""dd3c04e5-5ef3-4a96-af74-1cd2d7946cd1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AnyKey"",
                    ""type"": ""Button"",
                    ""id"": ""28d7bf34-f166-4659-adbe-1c5448857d88"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InputCancel"",
                    ""type"": ""Button"",
                    ""id"": ""0580ac91-da68-4243-8219-e7e4f28b9d90"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InputSubmit"",
                    ""type"": ""Button"",
                    ""id"": ""f76971ca-528e-4d5b-ae6b-287a0a4f9df3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InputMenu"",
                    ""type"": ""Button"",
                    ""id"": ""dafc01b1-2ab8-4dda-97ce-ea4217c663a3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""InputPosition"",
                    ""type"": ""Value"",
                    ""id"": ""762def09-cfe9-4a63-8f45-b7511e10ee9c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""InputJoystick"",
                    ""type"": ""Value"",
                    ""id"": ""fd993f7b-cf4b-4e67-9af2-20a5572330e7"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""64e6d01b-efb1-4102-8c96-9aedf7e82709"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputPrimary"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ea768769-f9eb-4ef6-abfe-963188470f66"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputPrimary"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8f648f37-5cd6-4086-8b3d-06153e9febef"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputSecondary"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4e0a7b8b-7619-48e3-91d2-bbf147f7d3ee"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputSecondary"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aab998f1-c025-4777-825e-50764c13443e"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b4fc4c54-cab9-41fa-b256-1ca3979876f3"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c8de980e-a263-4a11-9bae-e9632ff6f46a"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3f41fd6a-105c-483b-8fb7-cbff68424962"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""febd62ee-cf2a-46a9-83f1-4153a2097f16"",
                    ""path"": ""<Keyboard>/anyKey"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AnyKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be0362a2-b2d9-41db-b16d-392192502dc8"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AnyKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9fe3ebfa-6743-4dea-b2ac-a1a8c811be37"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AnyKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""29286638-4b19-4546-bfda-78d32faf311d"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""38557897-99e5-4d75-9bb3-1e838a178e63"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a3d7f4d8-2a2c-4b61-ad95-0d72d9620fd1"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""63b720fc-bf6e-4c92-8f5b-33221192116e"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41c5dda8-e2c1-4d24-acc6-3e5b744a5322"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""25ceb987-0b78-4900-87a0-c9588871f506"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""53f8300d-4ed4-47c4-abb5-bb12be36070a"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e96a8fc4-0d50-4eee-9d6c-431471005717"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ActionButton_4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""204d7b92-404a-431f-bfad-1f8b59cc9976"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputJoystick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""83e7a9c3-0d5e-48c9-9020-53711c9baf2c"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputSubmit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cbb37a54-27af-42ce-8419-2cf7e6de7f9f"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputSubmit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""588098f6-fc00-4bb8-bd69-cfc76237980b"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""21f99668-d68a-4be1-aed1-41da6d0d775e"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InputMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Controls
        m_Controls = asset.FindActionMap("Controls", throwIfNotFound: true);
        m_Controls_InputPrimary = m_Controls.FindAction("InputPrimary", throwIfNotFound: true);
        m_Controls_InputSecondary = m_Controls.FindAction("InputSecondary", throwIfNotFound: true);
        m_Controls_ActionButton_1 = m_Controls.FindAction("ActionButton_1", throwIfNotFound: true);
        m_Controls_ActionButton_2 = m_Controls.FindAction("ActionButton_2", throwIfNotFound: true);
        m_Controls_ActionButton_3 = m_Controls.FindAction("ActionButton_3", throwIfNotFound: true);
        m_Controls_ActionButton_4 = m_Controls.FindAction("ActionButton_4", throwIfNotFound: true);
        m_Controls_AnyKey = m_Controls.FindAction("AnyKey", throwIfNotFound: true);
        m_Controls_InputCancel = m_Controls.FindAction("InputCancel", throwIfNotFound: true);
        m_Controls_InputSubmit = m_Controls.FindAction("InputSubmit", throwIfNotFound: true);
        m_Controls_InputMenu = m_Controls.FindAction("InputMenu", throwIfNotFound: true);
        m_Controls_InputPosition = m_Controls.FindAction("InputPosition", throwIfNotFound: true);
        m_Controls_InputJoystick = m_Controls.FindAction("InputJoystick", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Controls
    private readonly InputActionMap m_Controls;
    private IControlsActions m_ControlsActionsCallbackInterface;
    private readonly InputAction m_Controls_InputPrimary;
    private readonly InputAction m_Controls_InputSecondary;
    private readonly InputAction m_Controls_ActionButton_1;
    private readonly InputAction m_Controls_ActionButton_2;
    private readonly InputAction m_Controls_ActionButton_3;
    private readonly InputAction m_Controls_ActionButton_4;
    private readonly InputAction m_Controls_AnyKey;
    private readonly InputAction m_Controls_InputCancel;
    private readonly InputAction m_Controls_InputSubmit;
    private readonly InputAction m_Controls_InputMenu;
    private readonly InputAction m_Controls_InputPosition;
    private readonly InputAction m_Controls_InputJoystick;
    public struct ControlsActions
    {
        private @PlayerInput m_Wrapper;
        public ControlsActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @InputPrimary => m_Wrapper.m_Controls_InputPrimary;
        public InputAction @InputSecondary => m_Wrapper.m_Controls_InputSecondary;
        public InputAction @ActionButton_1 => m_Wrapper.m_Controls_ActionButton_1;
        public InputAction @ActionButton_2 => m_Wrapper.m_Controls_ActionButton_2;
        public InputAction @ActionButton_3 => m_Wrapper.m_Controls_ActionButton_3;
        public InputAction @ActionButton_4 => m_Wrapper.m_Controls_ActionButton_4;
        public InputAction @AnyKey => m_Wrapper.m_Controls_AnyKey;
        public InputAction @InputCancel => m_Wrapper.m_Controls_InputCancel;
        public InputAction @InputSubmit => m_Wrapper.m_Controls_InputSubmit;
        public InputAction @InputMenu => m_Wrapper.m_Controls_InputMenu;
        public InputAction @InputPosition => m_Wrapper.m_Controls_InputPosition;
        public InputAction @InputJoystick => m_Wrapper.m_Controls_InputJoystick;
        public InputActionMap Get() { return m_Wrapper.m_Controls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ControlsActions set) { return set.Get(); }
        public void SetCallbacks(IControlsActions instance)
        {
            if (m_Wrapper.m_ControlsActionsCallbackInterface != null)
            {
                @InputPrimary.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPrimary;
                @InputPrimary.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPrimary;
                @InputPrimary.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPrimary;
                @InputSecondary.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSecondary;
                @InputSecondary.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSecondary;
                @InputSecondary.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSecondary;
                @ActionButton_1.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_1;
                @ActionButton_1.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_1;
                @ActionButton_1.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_1;
                @ActionButton_2.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_2;
                @ActionButton_2.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_2;
                @ActionButton_2.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_2;
                @ActionButton_3.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_3;
                @ActionButton_3.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_3;
                @ActionButton_3.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_3;
                @ActionButton_4.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_4;
                @ActionButton_4.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_4;
                @ActionButton_4.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnActionButton_4;
                @AnyKey.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAnyKey;
                @AnyKey.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAnyKey;
                @AnyKey.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAnyKey;
                @InputCancel.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputCancel;
                @InputCancel.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputCancel;
                @InputCancel.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputCancel;
                @InputSubmit.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSubmit;
                @InputSubmit.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSubmit;
                @InputSubmit.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputSubmit;
                @InputMenu.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputMenu;
                @InputMenu.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputMenu;
                @InputMenu.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputMenu;
                @InputPosition.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPosition;
                @InputPosition.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPosition;
                @InputPosition.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputPosition;
                @InputJoystick.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputJoystick;
                @InputJoystick.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputJoystick;
                @InputJoystick.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInputJoystick;
            }
            m_Wrapper.m_ControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @InputPrimary.started += instance.OnInputPrimary;
                @InputPrimary.performed += instance.OnInputPrimary;
                @InputPrimary.canceled += instance.OnInputPrimary;
                @InputSecondary.started += instance.OnInputSecondary;
                @InputSecondary.performed += instance.OnInputSecondary;
                @InputSecondary.canceled += instance.OnInputSecondary;
                @ActionButton_1.started += instance.OnActionButton_1;
                @ActionButton_1.performed += instance.OnActionButton_1;
                @ActionButton_1.canceled += instance.OnActionButton_1;
                @ActionButton_2.started += instance.OnActionButton_2;
                @ActionButton_2.performed += instance.OnActionButton_2;
                @ActionButton_2.canceled += instance.OnActionButton_2;
                @ActionButton_3.started += instance.OnActionButton_3;
                @ActionButton_3.performed += instance.OnActionButton_3;
                @ActionButton_3.canceled += instance.OnActionButton_3;
                @ActionButton_4.started += instance.OnActionButton_4;
                @ActionButton_4.performed += instance.OnActionButton_4;
                @ActionButton_4.canceled += instance.OnActionButton_4;
                @AnyKey.started += instance.OnAnyKey;
                @AnyKey.performed += instance.OnAnyKey;
                @AnyKey.canceled += instance.OnAnyKey;
                @InputCancel.started += instance.OnInputCancel;
                @InputCancel.performed += instance.OnInputCancel;
                @InputCancel.canceled += instance.OnInputCancel;
                @InputSubmit.started += instance.OnInputSubmit;
                @InputSubmit.performed += instance.OnInputSubmit;
                @InputSubmit.canceled += instance.OnInputSubmit;
                @InputMenu.started += instance.OnInputMenu;
                @InputMenu.performed += instance.OnInputMenu;
                @InputMenu.canceled += instance.OnInputMenu;
                @InputPosition.started += instance.OnInputPosition;
                @InputPosition.performed += instance.OnInputPosition;
                @InputPosition.canceled += instance.OnInputPosition;
                @InputJoystick.started += instance.OnInputJoystick;
                @InputJoystick.performed += instance.OnInputJoystick;
                @InputJoystick.canceled += instance.OnInputJoystick;
            }
        }
    }
    public ControlsActions @Controls => new ControlsActions(this);
    public interface IControlsActions
    {
        void OnInputPrimary(InputAction.CallbackContext context);
        void OnInputSecondary(InputAction.CallbackContext context);
        void OnActionButton_1(InputAction.CallbackContext context);
        void OnActionButton_2(InputAction.CallbackContext context);
        void OnActionButton_3(InputAction.CallbackContext context);
        void OnActionButton_4(InputAction.CallbackContext context);
        void OnAnyKey(InputAction.CallbackContext context);
        void OnInputCancel(InputAction.CallbackContext context);
        void OnInputSubmit(InputAction.CallbackContext context);
        void OnInputMenu(InputAction.CallbackContext context);
        void OnInputPosition(InputAction.CallbackContext context);
        void OnInputJoystick(InputAction.CallbackContext context);
    }
}
