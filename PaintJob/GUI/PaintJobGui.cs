using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Utils;
using VRageMath;
using PaintJob.App;
using PaintJob.App.Models;
using PaintJob.App.Constants;
using PaintJob.App.Validation;

namespace PaintJob.GUI
{
    public class PaintJobGui : MyGuiScreenBase
    {
        private MyGuiControlLabel titleLabel;
        private MyGuiControlLabel gridLabel;
        private MyGuiControlCombobox gridCombobox;
        private MyGuiControlLabel styleLabel;
        private MyGuiControlCombobox styleCombobox;
        private MyGuiControlLabel variantLabel;
        private MyGuiControlCombobox variantCombobox;
        private MyGuiControlButton applyButton;
        private MyGuiControlButton cancelButton;
        private MyGuiControlButton refreshButton;
        private MyGuiControlButton suggestionsButton;
        private MyGuiControlLabel statusLabel;
        
        private IPaintJob paintJob;
        private List<MyCubeGrid> availableGrids;
        private Dictionary<Style, List<string>> styleVariants;

        public PaintJobGui(IPaintJob paintJob) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(PaintJobConstants.GUI_WIDTH, PaintJobConstants.GUI_HEIGHT))
        {
            this.paintJob = paintJob;
            availableGrids = new List<MyCubeGrid>();
            InitializeStyleVariants();
            
            EnabledBackgroundFade = true;
            CanHideOthers = false;
            CloseButtonEnabled = true;
            RecreateControls(true);
        }

        private void InitializeStyleVariants()
        {
            styleVariants = StyleVariants.GetStyleVariantMap();
        }

