#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

#endregion

namespace RevitAddinBootCamp_Module02_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            /*** ATTN: Michael: This is your code, that I re-typed while attempting to understand what each block of code is doing. 
             *** There is NO WAY I would have been able to come up with ANY solution that would have worked for this exercise, at this time.
             *** I will keep practicing and will get this, eventually. ***/

            // 1. Pick elements and filter them into a list. 
            TaskDialog.Show("Select Lines", "Please select lines to convert to Revit elements.");
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("Please select elements with a window/crossing");

            //2. Filter selected elements for curves.
            List<CurveElement> FilteredList = new List<CurveElement>();
            foreach (Element element in PickList)
            {
                if (element is CurveElement)
                {
                    //or this can be used: CurveElement curveElem = (CurveElement) element;
                    CurveElement curveElem = element as CurveElement;  // Casting elements in PickList as CurveElement.
                    FilteredList.Add(curveElem);
                }
            }

            TaskDialog.Show("Curves", $"You selected {FilteredList.Count} lines.");

            // 3. Get level and various types.
            Parameter LevelParam = doc.ActiveView.LookupParameter("Associated Level");
            Level currentLevel = GetLevelByName(doc, LevelParam.AsString());

            // 4. Get types.
            WallType wallType1 = GetWallTypeByName(doc, "Storefront");
            WallType wallType2 = GetWallTypeByName(doc, "Generic - 8\"");

            MEPSystemType ductSystemType = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType ductType = GetDuctTypeByName(doc, "Default");

            MEPSystemType pipeSystemType = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType pipeType = GetPipeTypeByName(doc, "Default");

            List<ElementId> linesToHide = new List<ElementId>();

            // 5. Loop through selected CurveElements.
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements");
                foreach (CurveElement currentCurve in FilteredList)
                {
                    // 6. Get GraphicSytle & Curve for each CurveElement.
                    Curve elementCurve = currentCurve.GeometryCurve;
                    GraphicsStyle currentStyle = currentCurve.LineStyle as GraphicsStyle;

                    // 7. Skip arcs.
                    //if (elementCurve.IsBound == flase)
                      //  continue;

                    // 8. Get start & end points.
                    //XYZ startPoint = elementCurve.GetEndPoint(0);
                    //XYZ endPoint = elementCurve.GetEndPoint(1);

                    // 9. Use Switch Statement to create walls, ducts, & pipes.
                    switch (currentStyle.Name)
                    {
                        case "A-GLAZ":
                            Wall currentWall = Wall.Create(doc, elementCurve, wallType1.Id, currentLevel.Id, 20, 0, false, false); 
                            break;

                        case "A-WALL":
                            Wall currentWall1 = Wall.Create(doc, elementCurve, wallType2.Id, currentLevel.Id, 20, 0, false, false);
                            break;

                        case "M-DUCT":
                            Duct currentDuct = Duct.Create(doc, ductSystemType.Id, ductType.Id, currentLevel.Id,
                                elementCurve.GetEndPoint(0), elementCurve.GetEndPoint(1));
                            break;

                        case "P-PIPE":
                            Pipe currentPipe = Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, currentLevel.Id,
                                elementCurve.GetEndPoint(0), elementCurve.GetEndPoint(1));
                            break;

                        default:
                            linesToHide.Add(currentCurve.Id);
                            break;
                    }

                }

                doc.ActiveView.HideElements(linesToHide);

                t.Commit();
            }


            return Result.Succeeded;
        }

        private PipeType GetPipeTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType curType in collector)
            {
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType curType in collector)
            {
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }

        private MEPSystemType GetMEPSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType curType in collector)
            {
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }

        private WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curType in collector)
            {
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }

        private Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach (Level curLevel in collector)
            {
                if (curLevel.Name == levelName)
                    return curLevel;
            }

            return null;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
