using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ProMik.SmartIct.Interfaces.Gui;

namespace Ui.Modules.ModuleName.Helper
{
    public class ContentToImageExporter
    {
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;

        public ContentToImageExporter(ILogger logger, IDialogSelector dialogSelector)
        {
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public void TakeTheChart(Visual target, string filePath = "")
        {
            bool logResult = string.IsNullOrEmpty(filePath);
            if (string.IsNullOrEmpty(filePath) && !dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                "Select destination to save the picture",
                "No valid target selected!",
                out filePath))
            {
                return;
            }

            using Bitmap bitmap = ControlToImage(target, 96, 96);
            if (bitmap != null)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    bitmap.SaveJPG100(filePath);
                    if (logResult)
                    {
                        logger.LogMessage("Picture successfully exported to " + filePath, LogCategory.INFO);
                    }
                }
                catch (Exception e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
            }
        }

        private Bitmap ControlToImage(Visual target, double dpiX, double dpiY)
        {
            if (target == null)
            {
                return null;
            }

            // render control content
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)(bounds.Width * dpiX / 96.0),
                (int)(bounds.Height * dpiY / 96.0),
                dpiX,
                dpiY,
                PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new Rect(default(System.Windows.Point), bounds.Size));
            }

            rtb.Render(dv);
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            try
            {
                encoder.Save(stream);
                return new Bitmap(stream);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
        }
    }
}
