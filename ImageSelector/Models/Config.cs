using System;
using System.Configuration;

namespace ImageSelector.Models
{
    internal static class Config
    {
        internal const string _errorDataKey = "ErroData";

        private const string _defaultFolderKey = "DefaultFolder";
        private const string _defaultSaveFolder = "DefaultSaveFolder";
        private const string _zoomSpeedKey = "ZoomSpeed";
        private const string _appSettingsSectionName = "appSettings";

        internal static string DefaultFolder
        {
            get
            {
                return GetSetting<string>(_defaultFolderKey);
            }
            set
            {
                SetSetting(_defaultFolderKey, value);
            }
        }
        internal static string DefaultSaveFolder
        {
            get
            {
                return GetSetting<string>(_defaultSaveFolder);
            }
            set
            {
                SetSetting(_defaultSaveFolder, value);
            }
        }
        internal static double ZoomSpeed
        {
            get
            {
                return GetSetting<double>(_zoomSpeedKey);
            }
            set
            {
                SetSetting(_zoomSpeedKey, value);
            }
        }

        private static T GetSetting<T>(string key)
        {
            try
            {
                return (T)Convert.ChangeType(ConfigurationManager.AppSettings[key], typeof(T));
            }
            catch (Exception ex)
            {
                ex.Data.Add(_errorDataKey, $"Error when retrieving AppSetting {key}");
                throw;
            }
        }
        private static void SetSetting(string key, object value)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[key].Value = value.ToString();
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                ex.Data.Add(_errorDataKey, $"Error when setting AppSetting {key} to {value}");
                throw;
            }
            finally
            {
                ConfigurationManager.RefreshSection(_appSettingsSectionName);
            }
        }
    }
}
