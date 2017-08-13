﻿using Caliburn.Micro;
using RED.Addons;
using RED.Contexts;
using RED.ViewModels.Modules;
using RED.ViewModels.Navigation;

namespace RED.ViewModels.Settings.Modules
{
    public class GPSSettingsViewModel : PropertyChangedBase
    {
        private GPSSettingsContext _settings;
        private GPSViewModel _gpsvm;
        private MapViewModel _mapvm;

        public double BaseStationLocationLatitude
        {
            get
            {
                return _gpsvm.BaseStationLocation.Latitude;
            }
            set
            {
                _gpsvm.BaseStationLocation = new GPSCoordinate(value, _gpsvm.BaseStationLocation.Longitude);
                _settings.BaseStationLocationLatitude = value;
                NotifyOfPropertyChange(() => BaseStationLocationLatitude);
            }
        }

        public double BaseStationLocationLongitude
        {
            get
            {
                return _gpsvm.BaseStationLocation.Longitude;
            }
            set
            {
                _gpsvm.BaseStationLocation = new GPSCoordinate(_gpsvm.BaseStationLocation.Latitude, value);
                _settings.BaseStationLocationLongitude = value;
                NotifyOfPropertyChange(() => BaseStationLocationLongitude);
            }
        }

        public double StartLocationLatitude
        {
            get
            {
                return _settings.StartLocationLatitude;
            }
            set
            {
                _settings.StartLocationLatitude = value;
                _mapvm.StartPosition = new GPSCoordinate(StartLocationLatitude, StartLocationLongitude);
                NotifyOfPropertyChange(() => StartLocationLatitude);
            }
        }

        public double StartLocationLongitude
        {
            get
            {
                return _settings.StartLocationLongitude;
            }
            set
            {
                _settings.StartLocationLongitude = value;
                _mapvm.StartPosition = new GPSCoordinate(StartLocationLatitude, StartLocationLongitude);
                NotifyOfPropertyChange(() => StartLocationLongitude);
            }
        }

        public GPSSettingsViewModel(GPSSettingsContext settings, GPSViewModel GpsVM, MapViewModel MapVM)
        {
            _settings = settings;
            _gpsvm = GpsVM;
            _mapvm = MapVM;

            _gpsvm.BaseStationLocation = new GPSCoordinate(_settings.BaseStationLocationLatitude, _settings.BaseStationLocationLongitude);
            _mapvm.StartPosition = new GPSCoordinate(StartLocationLatitude, StartLocationLongitude);
        }

        public void SetLocation(string presetName)
        {
            switch (presetName)
            {
                case "SDELC":
                    StartLocationLatitude = 37.951631;
                    StartLocationLongitude = -91.777713;
                    break;
                case "FugitiveBeach":
                    StartLocationLatitude = 37.850025;
                    StartLocationLongitude = -91.701845;
                    break;
                case "HanksvilleInn":
                    StartLocationLatitude = 38.373933;
                    StartLocationLongitude = -110.708362;
                    break;
                case "MDRS":
                    StartLocationLatitude = 38.406426;
                    StartLocationLongitude = -110.791919;
                    break;
                case "Kielce":
                    StartLocationLatitude = 50.782542;
                    StartLocationLongitude = 20.462027;
                    break;
            }
        }

        public static GPSSettingsContext DefaultConfig = new GPSSettingsContext()
        {
            BaseStationLocationLatitude = 0,
            BaseStationLocationLongitude = 0,
            StartLocationLatitude = 38.406426,
            StartLocationLongitude = -110.791919
        };
    }
}