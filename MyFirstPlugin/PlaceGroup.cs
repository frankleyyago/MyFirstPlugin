#region Namespaces
using System;
using System.Collections.Generic;
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
    /// Copy and paste a group of elements to a selected room.
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
                //Define a reference.
                Reference pickedRef = null;

                //Pick a group.
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");
                Element e = doc.GetElement(pickedRef);
                Group group = e as Group;

                //Get the group's center point.
                XYZ origin = GetElementCenter(group);

                //Get the room that the picked group is located in.
                Room room = GetRoomOfGroup(doc, origin);

                //Get the room's center point.
                XYZ sourceCenter = GetElementCenter(room);

                //Prompt the user to select a target room.
                RoomPickFilter roomPickFilter = new RoomPickFilter();
                IList<Reference> rooms = sel.PickObjects(ObjectType.Element, roomPickFilter, "Select target rooms for duplicate furniture group");

                //Place furniture in each of the rooms.
                Transaction tx = new Transaction(doc);
                tx.Start("Lab");                
                PlaceFurnitureInRoom(doc, rooms, sourceCenter, group.GroupType, origin);
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

        #region GroupPickFilter
        /// <summary>
        /// Filter the pick element to allow only groups to be picked/highlighted.
        /// </summary>
        public class GroupPickFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                if (e != null && e.Category != null)
                {
                    return (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups));
                }
                return false;
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
        ///Z value is equal to the bottom of the room.
        /// </summary>
        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);

            return roomCenter;
        }
        #endregion

        #region RoomPickFilter
        ///<summary>
        ///Filter elements to allow only rooms.
        /// </summary>
        public class RoomPickFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                if (e != null && e.Category != null)
                {
                    return (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Rooms));
                }
                return false;
            }
            public bool AllowReference(Reference r, XYZ p)
            {
                return false;
            }
        }
        #endregion

        #region PlaceFurnitureInRoom()
        ///<summary>
        ///Copy the selected group to the selected rooms.
        /// </summary>
        public void PlaceFurnitureInRoom(Document doc, IList<Reference> rooms, XYZ sourceCenter, GroupType gt, XYZ groupOrigin)
        {
            XYZ offset = groupOrigin - sourceCenter;
            XYZ offsetXY = new XYZ(offset.X, offset.Y, 0);
            foreach (Reference r in rooms)
            {
                Room roomTarget = doc.GetElement(r) as Room;
                if (roomTarget != null)
                {
                    XYZ roomCenter = GetRoomCenter(roomTarget);
                    Group group = doc.Create.PlaceGroup(roomCenter + offsetXY, gt);
                }
            }
        }
        #endregion
    }
}