        public override string GetFriendlyName()
        {
            return PaintJobConstants.GUI_FRIENDLY_NAME;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            
            var currentPosition = new Vector2(0f, -0.25f);
            var buttonSize = new Vector2(PaintJobConstants.BUTTON_WIDTH, PaintJobConstants.BUTTON_HEIGHT);
            var comboSize = new Vector2(PaintJobConstants.COMBO_WIDTH, PaintJobConstants.COMBO_HEIGHT);
            var spacing = PaintJobConstants.SPACING;
            titleLabel = new MyGuiControlLabel(
                position: new Vector2(0f, currentPosition.Y),
                text: PaintJobConstants.GUI_TITLE,
                colorMask: Color.White,
                textScale: 1.2f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(titleLabel);
            currentPosition.Y += 0.06f;
            gridLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: PaintJobConstants.LABEL_SELECT_GRID,
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(gridLabel);
            currentPosition.Y += 0.03f;
            
            gridCombobox = new MyGuiControlCombobox(
                position: new Vector2(-0.08f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            gridCombobox.SetToolTip(PaintJobConstants.TOOLTIP_GRID_SELECT);
            Controls.Add(gridCombobox);
            refreshButton = new MyGuiControlButton(
                position: new Vector2(0.11f, currentPosition.Y),
                size: new Vector2(PaintJobConstants.REFRESH_BUTTON_WIDTH, PaintJobConstants.REFRESH_BUTTON_HEIGHT),
                text: new StringBuilder(PaintJobConstants.BUTTON_REFRESH),
                onButtonClick: OnRefreshClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Small);
            Controls.Add(refreshButton);
            currentPosition.Y += 0.05f + spacing;
            styleLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: PaintJobConstants.LABEL_PAINT_STYLE,
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(styleLabel);
            currentPosition.Y += 0.03f;
            
            styleCombobox = new MyGuiControlCombobox(
                position: new Vector2(-0.08f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            styleCombobox.ItemSelected += OnStyleSelected;
            Controls.Add(styleCombobox);
            currentPosition.Y += 0.05f + spacing;
            variantLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: PaintJobConstants.LABEL_VARIANT,
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(variantLabel);
            currentPosition.Y += 0.03f;
            
            variantCombobox = new MyGuiControlCombobox(
                position: new Vector2(-0.08f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(variantCombobox);
            currentPosition.Y += 0.05f + spacing * 2;
            statusLabel = new MyGuiControlLabel(
                position: new Vector2(0f, currentPosition.Y),
                text: "",
                colorMask: Color.Yellow,
                textScale: 0.7f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(statusLabel);
            currentPosition.Y += 0.04f;
            applyButton = new MyGuiControlButton(
                position: new Vector2(-0.08f, currentPosition.Y),
                size: buttonSize,
                text: new StringBuilder(PaintJobConstants.BUTTON_APPLY),
                onButtonClick: OnApplyClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Default);
            applyButton.SetToolTip(PaintJobConstants.TOOLTIP_APPLY);
            Controls.Add(applyButton);
            
            cancelButton = new MyGuiControlButton(
                position: new Vector2(0.08f, currentPosition.Y),
                size: buttonSize,
                text: new StringBuilder(PaintJobConstants.BUTTON_CANCEL),
                onButtonClick: OnCancelClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Default);
            Controls.Add(cancelButton);
            
            currentPosition.Y += 0.06f;
            suggestionsButton = new MyGuiControlButton(
                position: new Vector2(0f, currentPosition.Y),
                size: new Vector2(0.12f, 0.04f),
                text: new StringBuilder(PaintJobConstants.BUTTON_SUGGESTIONS),
                onButtonClick: OnSuggestionsClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Small);
            suggestionsButton.SetToolTip("Open GitHub to submit suggestions or report issues");
            Controls.Add(suggestionsButton);
            
            PopulateStyleCombobox();
            RefreshGridList();
        }

        private void PopulateStyleCombobox()
        {
            styleCombobox.ClearItems();
            
            foreach (Style style in Enum.GetValues(typeof(Style)))
            {
                var displayName = style.ToString().Replace('_', ' ');
                styleCombobox.AddItem((long)style, displayName);
            }
            
            if (styleCombobox.GetItemsCount() > 0)
            {
                styleCombobox.SelectItemByIndex(0);
                UpdateVariantCombobox();
            }
        }

        private void RefreshGridList()
        {
            availableGrids.Clear();
            gridCombobox.ClearItems();
            
            var player = MySession.Static.LocalHumanPlayer;
            if (player == null) return;
            
            var playerPosition = player.GetPosition();
            var sphere = new BoundingSphereD(playerPosition, PaintJobConstants.MAX_GRID_RANGE);
            
            var entities = new List<VRage.Game.Entity.MyEntity>();
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, entities);
            
            foreach (var entity in entities)
            {
                var grid = entity as MyCubeGrid;
                if (grid == null || grid.Physics == null) continue;
                
                if (!GridValidator.CanPlayerPaintGrid(grid, player.Identity.IdentityId, playerPosition)) continue;
                
                var distance = GridValidator.GetDistanceToGrid(grid, playerPosition);
                
                availableGrids.Add(grid);
                
                var displayName = $"{grid.DisplayName} ({distance:F0}m)";
                var index = gridCombobox.GetItemsCount();
                gridCombobox.AddItem(index, displayName);
            }
            
            if (gridCombobox.GetItemsCount() > 0)
            {
                gridCombobox.SelectItemByIndex(0);
                UpdateStatus(string.Format(PaintJobConstants.STATUS_GRIDS_FOUND, availableGrids.Count));
            }
            else
            {
                UpdateStatus(PaintJobConstants.STATUS_NO_GRIDS_FOUND, Color.Orange);
            }
        }


        private void OnStyleSelected()
        {
            UpdateVariantCombobox();
        }

        private void UpdateVariantCombobox()
        {
            variantCombobox.ClearItems();
            
            var selectedKey = styleCombobox.GetSelectedKey();
            if (selectedKey == -1L) return;
            
            var selectedStyle = (Style)selectedKey;
            
            if (styleVariants.TryGetValue(selectedStyle, out var variants))
            {
                for (int i = 0; i < variants.Count; i++)
                {
                    var displayName = variants[i].Replace('_', ' ');
                    variantCombobox.AddItem(i, displayName);
                }
                
                if (variantCombobox.GetItemsCount() > 0)
                {
                    variantCombobox.SelectItemByIndex(0);
                }
            }
        }

        private void OnRefreshClicked(MyGuiControlButton button)
        {
            RefreshGridList();
        }

        private void OnApplyClicked(MyGuiControlButton button)
        {
            if (gridCombobox.GetSelectedIndex() < 0 || gridCombobox.GetSelectedIndex() >= availableGrids.Count)
            {
                UpdateStatus(PaintJobConstants.STATUS_NO_GRID_SELECTED, Color.Red);
                return;
            }
            
            var selectedStyleKey = styleCombobox.GetSelectedKey();
            if (selectedStyleKey == -1L)
            {
                UpdateStatus(PaintJobConstants.STATUS_NO_STYLE_SELECTED, Color.Red);
                return;
            }
            
            var selectedGrid = availableGrids[gridCombobox.GetSelectedIndex()];
            var selectedStyle = (Style)selectedStyleKey;
            
            var args = new List<string> { selectedStyle.ToString() };
            
            if (selectedStyle != Style.Rudimentary && variantCombobox.GetSelectedIndex() >= 0)
            {
                if (styleVariants.TryGetValue(selectedStyle, out var variants))
                {
                    var variant = variants[variantCombobox.GetSelectedIndex()];
                    args.Add(variant.ToLower());
                }
            }
            
            try
            {
                ApplyPaintToGrid(selectedGrid, args.ToArray());
                
                UpdateStatus(string.Format(PaintJobConstants.STATUS_PAINT_APPLIED, selectedGrid.DisplayName), Color.Green);
                
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                    MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.OK,
                    new StringBuilder(string.Format(PaintJobConstants.DIALOG_SUCCESS_MESSAGE, selectedGrid.DisplayName, selectedStyle)),
                    new StringBuilder(PaintJobConstants.DIALOG_SUCCESS_TITLE),
                    callback: (result) => CloseScreen()));
            }
            catch (Exception ex)
            {
                UpdateStatus(string.Format(PaintJobConstants.STATUS_ERROR, ex.Message), Color.Red);
                MyAPIGateway.Utilities.ShowMessage("PaintJob", $"Error applying paint: {ex.Message}");
            }
        }

        private void ApplyPaintToGrid(MyCubeGrid grid, string[] args)
        {
            paintJob.Run(args, grid);
        }

        private void OnCancelClicked(MyGuiControlButton button)
        {
            CloseScreen();
        }
        
        private void OnSuggestionsClicked(MyGuiControlButton button)
        {
            System.Diagnostics.Process.Start(PaintJobConstants.GITHUB_ISSUES_URL);
        }

        private void UpdateStatus(string message, Color? color = null)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = message;
                statusLabel.ColorMask = color ?? Color.White;
            }
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                CloseScreen();
            }
        }
    }
}