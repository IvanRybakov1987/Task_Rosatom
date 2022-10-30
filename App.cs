using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Task_Rosatom
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string rosatomTab = "Rosatom";
            application.CreateRibbonTab(rosatomTab);
            RibbonPanel panelCatalog = application.CreateRibbonPanel(rosatomTab, "Задание");

            PushButtonData btnTask1 = new PushButtonData("Task1", "Задание 1", @"C:\ProgramData\Autodesk\Revit\Addins\2020\Rosatom\Task_Rosatom.dll", "Task_Rosatom.Filter");
            Uri uriImageTask1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\Rosatom\green.png", UriKind.Absolute);
            BitmapImage largeImageTask1 = new BitmapImage(uriImageTask1);
            btnTask1.LargeImage = largeImageTask1;
            panelCatalog.AddItem(btnTask1);

            PushButtonData btnTask2 = new PushButtonData("Task2", "Задание 2", @"C:\ProgramData\Autodesk\Revit\Addins\2020\Rosatom\Task_Rosatom.dll", "Task_Rosatom.Main");
            Uri uriImageTask2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\Rosatom\blue.png", UriKind.Absolute);
            BitmapImage largeImageTask2 = new BitmapImage(uriImageTask2);
            btnTask2.LargeImage = largeImageTask2;
            panelCatalog.AddItem(btnTask2);

            return Result.Succeeded;
        }
    }
}
