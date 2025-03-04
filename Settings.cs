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

            // Checkboxes for tracking specific items
            TrackExalted = new ToggleNode(true);
            TrackChaos = new ToggleNode(true);
            TrackDivine = new ToggleNode(true);
            TrackAnnulment = new ToggleNode(true);
            TrackMirror = new ToggleNode(true);

            // Reset Button (now with confirmation)
            ResetCounter = new ButtonNode();
        }

        public ToggleNode Enable { get; set; }
        public RangeNode<int> X { get; set; }
        public RangeNode<int> Y { get; set; }

        public ToggleNode TrackExalted { get; set; }
        public ToggleNode TrackChaos { get; set; }
        public ToggleNode TrackDivine { get; set; }
        public ToggleNode TrackAnnulment { get; set; }
        public ToggleNode TrackMirror { get; set; }

        public ButtonNode ResetCounter { get; set; }
    }
}
