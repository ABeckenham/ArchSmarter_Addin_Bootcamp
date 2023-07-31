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
using System.Reflection;

#endregion

namespace ArchSmarter_Addin_Bootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Demo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here

            //1. pick elements and filter them into list
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> picklist = uidoc.Selection.PickElementsByRectangle("Select elements");

            TaskDialog.Show("Test", "I selected " + picklist.Count.ToString() + " elements");

            //2. filter selected elements for curves
            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element elem in picklist)
            {
                if(elem is CurveElement)
                {
                    allCurves.Add(elem as CurveElement);
                }
            }

            //2b. filter selected elements for model curves
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in picklist)
            {
                if (elem is CurveElement)
                {
                    //casing both options below work, top one is Michaels fave
                    CurveElement curveElem = elem as CurveElement; 
                    //CurveElement curveElem = (CurveElement) elem;

                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                    //is this to filter out none model elements ??
                }
            }

            //3. curve data
            foreach (CurveElement currentCurve in modelCurves)
            { 
                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0); // gets start point
                XYZ endPoint = curve.GetEndPoint(1); //gets end point

                GraphicsStyle curStyle = currentCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            //5. create transaction with using statement
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Revit Elements");
                //4. create wall
                Level newLevel = Level.Create(doc, 20);
                Curve curCurve1 = modelCurves[0].GeometryCurve;

                //Wall.Create(doc, curCurve1, newLevel.Id, false);

                FilteredElementCollector wallTypes = new FilteredElementCollector(doc);
                wallTypes.OfClass(typeof(WallType));

                Curve curCurve2 = modelCurves[1].GeometryCurve;
                WallType myWallType = GetWallTypeByName(doc, "Wall-Ext_102Bwk-75Ins-100LBlk-12P");
                Wall.Create(doc, curCurve2, myWallType.Id, newLevel.Id, 20, 0, false, false);

                //6. get system types
                FilteredElementCollector systemCollector = new FilteredElementCollector(doc);
                systemCollector.OfClass(typeof(MEPSystemType));

                //7. get duct system type
                MEPSystemType ductSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Supply Air")
                    {
                        ductSystemType = curType;
                        break;
                    }
                }

                               
                //8. get duct type

                FilteredElementCollector collector1 = new FilteredElementCollector(doc);
                collector1.OfClass(typeof(DuctType));


                //9. create duct
                Curve curCurve3 = modelCurves[2].GeometryCurve;
                Duct newDuct = Duct.Create(doc,ductSystemType.Id, collector1.FirstElementId(), newLevel.Id,
                    curCurve3.GetEndPoint(0), curCurve3.GetEndPoint(1));


                //10. get pipe system type
                MEPSystemType pipeSystemType = null;
                foreach (MEPSystemType curType in systemCollector)
                {
                    if (curType.Name == "Domestic Hot Water")
                    {
                        pipeSystemType = curType;
                        break;
                    }
                }

                //11. get pipe type
                FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                collector2.OfClass(typeof(PipeType));

                //12. create pipe
                Curve curCurve4 = modelCurves[3].GeometryCurve;
                Pipe newPipe = Pipe.Create(doc, pipeSystemType.Id, collector2.FirstElementId(), newLevel.Id,
                    curCurve4.GetEndPoint(0), curCurve4.GetEndPoint(1));

                //13. use our new methods
                string teststring = MyFirstMethod();
                MySecondMethod();
                string testString2 = MyThirdMethod("Hello world");

                //15. switch statement
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

                //16. advanced switch statements
                Curve curve5 = modelCurves[1].GeometryCurve;
                GraphicsStyle curve5GS = modelCurves[1].LineStyle as GraphicsStyle;

                WallType walltype1 = GetWallTypeByName(doc, "Wall-Partn_12P-75Std-12P");
                WallType walltype2 = GetWallTypeByName(doc, "Wall-Ext_215Bwk");

                switch (curve5GS.Name)
                {
                    case "<Thin Lines>":
                        Wall.Create(doc, curve5, walltype1.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    case "<Wide Lines>":
                        Wall.Create(doc, curve5, walltype2.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    default:
                        Wall.Create(doc, curve5, newLevel.Id, false);
                        break;

                }

                t.Commit();
            }
                                   

            return Result.Succeeded;
        }

        //creating a method
        internal string MyFirstMethod()
        {
            return "This is my first method";
        }

        internal void MySecondMethod()
        {
            Debug.Print("This is my second method!");

        }
        internal string MyThirdMethod(string input)
        {
           return "This is my third method: " + input;

        }
                
        internal WallType GetWallTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType curType in collector)
            {
                if (curType.Name == typeName)
                {
                    return curType;
                }
            }
            return null;
        }
     

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnModule02Demo";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
