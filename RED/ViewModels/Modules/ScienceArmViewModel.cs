﻿using Caliburn.Micro;
using RED.Interfaces;
using RED.Interfaces.Input;
using System;
using System.Collections.Generic;

namespace RED.ViewModels.Modules
{
    public class ScienceArmViewModel : PropertyChangedBase, IInputMode
    {
        private readonly IDataRouter _router;
        private readonly IDataIdResolver _idResolver;
        private readonly ILogger _log;

        private const int ArmSpeedScale = 1000;
        private const int DrillSpeedScale = 1000;

        public string Name { get; }
        public string ModeType { get; }

        public ScienceArmViewModel(IDataRouter router, IDataIdResolver idResolver, ILogger log)
        {
            _router = router;
            _idResolver = idResolver;
            _log = log;

            Name = "Science Arm";
            ModeType = "ScienceArm";
        }

        public void StartMode()
        { }

        public void SetValues(Dictionary<string, float> values)
        {
            float armMovement = values["Arm"];
            float drillSpeed = values["Drill"];

            _router.Send(_idResolver.GetId("ScienceArmDrive"), (Int16)(armMovement * ArmSpeedScale));
            _router.Send(_idResolver.GetId("Drill"), (Int16)(drillSpeed * DrillSpeedScale));
        }

        public void StopMode()
        {
            _router.Send(_idResolver.GetId("ScienceArmDrive"), (Int16)(0), true);
            _router.Send(_idResolver.GetId("Drill"), (Int16)(0), true);
        }
    }
}
