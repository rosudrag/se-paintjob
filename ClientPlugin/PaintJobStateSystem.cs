using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Sandbox.ModAPI;
using VRage.Game;
using VRageMath;

namespace ClientPlugin
{

    public class PaintJobStateSystem : IPaintJobStateSystem
    {
        private readonly Dictionary<string, Color> _colorDictionary = new Dictionary<string, Color>();
        private readonly List<Color> _colors = new List<Color>();

        private readonly IPaintJobHelpSystem _helpSystem;
        private readonly string _stateFilePath = "PaintJobState.xml";
        private readonly string _stateHashFilePath = "PaintJobStateHash.txt";
        private Style _currentStyle = Style.Test;

        public PaintJobStateSystem(IPaintJobHelpSystem helpSystem)
        {
            _helpSystem = helpSystem;
            InitializeColorDictionary();
        }

        public void ShowState()
        {
            var sb = new StringBuilder("Paint plugin state");
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Colors selected:");
            sb.AppendLine();
            for (var i = 0; i < _colors.Count; i++)
            {
                var color = _colors[i];
                var colorName = _colorDictionary.FirstOrDefault(x => x.Value == color).Key;
                sb.AppendLine($"{i}.{colorName}");
            }
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine($"Style: {_currentStyle.ToString()}");

            MyAPIGateway.Utilities.ShowMissionScreen("Paint state", "", "", sb.ToString(), null, "Close");
        }

        public void AddColor(string[] args)
        {
            if (args.Length == 0)
            {
                MyAPIGateway.Utilities.ShowNotification("Usage: /paint add [colorName]", 5000, MyFontEnum.Red);
                return;
            }

            var colorName = args[0];
            if (_colorDictionary.TryGetValue(colorName, out var color))
            {
                _colors.Add(color);
                MyAPIGateway.Utilities.ShowNotification($"Color '{colorName}' added.", 5000, MyFontEnum.Green);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification($"Invalid color '{colorName}'.", 5000, MyFontEnum.Red);
            }
        }

        public void RemoveColor(string[] args)
        {
            if (args.Length == 0)
            {
                MyAPIGateway.Utilities.ShowNotification("Usage: /paint remove [index]", 5000, MyFontEnum.Red);
                return;
            }

            if (int.TryParse(args[0], out var index))
            {
                if (index >= 0 && index < _colors.Count)
                {
                    var color = _colors[index];
                    var colorName = _colorDictionary.FirstOrDefault(x => x.Value == color).Key;
                    _colors.RemoveAt(index);
                    MyAPIGateway.Utilities.ShowNotification($"Color '{colorName} ({color})' removed.", 5000, MyFontEnum.Green);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("Invalid index. Please enter a valid integer.", 5000, MyFontEnum.Red);
                }
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification($"Invalid input '{args[0]}'. Please enter a valid integer.", 5000, MyFontEnum.Red);
            }
        }
        public IEnumerable<Color> GetColors()
        {
            return _colors;
        }


        public void SetStyle(Style style)
        {
            _currentStyle = style;
        }

        public Style GetCurrentStyle()
        {
            return _currentStyle;
        }

        public void Save()
        {
            var state = new SerializableState
            {
                Colors = _colors,
                CurrentStyle = _currentStyle
            };

            var serializer = new XmlSerializer(typeof(SerializableState));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, state);
                var serializedXml = stringWriter.ToString();

                var encodedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedXml));
                File.WriteAllText(_stateFilePath, encodedXml);
            }
        }

        public void Load()
        {
            if (File.Exists(_stateFilePath))
            {
                var encodedXml = File.ReadAllText(_stateFilePath);
                var serializedXml = Encoding.UTF8.GetString(Convert.FromBase64String(encodedXml));

                var serializer = new XmlSerializer(typeof(SerializableState));
                using (var stringReader = new StringReader(serializedXml))
                {
                    var state = (SerializableState)serializer.Deserialize(stringReader);

                    _colors.Clear();
                    _colors.AddRange(state.Colors);

                    _currentStyle = state.CurrentStyle;
                }
            }
        }

        private string CalculateHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private void InitializeColorDictionary()
        {
            var colorType = typeof(Color);
            var colorProperties = colorType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == colorType);

            foreach (var colorProperty in colorProperties)
            {
                var colorName = colorProperty.Name;
                var colorValue = (Color)colorProperty.GetValue(null);
                _colorDictionary[colorName] = colorValue;
            }

            _helpSystem.WithColors(_colorDictionary.Select(x => x.Key));
        }

        [Serializable]
        public class SerializableState
        {
            public List<Color> Colors { get; set; }
            public Style CurrentStyle { get; set; }
        }
    }

}