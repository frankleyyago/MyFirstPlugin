#region Namespaces
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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

                //Get the group's center point.
                XYZ origin = GetElementCenter(group);

                //Get the that the picked group is located in.
                Room room = GetRoomOfGroup(doc, origin);

                //Get the room's center point.
                XYZ sourceCenter = GetElementCenter(room);
                string coords =
                    $"X = {sourceCenter.X.ToString()}\r\n" +
                    $"Y = {sourceCenter.Y.ToString()}\r\n" +
                    $"Z= {sourceCenter.Z.ToString()}";

                TaskDialog.Show("Source room center: ", coords);

                //Pick point.
                //XYZ point = sel.PickPoint("Please pick a point to place group");
                
                //Place the group.
                Transaction tx = new Transaction(doc);
                tx.Start("Lab");
                //doc.Create.PlaceGroup(point, group.GroupType);

                //Calculate the new group's position.
                XYZ groupLocation = sourceCenter + new XYZ(20, 0, 0);
                doc.Create.PlaceGroup(groupLocation, group.GroupType);
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

        #region GroupPickFilter()
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
        #endregion

        #region GetElementCenter()
        /// <summary>
        /// Get the center of a group bounding box.
        /// </summary>
        public XYZ GetElementCenter(Element e)
        {
            BoundingBoxXYZ bBox = e.get_BoundingBox(null);
            XYZ center = (bBox.Max + bBox.Min) * 0.5;
            return center;
        }
        #endregion

        #region GetRoomOfGroup()
        /// <summary>
        /// Get the room where a group lies to.
        /// </summary>
        public Room GetRoomOfGroup(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            Room room = null;
            foreach (Element e in collector)
            {
                room = e as Room;
                if (room != null)
                {
                    //Decide if this point is in the picked room.
                    if (room.IsPointInRoom(point))
                    {
                        break;
                    }
                }
            }

            return room;
        }
        #endregion

        #region GetRoomCenter()
        ///<summary>
        ///Get the room center coordinate.
        ///Z value is equal to the bottom of the room
        /// </summary>
        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);

            return roomCenter;
        }
        #endregion
    }
}
