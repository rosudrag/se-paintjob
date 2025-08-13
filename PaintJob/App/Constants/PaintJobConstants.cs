using System.Collections.Generic;

namespace PaintJob.App.Constants
{
    public static class PaintJobConstants
    {
        public const float MAX_GRID_RANGE = 500f; // meters
        public const float GUI_WIDTH = 0.5f;
        public const float GUI_HEIGHT = 0.6f;
        public const float BUTTON_WIDTH = 0.15f;
        public const float BUTTON_HEIGHT = 0.05f;
        public const float COMBO_WIDTH = 0.3f;
        public const float COMBO_HEIGHT = 0.05f;
        public const float REFRESH_BUTTON_WIDTH = 0.07f;
        public const float REFRESH_BUTTON_HEIGHT = 0.035f;
        public const float SPACING = 0.02f;
        
        public const float MIN_OWNERSHIP_PERCENTAGE = 0.5f;
        public const string COMMAND_PREFIX = "/paint";
        public const string COMMAND_HELP = "help";
        public const string GUI_TITLE = "PAINT JOB DESIGNER";
        public const string GUI_FRIENDLY_NAME = "Paint Job Designer";
        public const string LABEL_SELECT_GRID = "Select Grid:";
        public const string LABEL_PAINT_STYLE = "Paint Style:";
        public const string LABEL_VARIANT = "Variant:";
        public const string BUTTON_APPLY = "Apply";
        public const string BUTTON_CANCEL = "Cancel";
        public const string BUTTON_REFRESH = "Refresh";
        public const string TOOLTIP_GRID_SELECT = "Select a grid within 500m that you own";
        public const string TOOLTIP_APPLY = "Apply the selected paint job to the grid";
        public const string STATUS_NO_GRID_SELECTED = "Please select a grid";
        public const string STATUS_NO_STYLE_SELECTED = "Please select a paint style";
        public const string STATUS_GRIDS_FOUND = "Found {0} grid(s) in range";
        public const string STATUS_NO_GRIDS_FOUND = "No owned grids found within 500m";
        public const string STATUS_PAINT_APPLIED = "Paint applied to {0}!";
        public const string STATUS_ERROR = "Error: {0}";
        public const string DIALOG_SUCCESS_TITLE = "Paint Job Applied";
        public const string DIALOG_SUCCESS_MESSAGE = "Successfully painted {0} with {1} style!";
        public const string HELP_TITLE = "Paint Job Help";
        public const string HELP_HEADER = "=== PAINT JOB DESIGNER ===";
        public const string NOTIFICATION_INVALID_COMMAND = "Invalid command. Use '/paint' to open GUI or '/paint help' for info.";
        public const string GITHUB_ISSUES_URL = "https://github.com/rosudrag/se-paintjob/issues";
        public const string BUTTON_SUGGESTIONS = "Suggestions";
    }
}