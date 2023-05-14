using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#if !TORCH

namespace Shared.Config
{
    public class PluginConfig : IPluginConfig
    {

        private bool enabled = true;
        public event PropertyChangedEventHandler PropertyChanged;
        // TODO: Implement your config fields

        public bool Enabled
        {
            get => enabled;
            set => SetValue(ref enabled, value);
        }

        private void SetValue<T>(ref T field, T value, [CallerMemberName] string propName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propName);
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;

            propertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        // TODO: Encapsulate them as properties
    }
}

#endif