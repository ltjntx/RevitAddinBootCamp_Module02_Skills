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
using System.Net;
using System.Reflection;
using System.Windows;

#endregion

namespace RevitAddinBootCamp_Module02_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here.

            //**** Revit API ****   PickElementsByRectangle Method (String)
            /*IList<Element> pickedElements = uidoc.Selection.PickElementsByRectangle("Select by rectangle");
            if (pickedElements.Count > 0)
            {
                // Collect Ids of all picked elements
                IList<ElementId> idsToSelect = new List<ElementId>(pickedElements.Count);
                foreach (Element element in pickedElements)
                {
                    idsToSelect.Add(element.Id);
                }

                // Update the current selection
                uidoc.Selection.SetElementIds(idsToSelect);
                TaskDialog.Show("Revit", string.Format("{0} elements added to Selection.", idsToSelect.Count));
            }*/
        

            // 1. Pick elements and filter them into list.
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("Please select elements");
            IList<Element> PickedElements = uidoc.Selection.PickElementsByRectangle("Select by rectangle");
            if (PickedElements.Count > 0)
            {
                // Collect Ids of all picked elements
                IList<ElementId> IdsToSelect = new List<ElementId>(PickedElements.Count);
                foreach (Element element in PickedElements)
                {
                    IdsToSelect.Add(element.Id);
                }

                // Update the current selection
                uidoc.Selection.SetElementIds(IdsToSelect);
                TaskDialog.Show("Revit", string.Format("{0} elements added to Selection.", IdsToSelect.Count));
            }

            TaskDialog.Show("Test", "You selected " + PickList.Count.ToString() + " elements.");

            // 2. Filter selected elements for curves.
            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element element in PickList)
            {
                if (element is CurveElement)
                {
                    allCurves.Add(element as CurveElement);
                }
            }

            //2b. Filter selected elements for model curves.
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element element in PickList)
            {
                if (element is CurveElement)
                {
                    CurveElement curveElem = element as CurveElement;
                    //CurveElement curveElem = (CurveElement) element;

                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }

                }
            }

            // 3. Curve data.
            foreach (CurveElement currentCurve in modelCurves)
            {
                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);

                GraphicsStyle curStyle = currentCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            // 4. Create transaction with a "Using Statement".  The Using statement doesn't require a Dispose statement at the end.
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements.");

                // 5. Create Wall (Create ARCH System wall types).
                Level newLevel = Level.Create(doc, 20);
                Curve curCurve1 = modelCurves[0].GeometryCurve;

                //Wall.Create(doc, curCurve1, newLevel.Id, false);

                FilteredElementCollector wallTypes = new FilteredElementCollector(doc);
                wallTypes.OfClass(typeof(WallType));

                Curve curCurve2 = modelCurves[1].GeometryCurve;
                WallType myWallType = GetWallTypeByName(doc, "Exterior - Brick on CMU");
                Wall.Create(doc, curCurve2, myWallType.Id, newLevel.Id, 20, 0, false, false);


                // 6. Get MEP System Types.
                FilteredElementCollector SystemCollector = new FilteredElementCollector(doc);
                SystemCollector.OfClass(typeof(MEPSystemType));

                // 7. Get Duct System Type (Looping through the collector).
                MEPSystemType DuctSystemType = null;
                foreach (MEPSystemType CurType in SystemCollector)
                {
                    if (CurType.Name == "Supply Air")
                    {
                        DuctSystemType = CurType;
                        break;
                    }

                }

                // 8. Get Duct Type (Gets first duct type available).
                FilteredElementCollector DuctCollector = new FilteredElementCollector(doc);
                DuctCollector.OfClass(typeof(DuctType));

                // 9. Create Duct.
                // *** Revit API - Duct Create Method*** 
                /* Duct Create(Document document, ElementId systemTypeId, ElementId ductTypeId, ElementId levelId, XYZ startPoint, XYZ endPoint)*/
                Curve curCurve3 = modelCurves[2].GeometryCurve;
                Duct newDuct = Duct.Create(doc, DuctSystemType.Id, DuctCollector.FirstElementId(), newLevel.Id,
                    curCurve3.GetEndPoint(0), curCurve3.GetEndPoint(1));


                // 10. Get Pipe System Type.
                MEPSystemType PipeSystemType = null;
                foreach (MEPSystemType CurType in SystemCollector)
                {
                    if (CurType.Name == "Domestic Hot Water")
                    {
                        PipeSystemType = CurType;
                        break;
                    }
                }

                // 11. Get Pipe Type.
                FilteredElementCollector PipeCollector = new FilteredElementCollector(doc);
                PipeCollector.OfClass(typeof(PipeType));

                // 12. Create Pipe.
                Curve curCurve4 = modelCurves[3].GeometryCurve;
                Pipe.Create(doc, PipeSystemType.Id,
                     PipeCollector.FirstElementId(), newLevel.Id, curCurve4.GetEndPoint(0), curCurve4.GetEndPoint(1));


                // 13. Use my new methods.
                string testString = MyFirstMethod();
                MySecondMethod();
                string testString2 = MyThirdMethod("Hello World!");

                // 14. Switch Statement.
                int numberValue = 5;
                string numAsString = "";

                switch (numberValue)
                {
                    case 1:
                        numAsString = "One";
                        break;

                    case 2:
                        numAsString = "Two";
                        break;

                    case 3:
                        numAsString = "Three";
                        break;

                    case 4:
                        numAsString = "Four";
                        break;

                    case 5:
                        numAsString = "Five";
                        break;

                    default:
                        numAsString = "Zero";
                        break;

                }

                // 15. Advanced Switch Statement.
                Curve curve5 = modelCurves[4].GeometryCurve;
                GraphicsStyle curve5GS = modelCurves[1].LineStyle as GraphicsStyle;

                WallType wallType1 = GetWallTypeByName(doc, "Storefront");
                WallType wallType2 = GetWallTypeByName(doc, "Exterior - Brick on CMU");

                switch (curve5GS.Name)
                {
                    case "<Thin Lines>":
                        Wall.Create(doc, curve5, wallType1.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    case "<Wide Lines>":
                        Wall.Create(doc, curve5, wallType2.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    default:
                        Wall.Create(doc, curve5, newLevel.Id, false);
                        break;
                }

                t.Commit();

            }

            
            return Result.Succeeded;
        }


        internal string MyFirstMethod()
        {
            return "This is my first method!";
        }

        internal void MySecondMethod()
        {
            Debug.Print("This is my second method!");
        }

        internal string MyThirdMethod(string input)
        {
            return "This is my third method: " + input;
        }

        internal WallType GetWallTypeByName(Document doc, string TypeName)
        {
            FilteredElementCollector WallCollector = new FilteredElementCollector(doc);  //Gets all wall types in the project.
            WallCollector.OfClass(typeof(WallType));

            foreach (WallType CurType in WallCollector)  //Iterates through the wall types.
            {
                if (CurType.Name == TypeName)  //Conditional statement if true.
                {
                    return CurType;
                }
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