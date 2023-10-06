using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;

namespace MyFirstPlugin
{
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
