using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;

namespace DropCounter
{
    public class Settings : ISettings
    {
        public Settings()
        {
            Enable = new ToggleNode(true);
            X = new RangeNode<int>(150, -3000, 3000);
            Y = new RangeNode<int>(200, -3000, 3000);
            
            // 🔹 Transparency Option
            TransparentBackground = new ToggleNode(false); // Default: Off

            // 🔹 Currency Tracking
            TrackExalted = new ToggleNode(true);
            TrackChaos = new ToggleNode(true);
            TrackChance = new ToggleNode(false);
            TrackAnnulment = new ToggleNode(false);
            TrackPerfectJewellers = new ToggleNode(false);
            TrackDivine = new ToggleNode(true);
            TrackMirror = new ToggleNode(true);

            // 🔹 Item Tracking
            TrackStellarAmulet = new ToggleNode(false);
            TrackSapphireRing = new ToggleNode(false);

            // Reset Button
            ResetCounter = new ButtonNode();
        }

        public ToggleNode Enable { get; set; }
        public RangeNode<int> X { get; set; }
        public RangeNode<int> Y { get; set; }

        // 🔹 Transparency
        public ToggleNode TransparentBackground { get; set; }

        // 🔹 Currency
        public ToggleNode TrackExalted { get; set; }
        public ToggleNode TrackChaos { get; set; }
        public ToggleNode TrackChance { get; set; }
        public ToggleNode TrackAnnulment { get; set; }
        public ToggleNode TrackPerfectJewellers { get; set; }
        public ToggleNode TrackDivine { get; set; }
        public ToggleNode TrackMirror { get; set; }

        // 🔹 Items
        public ToggleNode TrackStellarAmulet { get; set; }
        public ToggleNode TrackSapphireRing { get; set; }

        public ButtonNode ResetCounter { get; set; }
    }
}
