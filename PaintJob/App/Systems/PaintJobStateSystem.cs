using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using PaintJob.App.Models;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.Systems
{

    public class PaintJobStateSystem : IPaintJobStateSystem
    {
        private List<Color> _colors = new List<Color>();

        private readonly string _stateFilePath = "PaintJobState.xml";
        private Style _currentStyle = Style.Rudimentary;

        public void ShowState()
        {
            var sb = new StringBuilder("Paint plugin state");
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine();
            sb.AppendLine($"Colors selected: first {_colors.Count} from palette");
            sb.AppendLine();
            sb.AppendLine("--- --- --- --- --- ---");
            sb.AppendLine($"Style: {_currentStyle.ToString()}");

            MyAPIGateway.Utilities.ShowMissionScreen("Paint state", "", "", sb.ToString(), null, "Close");
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
        
        public void Reset()
        {
            _colors.Clear();
            _currentStyle = Style.Rudimentary;
            
            Save();
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
        
        [Serializable]
        public class SerializableState
        {
            public List<Color> Colors { get; set; }
            public Style CurrentStyle { get; set; }
        }
    }

}