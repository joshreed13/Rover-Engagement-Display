﻿using Caliburn.Micro;
using RED.Interfaces;
using RED.Models.Modules;

namespace RED.ViewModels.Modules
{
    public class LightingViewModel : PropertyChangedBase
    {
        private readonly LightingModel _model;
        private readonly IDataRouter _router;
        private readonly IDataIdResolver _idResolver;

        public bool Enabled
        {
            get
            {
                return _model.Enabled;
            }
            set
            {
                _model.Enabled = value;
                NotifyOfPropertyChange(() => Enabled);
                if (value)
                    SendColors();
                else
                    TurnOff();
            }
        }
        public byte Red
        {
            get
            {
                return _model.Red;
            }
            set
            {
                _model.Red = value;
                NotifyOfPropertyChange(() => Red);
                SendColors();
            }
        }
        public byte Green
        {
            get
            {
                return _model.Green;
            }
            set
            {
                _model.Green = value;
                NotifyOfPropertyChange(() => Green);
                SendColors();
            }
        }
        public byte Blue
        {
            get
            {
                return _model.Blue;
            }
            set
            {
                _model.Blue = value;
                NotifyOfPropertyChange(() => Blue);
                SendColors();
            }
        }

        public LightingViewModel(IDataRouter router, IDataIdResolver idResolver)
        {
            _model = new LightingModel();
            _router = router;
            _idResolver = idResolver;
        }

        private void SendColors()
        {
            if (Enabled)
                _router.Send(_idResolver.GetId("UnderglowColor"), new byte[] { Red, Green, Blue });
        }

        private void TurnOff()
        {
            _router.Send(_idResolver.GetId("UnderglowColor"), new byte[] { 0, 0, 0 }, true);
        }
    }
}
