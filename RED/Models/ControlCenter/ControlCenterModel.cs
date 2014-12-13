﻿namespace RED.Models.ControlCenter
{
    using RED.Contexts;
    using RED.ViewModels.ControlCenter;
    using System.Collections.ObjectModel;

    public class ControlCenterModel
    {
        public SaveModuleStateVm SaveModuleState;
        public RemoveModuleStateVm RemoveModuleState;

        public readonly ObservableCollection<ButtonContext> ButtonContexts = new ObservableCollection<ButtonContext>();

        public StateManager StateManager;
        public ConsoleVm Console;
        public ModuleGridManager GridManager;
    }
}