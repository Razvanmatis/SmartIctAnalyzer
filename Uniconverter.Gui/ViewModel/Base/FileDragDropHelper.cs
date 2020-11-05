using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Uniconverter.Gui
{
    public interface IFileDragDropTarget
    {
        void OnFileDrop(string[] filepaths);
    }

}
