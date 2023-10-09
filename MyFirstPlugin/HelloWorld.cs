#region Namespaces
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
#endregion

namespace MyFirstPlugin
{
    /// <summary>
    /// Show a dialog box with a message.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    internal class HelloWorld : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {                
                TaskDialog.Show("Title", "Hello World");

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
