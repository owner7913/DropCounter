using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using Graphics = ExileCore2.Graphics;
using RectangleF = ExileCore2.Shared.RectangleF;
using Color = System.Drawing.Color;
using System.Windows.Forms; // Required for MessageBox

namespace DropCounter
{
    public class DropCounter : BaseSettingsPlugin<Settings>
    {
        private Dictionary<string, int> _dropCounts = new Dictionary<string, int>();

        private HashSet<string> _trackedItems = new HashSet<string>
        {
            "Exalted Orb",
            "Chaos Orb",
            "Orb of Chance",
            "Orb of Annulment",    
            "Perfect Jeweller's Orb",        
            "Divine Orb",
            "Mirror of Kalandra"
        };

        private HashSet<string> _previousLabels = new HashSet<string>(); // Stores previously seen labels

        public override void OnLoad()
        {
            base.OnLoad();
            Name = "Item Drop Counter";

            // Attach event listener to reset button
            Settings.ResetCounter.OnPressed += () =>
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to reset all drop counters?", 
                    "Reset Confirmation", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    ResetCounters();
                }
            };
        }

        public override void Render()
        {
            if (!Settings.Enable.Value) return;

            var displayText = "";

            // Create separate lists for items and currency
            List<string> itemLines = new List<string>();
            List<string> currencyLines = new List<string>();

            bool hasTrackedCurrency = false;
            bool hasTrackedItems = false;

            // 🔹 Items Section
            if (Settings.TrackStellarAmulet.Value)
            {
                int count = _dropCounts.ContainsKey("Stellar Amulet") ? _dropCounts["Stellar Amulet"] : 0;
                itemLines.Add($"Stellar Amulet: {count}");
                hasTrackedItems = true;
            }

            if (Settings.TrackSapphireRing.Value)
            {
                int count = _dropCounts.ContainsKey("Sapphire Ring") ? _dropCounts["Sapphire Ring"] : 0;
                itemLines.Add($"Sapphire Ring: {count}");
                hasTrackedItems = true;
            }

            // 🔹 Currency Section
            if (Settings.TrackExalted.Value)
            {
                int count = _dropCounts.ContainsKey("Exalted Orb") ? _dropCounts["Exalted Orb"] : 0;
                currencyLines.Add($"Exalted Orb: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackChaos.Value)
            {
                int count = _dropCounts.ContainsKey("Chaos Orb") ? _dropCounts["Chaos Orb"] : 0;
                currencyLines.Add($"Chaos Orb: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackChance.Value)
            {
                int count = _dropCounts.ContainsKey("Orb of Chance") ? _dropCounts["Orb of Chance"] : 0;
                currencyLines.Add($"Orb of Chance: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackAnnulment.Value)
            {
                int count = _dropCounts.ContainsKey("Orb of Annulment") ? _dropCounts["Orb of Annulment"] : 0;
                currencyLines.Add($"Orb of Annulment: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackPerfectJewellers.Value)
            {
                int count = _dropCounts.ContainsKey("Perfect Jeweller's Orb") ? _dropCounts["Perfect Jeweller's Orb"] : 0;
                currencyLines.Add($"Perfect Jeweller's Orb: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackDivine.Value)
            {
                int count = _dropCounts.ContainsKey("Divine Orb") ? _dropCounts["Divine Orb"] : 0;
                currencyLines.Add($"Divine Orb: {count}");
                hasTrackedCurrency = true;
            }

            if (Settings.TrackMirror.Value)
            {
                int count = _dropCounts.ContainsKey("Mirror of Kalandra") ? _dropCounts["Mirror of Kalandra"] : 0;
                currencyLines.Add($"Mirror of Kalandra: {count}");
                hasTrackedCurrency = true;
            }

            // 🔹 Ensure alignment by matching rows
            int maxRows = Math.Max(itemLines.Count, currencyLines.Count);

            displayText += "Items:              Currency:\n";

            for (int i = 0; i < maxRows; i++)
            {
                string itemText = i < itemLines.Count ? itemLines[i] : "";
                string currencyText = i < currencyLines.Count ? currencyLines[i] : "";
                displayText += $"{itemText,-20} {currencyText}\n"; // Keeps everything aligned
            }

            // Show "No tracked items" only if both categories are empty
            if (!hasTrackedCurrency && !hasTrackedItems)
            {
                displayText += "\n(No tracked items yet)";
            }

            var size = Graphics.MeasureText(displayText, 20);
            var scrRect = GameController.Window.GetWindowRectangle();
            var topLeft = new Vector2(scrRect.X + Settings.X, scrRect.Y + Settings.Y);
            var drawRect = new RectangleF(topLeft.X - 5, topLeft.Y - 5, size.X + 10, size.Y + 10);

            Graphics.DrawText(displayText, new Vector2(topLeft.X, topLeft.Y), Color.White);
            Graphics.DrawBox(drawRect, Color.Black);
        }

        public override void Tick()
        {
            var labelsOnGround = GameController?.IngameState?.IngameUi?.ItemsOnGroundLabelElement?.LabelsOnGround;
            if (labelsOnGround == null || labelsOnGround.Count == 0) 
            {
                _previousLabels.Clear(); // Clear labels if nothing is visible
                return;
            }

            HashSet<string> currentLabels = new HashSet<string>();

            foreach (var label in labelsOnGround)
            {
                string itemText = label?.Label?.Text?.Trim();

                if (string.IsNullOrEmpty(itemText))
                    continue;

                int quantity = 1;
                string itemName = itemText;

                var match = System.Text.RegularExpressions.Regex.Match(itemText, @"(\d+)x (.+)");
                if (match.Success)
                {
                    quantity = int.Parse(match.Groups[1].Value); // Extract quantity (e.g., "20")
                    itemName = match.Groups[2].Value.Trim(); // Extract item name (e.g., "Exalted Orb")
                }

                // Handle case where no "x" exists, meaning it's a single item (e.g., "Exalted Orb")
                if (!match.Success)
                {
                    itemName = itemText.Trim(); // Use the full text directly
                }

                if (!_trackedItems.Contains(itemName))
                    continue;

                currentLabels.Add(itemText); // Track all visible labels

                // 🔹 If the label wasn't in the previous tick, count it as a new drop
                if (!_previousLabels.Contains(itemText))
                {
                    if (!_dropCounts.ContainsKey(itemName))
                        _dropCounts[itemName] = 0;

                    _dropCounts[itemName] += quantity; // Add to counter
                    DebugWindow.LogMsg($"DropCounter: Counted {quantity}x {itemName}, Total: {_dropCounts[itemName]}");
                }
            }

            _previousLabels = currentLabels; // Update stored labels for the next tick
        }

        public void ResetCounters()
        {
            _dropCounts.Clear();
            DebugWindow.LogMsg("DropCounter: Counters have been reset.");
        }

    }
}
