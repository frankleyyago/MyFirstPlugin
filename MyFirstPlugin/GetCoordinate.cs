#region Namespaces
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace MyFirstPlugin
{
    /// <summary>
    /// Get the coordinate of a picked element
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    internal class GetCoordinate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                Selection sel = uidoc.Selection;
                XYZ coord = sel.PickPoint("Pick a point to get the coordinate");

                TaskDialog.Show("Coordinate", $"X: {coord.X}\nY: {coord.Y}\nZ: {coord.Z}");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
