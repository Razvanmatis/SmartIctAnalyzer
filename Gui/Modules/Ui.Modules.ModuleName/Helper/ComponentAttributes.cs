namespace Ui.Modules.ModuleName.Helper
{
    public class ComponentAttributes
    {
        public ComponentAttributes(
            double actualXPos,
            double actualYPos,
            double xPosition,
            double yPosition,
            double zoomFactor,
            double overallWidth,
            double overallHeight,
            double originalWidth,
            double originalHeight)
        {
            ActualXPos = actualXPos;
            ActualYPos = actualYPos;
            XPosition = xPosition;
            YPosition = yPosition;
            ZoomFactor = zoomFactor;
            OverallWidth = overallWidth;
            OverallHeight = overallHeight;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public double ActualXPos { get; set; }

        public double ActualYPos { get; set; }

        public double XPosition { get; set; }

        public double YPosition { get; set; }

        public double ZoomFactor { get; set; }

        public double OverallWidth { get; set; }

        public double OverallHeight { get; set; }

        public double OriginalWidth { get; set; }

        public double OriginalHeight { get; set; }
    }
}
