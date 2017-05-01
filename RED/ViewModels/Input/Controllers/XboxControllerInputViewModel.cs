﻿using Caliburn.Micro;
using RED.Interfaces;
using RED.Interfaces.Input;
using RED.Models.Input.Controllers;
using RED.ViewModels.Modules;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RED.ViewModels.Input.Controllers
{
    public class XboxControllerInputViewModel : PropertyChangedBase, IInputDevice
    {
        private readonly XboxControllerInputModel Model = new XboxControllerInputModel();
        private StateViewModel _state;
        public readonly Controller ControllerOne = new Controller(UserIndex.One);

        public string Name { get; private set; }
        public string DeviceType { get; private set; }

        public int SerialReadSpeed
        {
            get
            {
                return Model.SerialReadSpeed;
            }
            set
            {
                Model.SerialReadSpeed = value;
                NotifyOfPropertyChange(() => SerialReadSpeed);
            }
        }
        public bool AutoDeadzone
        {
            get
            {
                return Model.AutoDeadzone;
            }
            set
            {
                Model.AutoDeadzone = value;
                NotifyOfPropertyChange(() => AutoDeadzone);
            }
        }
        public int ManualDeadzone
        {
            get
            {
                return Model.ManualDeadzone;
            }
            set
            {
                Model.ManualDeadzone = value;
                NotifyOfPropertyChange(() => ManualDeadzone);
            }
        }

        public bool Connected
        {
            get
            {
                return Model.Connected;
            }
            set
            {
                Model.Connected = value;
                NotifyOfPropertyChange(() => Connected);
                _state.ControllerIsConnected = value;
            }
        }

        public XboxControllerInputViewModel(StateViewModel state)
        {
            _state = state;

            Name = "Xbox Controller 1";
            DeviceType = "Xbox";
        }

        public Dictionary<string, float> GetValues()
        {
            if (ControllerOne == null || !ControllerOne.IsConnected)
            {
                Connected = false;
                throw new Exception("Controller Disconnected");
            }
            Connected = true;

            var currentGamepad = ControllerOne.GetState().Gamepad;

            var deadzone = AutoDeadzone ? Math.Max(Gamepad.LeftThumbDeadZone, Gamepad.RightThumbDeadZone) : ManualDeadzone;
            Func<short, float> deadzoneTransform = x => x < deadzone && x > -deadzone ? 0 : ((x + (x < 0 ? deadzone : -deadzone)) / (float)(32768 - deadzone));

            return new Dictionary<string, float>()
            {
                {"JoyStick1X", deadzoneTransform(currentGamepad.LeftThumbX)},
                {"JoyStick1Y", deadzoneTransform(currentGamepad.LeftThumbY)},
                {"JoyStick2X", deadzoneTransform(currentGamepad.RightThumbX)},
                {"JoyStick2Y", deadzoneTransform(currentGamepad.RightThumbY)},
                
                {"LeftTrigger", (float)currentGamepad.LeftTrigger / 255},
                {"RightTrigger", (float)currentGamepad.RightTrigger / 255},
                {"ButtonA", ((currentGamepad.Buttons & GamepadButtonFlags.A) != 0) ? 1 : 0},
                {"ButtonB", ((currentGamepad.Buttons & GamepadButtonFlags.B) != 0) ? 1 : 0},
                {"ButtonX", ((currentGamepad.Buttons & GamepadButtonFlags.X) != 0) ? 1 : 0},
                {"ButtonY", ((currentGamepad.Buttons & GamepadButtonFlags.Y) != 0) ? 1 : 0},
                {"ButtonLb", ((currentGamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0) ? 1 : 0},
                {"ButtonRb", ((currentGamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0) ? 1 : 0},
                {"ButtonLs", ((currentGamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0) ? 1 : 0},
                {"ButtonRs", ((currentGamepad.Buttons & GamepadButtonFlags.RightThumb) != 0) ? 1 : 0},
                {"ButtonStart", ((currentGamepad.Buttons & GamepadButtonFlags.Start) != 0) ? 1 : 0},
                {"ButtonBack", ((currentGamepad.Buttons & GamepadButtonFlags.Back) != 0) ? 1 : 0},
                {"DPadL", ((currentGamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0) ? 1 : 0},
                {"DPadU", ((currentGamepad.Buttons & GamepadButtonFlags.DPadUp) != 0) ? 1 : 0},
                {"DPadR", ((currentGamepad.Buttons & GamepadButtonFlags.DPadRight) != 0) ? 1 : 0},
                {"DPadD", ((currentGamepad.Buttons & GamepadButtonFlags.DPadDown) != 0) ? 1 : 0}
            };
        }

        public void StartDevice()
        { }

        public void StopDevice()
        { }
    }
}