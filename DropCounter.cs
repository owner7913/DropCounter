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

            var displayText = "Item Drop Counter:\n\n";
            
            List<string> currencyList = new List<string>();
            List<string> itemList = new List<string>();

            // 🔹 Currency Section
            if (Settings.TrackExalted.Value)
                currencyList.Add($"Exalted Orb: {_dropCounts.GetValueOrDefault("Exalted Orb", 0)}");

            if (Settings.TrackChaos.Value)
                currencyList.Add($"Chaos Orb: {_dropCounts.GetValueOrDefault("Chaos Orb", 0)}");

            if (Settings.TrackChance.Value)
                currencyList.Add($"Orb of Chance: {_dropCounts.GetValueOrDefault("Orb of Chance", 0)}");

            if (Settings.TrackAnnulment.Value)
                currencyList.Add($"Orb of Annulment: {_dropCounts.GetValueOrDefault("Orb of Annulment", 0)}");

            if (Settings.TrackPerfectJewellers.Value)
                currencyList.Add($"Perfect Jeweller's Orb: {_dropCounts.GetValueOrDefault("Perfect Jeweller's Orb", 0)}");

            if (Settings.TrackDivine.Value)
                currencyList.Add($"Divine Orb: {_dropCounts.GetValueOrDefault("Divine Orb", 0)}");

            if (Settings.TrackMirror.Value)
                currencyList.Add($"Mirror of Kalandra: {_dropCounts.GetValueOrDefault("Mirror of Kalandra", 0)}");

            // 🔹 Items Section
            if (Settings.TrackStellarAmulet.Value)
                itemList.Add($"Stellar Amulet: {_dropCounts.GetValueOrDefault("Stellar Amulet", 0)}");

            if (Settings.TrackSapphireRing.Value)
                itemList.Add($"Sapphire Ring: {_dropCounts.GetValueOrDefault("Sapphire Ring", 0)}");

            // Determine max list count to align properly
            int maxCount = Math.Max(currencyList.Count, itemList.Count);
            displayText += "Items:".PadRight(25) + "Currency:\n"; // Column headers

            for (int i = 0; i < maxCount; i++)
            {
                string itemEntry = (i < itemList.Count) ? itemList[i].PadRight(25) : "".PadRight(25);
                string currencyEntry = (i < currencyList.Count) ? currencyList[i] : "";
                displayText += $"{itemEntry} {currencyEntry}\n";
            }

            // If nothing is tracked, show default message
            if (currencyList.Count == 0 && itemList.Count == 0)
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
