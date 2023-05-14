using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private Style _currentStyle = Style.Rudimentary;

        public PaintJobStateSystem(IPaintJobHelpSystem helpSystem)
        {
            _helpSystem = helpSystem;
            InitializeColorDictionary();
        }

        public void ListColors()
        {
            var colorList = new StringBuilder("Colors: ");
            for (var i = 0; i < _colors.Count; i++)
            {
                var color = _colors[i];
                var colorName = _colorDictionary.FirstOrDefault(x => x.Value == color).Key;
                if (i > 0)
                {
                    colorList.Append(", ");
                }
                colorList.Append($"{colorName} ({color})");
            }

            MyAPIGateway.Utilities.ShowNotification(colorList.ToString(), 5000, MyFontEnum.Green);
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
                _helpSystem.ShowValidValues("Valid Colors", _colorDictionary.Select(x => x.Key));
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
        }
    }

}