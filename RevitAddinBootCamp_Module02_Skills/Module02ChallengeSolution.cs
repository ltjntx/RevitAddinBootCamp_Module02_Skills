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
using System.Net;
using System.Reflection;
using System.Windows.Media;

#endregion

namespace RAB_Module02_Challenge_Solution
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. prompt user to select elements
            TaskDialog.Show("Select Lines", "Select some line to convert to Revit elements.");
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select some elements");

            // 2. filter selected elements
            List<CurveElement> filteredList = new List<CurveElement>();
            foreach (Element element in pickList)
            {
                if (element is CurveElement)
                {
                    //CurveElement curve = (CurveElement) element;
                    CurveElement curve = element as CurveElement;
                    filteredList.Add(curve);
                }
            }

            TaskDialog.Show("Curves", $"You selected {filteredList.Count} lines");

            // 3. Get level and various types
            Parameter levelParam = doc.ActiveView.LookupParameter("Associated Level");
            Level currentLevel = GetLevelByName(doc, levelParam.AsString());

            // 4. Get types
            WallType wt1 = GetWallTypeByName(doc, "Storefront");
            WallType wt2 = GetWallTypeByName(doc, "Generic - 8\"");

            MEPSystemType ductSystemType = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType ductType = GetDuctTypeByName(doc, "Default");

            MEPSystemType pipeSystemType = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType pipeType = GetPipeTypeByName(doc, "Default");

            List<ElementId> linesToHide = new List<ElementId>();

            // 5. Loop through selected CurveElements
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements");
                foreach (CurveElement currentCurve in filteredList)
                {
                    // 6. Get GraphicStyle and Curve for each CurveElement
                    Curve elementCurve = currentCurve.GeometryCurve;
                    GraphicsStyle currentStyle = currentCurve.LineStyle as GraphicsStyle;

                    // 7. Skip arcs
                    //if (elementCurve.IsBound == false)
                    //    continue;

                    // 8. Get start and end points
                    //XYZ startPoint = elementCurve.GetEndPoint(0);
                    //XYZ endPoint = elementCurve.GetEndPoint(1);

                    // 9. Use Switch statement to create walls, ducts, and pipes
                    switch (currentStyle.Name)
                    {
                        case "A-GLAZ":
                            Wall currentWall = Wall.Create(doc, elementCurve, wt1.Id, currentLevel.Id, 20, 0, false, false);
                            break;

                        case "A-WALL":
                            Wall currentWall2 = Wall.Create(doc, elementCurve, wt2.Id, currentLevel.Id, 20, 0, false, false);
                            break;

                        case "M-DUCT":
                            Duct currentDuct = Duct.Create(doc, ductSystemType.Id,
                                ductType.Id, currentLevel.Id, elementCurve.GetEndPoint(0),
                                elementCurve.GetEndPoint(1));
                            break;

                        case "P-PIPE":
                            Pipe currentPipe = Pipe.Create(doc, pipeSystemType.Id,
                                pipeType.Id, currentLevel.Id, elementCurve.GetEndPoint(0),
                                elementCurve.GetEndPoint(1));
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