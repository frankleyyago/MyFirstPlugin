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
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PlaceGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {
                //Define a reference object to accept the pick result
                Reference pickedRef = null;

                //Pick a group
                Selection sel = uiapp.ActiveUIDocument.Selection;
                pickedRef = sel.PickObject(ObjectType.Element, "Please select a group");
                Element elem = doc.GetElement(pickedRef);
                Group group = elem as Group;

                //Pick point
                XYZ point = sel.PickPoint("Please pick a point to place group");

                //Place the group
                Transaction tx = new Transaction(doc);
                tx.Start("Lab");
                doc.Create.PlaceGroup(point, group.GroupType);
                tx.Commit();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Fail", "Action canceled by user");

                return Result.Failed;
            }

            
        }
    }
}
