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
        private MyGuiControlLabel statusLabel;
        
        private IPaintJob paintJob;
        private List<MyCubeGrid> availableGrids;
        private Dictionary<Style, List<string>> styleVariants;
        
        private const float MAX_RANGE = 500f; // 500 meters

        public PaintJobGui(IPaintJob paintJob) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.5f, 0.6f))
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
            styleVariants = new Dictionary<Style, List<string>>
            {
                { Style.Rudimentary, new List<string> { "Default" } },
                { Style.Military, new List<string> { "Standard", "Stealth", "Asteroid", "Industrial", "Deep_Space" } },
                { Style.Racing, new List<string> { "Formula1", "Rally", "Street", "Drag", "Endurance" } },
                { Style.Pirate, new List<string> { "Skull", "Kraken", "Blackbeard", "Corsair", "Marauder" } },
                { Style.Corporate, new List<string> { "Mining_Corp", "Transport_Co", "Security_Firm", "Research_Lab", "Construction" } },
                { Style.Alien, new List<string> { "Organic", "Crystalline", "Techno_Organic", "Hive", "Ancient" } },
                { Style.Retro, new List<string> { "80s_Neon", "50s_Chrome", "70s_Disco", "90s_Cyber", "Art_Deco" } }
            };
        }

        public override string GetFriendlyName()
        {
            return "Paint Job Designer";
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            
            var currentPosition = new Vector2(0f, -0.25f);
            var buttonSize = new Vector2(0.15f, 0.05f);
            var comboSize = new Vector2(0.3f, 0.05f);
            var spacing = 0.02f;
            
            // Title
            titleLabel = new MyGuiControlLabel(
                position: new Vector2(0f, currentPosition.Y),
                text: "PAINT JOB DESIGNER",
                colorMask: Color.White,
                textScale: 1.2f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(titleLabel);
            currentPosition.Y += 0.06f;
            
            // Grid selection
            gridLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: "Select Grid:",
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(gridLabel);
            currentPosition.Y += 0.03f;
            
            gridCombobox = new MyGuiControlCombobox(
                position: new Vector2(0f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            gridCombobox.SetToolTip("Select a grid within 500m that you own");
            Controls.Add(gridCombobox);
            
            // Refresh button next to grid combo
            refreshButton = new MyGuiControlButton(
                position: new Vector2(0.18f, currentPosition.Y),
                size: new Vector2(0.08f, 0.04f),
                text: new StringBuilder("Refresh"),
                onButtonClick: OnRefreshClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Small);
            Controls.Add(refreshButton);
            currentPosition.Y += 0.05f + spacing;
            
            // Style selection
            styleLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: "Paint Style:",
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(styleLabel);
            currentPosition.Y += 0.03f;
            
            styleCombobox = new MyGuiControlCombobox(
                position: new Vector2(0f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            styleCombobox.ItemSelected += OnStyleSelected;
            Controls.Add(styleCombobox);
            currentPosition.Y += 0.05f + spacing;
            
            // Variant selection
            variantLabel = new MyGuiControlLabel(
                position: new Vector2(-0.15f, currentPosition.Y),
                text: "Variant:",
                colorMask: Color.White,
                textScale: 0.8f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(variantLabel);
            currentPosition.Y += 0.03f;
            
            variantCombobox = new MyGuiControlCombobox(
                position: new Vector2(0f, currentPosition.Y),
                size: comboSize,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(variantCombobox);
            currentPosition.Y += 0.05f + spacing * 2;
            
            // Status label
            statusLabel = new MyGuiControlLabel(
                position: new Vector2(0f, currentPosition.Y),
                text: "",
                colorMask: Color.Yellow,
                textScale: 0.7f,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            Controls.Add(statusLabel);
            currentPosition.Y += 0.04f;
            
            // Buttons
            applyButton = new MyGuiControlButton(
                position: new Vector2(-0.08f, currentPosition.Y),
                size: buttonSize,
                text: new StringBuilder("Apply"),
                onButtonClick: OnApplyClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Default);
            applyButton.SetToolTip("Apply the selected paint job to the grid");
            Controls.Add(applyButton);
            
            cancelButton = new MyGuiControlButton(
                position: new Vector2(0.08f, currentPosition.Y),
                size: buttonSize,
                text: new StringBuilder("Cancel"),
                onButtonClick: OnCancelClicked,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                visualStyle: MyGuiControlButtonStyleEnum.Default);
            Controls.Add(cancelButton);
            
            // Initialize controls
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
            var sphere = new BoundingSphereD(playerPosition, MAX_RANGE);
            
            var entities = new List<VRage.Game.Entity.MyEntity>();
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, entities);
            
            foreach (var entity in entities)
            {
                var grid = entity as MyCubeGrid;
                if (grid == null || grid.Physics == null) continue;
                
                // Check ownership
                if (!IsGridOwnedByPlayer(grid, player.Identity.IdentityId)) continue;
                
                // Check distance
                var distance = Vector3D.Distance(playerPosition, grid.PositionComp.GetPosition());
                if (distance > MAX_RANGE) continue;
                
                availableGrids.Add(grid);
                
                var displayName = $"{grid.DisplayName} ({distance:F0}m)";
                var index = gridCombobox.GetItemsCount();
                gridCombobox.AddItem(index, displayName);
            }
            
            if (gridCombobox.GetItemsCount() > 0)
            {
                gridCombobox.SelectItemByIndex(0);
                UpdateStatus($"Found {availableGrids.Count} grid(s) in range");
            }
            else
            {
                UpdateStatus("No owned grids found within 500m", Color.Orange);
            }
        }

        private bool IsGridOwnedByPlayer(MyCubeGrid grid, long playerId)
        {
            // Check if player owns majority of blocks or is the grid owner
            var blocks = grid.GetBlocks();
            if (blocks.Count == 0) return false;
            
            int ownedBlocks = 0;
            foreach (var block in blocks)
            {
                if (block.FatBlock != null && block.FatBlock.OwnerId == playerId)
                {
                    ownedBlocks++;
                }
            }
            
            // Player owns if they own more than 50% of blocks with ownership
            var blocksWithOwnership = blocks.Count(b => b.FatBlock != null);
            if (blocksWithOwnership > 0 && ownedBlocks > blocksWithOwnership / 2)
                return true;
            
            // Also check BigOwners list
            return grid.BigOwners.Contains(playerId);
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
                UpdateStatus("Please select a grid", Color.Red);
                return;
            }
            
            var selectedStyleKey = styleCombobox.GetSelectedKey();
            if (selectedStyleKey == -1L)
            {
                UpdateStatus("Please select a paint style", Color.Red);
                return;
            }
            
            var selectedGrid = availableGrids[gridCombobox.GetSelectedIndex()];
            var selectedStyle = (Style)selectedStyleKey;
            
            // Build args array
            var args = new List<string> { selectedStyle.ToString() };
            
            // Add variant if not rudimentary
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
                // Apply paint job to selected grid
                ApplyPaintToGrid(selectedGrid, args.ToArray());
                
                UpdateStatus($"Paint applied to {selectedGrid.DisplayName}!", Color.Green);
                
                // Close GUI after successful application
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                    MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.OK,
                    new StringBuilder($"Successfully painted {selectedGrid.DisplayName} with {selectedStyle} style!"),
                    new StringBuilder("Paint Job Applied"),
                    callback: (result) => CloseScreen()));
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", Color.Red);
                MyAPIGateway.Utilities.ShowMessage("PaintJob", $"Error applying paint: {ex.Message}");
            }
        }

        private void ApplyPaintToGrid(MyCubeGrid grid, string[] args)
        {
            // We need to use the existing paint job system
            // This will be called from the GUI context
            paintJob.Run(args, grid);
        }

        private void OnCancelClicked(MyGuiControlButton button)
        {
            CloseScreen();
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