﻿using Caliburn.Micro;
using RED.Models.Modules;

namespace RED.ViewModels.Modules
{
    public class CameraMuxViewModel : PropertyChangedBase
    {
        private ControlCenterViewModel _cc;
        private CameraMuxModel _model;

        public byte MuxIndex
        {
            get
            {
                return _model.MuxIndex;
            }
            set
            {
                _model.MuxIndex = value;
                NotifyOfPropertyChange(() => MuxIndex);
            }
        }

        public CameraMuxViewModel(ControlCenterViewModel cc)
        {
            _cc = cc;
            _model = new CameraMuxModel();
        }

        public void SetMux()
        {
            _cc.DataRouter.Send(_cc.MetadataManager.GetId("CameraMuxSet"), MuxIndex);
        }
    }
}