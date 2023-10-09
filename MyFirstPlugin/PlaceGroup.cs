#region Namespaces
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace MyFirstPlugin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    ///<summary>
    /// Copy and paste a group of elements.
    /// </summary>
    public class PlaceGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application.
            UIApplication uiapp = commandData.Application;
            //Get document objects.
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {

                //Pick a group.
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();

                Reference pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");
                Element e = doc.GetElement(pickedRef);
                Group group = e as Group;

                //Pick point.
                XYZ point = sel.PickPoint("Please pick a point to place group");
                
                //Place the group.
                Transaction tx = new Transaction(doc);
                tx.Start("Lab");
                doc.Create.PlaceGroup(point, group.GroupType);
                tx.Commit();

                return Result.Succeeded;
            }
            //Handle canceled exceptions.
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            //Handle unexpected exceptions.
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            
        }
    }

    /// <summary>
    /// Filter the pick element to allow only groups to be picked/highlighted.
    /// </summary>
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups));
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}
