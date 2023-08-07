#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Controls;

#endregion

namespace ArchSmarter_Addin_Bootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class Module02Challenge : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            // below is needed because you could technically have multiple documents open at once.
            // So this variable gets the current active doc
            Document doc = uiapp.ActiveUIDocument.Document;            
            //prompt to select model lines 
            //first select the current UI in revit
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select Elements");            

            //filter elements to find just curves
            //List<CurveElement> allCurves = new List<CurveElement>(); 
            //foreach (Element elem in pickList)
            //{
                //if (elem is CurveElement) //if the element is of the curveelement class
                //{
                //    allCurves.Add(elem as CurveElement);
               // }
           // }
            
            //do i need the above list? i think no

            //filter more specificially 

            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element elem in pickList)
            {
                if(elem is CurveElement)
                {
                    // cast Element type to a CurveElement type 
                    // they already are Curve elements because of the If Statement
                    // casting them to their more specific CurveElement Type gives us
                    // access different parameters
                   // CurveElement curveElem = (CurveElementr)elem;
                   
                    CurveElement curveElem = elem as CurveElement;
                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        Curve currentCurve = curveElem.GeometryCurve;
                                                
                        modelCurves.Add(curveElem);
                                               
                    }
                }
            }

            TaskDialog.Show("Selected Elements", "  Selected in total: " + pickList.Count.ToString() 
                + "Selected ModelLines: " + modelCurves.Count.ToString());

            FilteredElementCollector projlevels = new FilteredElementCollector(doc);
            projlevels.OfClass(typeof(Level));

            FilteredElementCollector projDuct = new FilteredElementCollector(doc);
            projDuct.OfClass(typeof(DuctType));

            DuctType ductType = null;
            foreach (DuctType curDuct in projDuct)
            {
                if (curDuct.Name == "Default")
                {
                    ductType = curDuct;
                    break;
                }
            }

            FilteredElementCollector projPipe = new FilteredElementCollector(doc);
            projPipe.OfClass(typeof(PipeType));

            PipeType pipeType = null;
            foreach (PipeType curPipe in projPipe)
            {
                if (curPipe.Name == "Default")
                {
                    pipeType = curPipe;
                    break;
                }
            }

            WallType genWall = GetWallTypeByName(doc, @"Generic - 8""");
            WallType storeWall = GetWallTypeByName(doc, "Storefront");
            MEPSystemType ductSystemType = GetSystemTypeByName(doc, "Supply Air");
            MEPSystemType pipeSystemType = GetSystemTypeByName(doc, "Domestic Hot Water");

            //create transaction with using statement

            using (Transaction t = new Transaction(doc))
            {
                t.Start("create revit elements");                                                               

                foreach (CurveElement curCurve in modelCurves)
                {
                    Curve curve = curCurve.GeometryCurve;                    
                    GraphicsStyle curStyle = curCurve.LineStyle as GraphicsStyle;

                    switch (curStyle.Name)
                    {
                        case "A-GLAZ":
                            Wall.Create(doc, curve, storeWall.Id, projlevels.FirstElement().Id, 20, 0, false, false);
                            break;

                        case "A-WALL":
                            Wall.Create(doc, curve, genWall.Id, projlevels.FirstElement().Id, 20, 0, false, false);
                            break;

                        case "M-DUCT":
                            XYZ startPointd = curve.GetEndPoint(0);
                            XYZ endPointd = curve.GetEndPoint(1);
                            Duct.Create(doc, ductSystemType.Id, ductType.Id, projlevels.FirstElement().Id, startPointd, endPointd);
                            break;

                        case "P-PIPE":
                            XYZ startPointp = curve.GetEndPoint(0);
                            XYZ endPointp = curve.GetEndPoint(1);
                            Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, projlevels.FirstElement().Id, startPointp, endPointp);
                            break;

                        default:
                            break;
                    }
                }
                t.Commit();
                                
            }                  
            //BONUS - create a custom method to "get wall type by name", "get system type by name", DONE
            //"create wall", "create pipe", "crate duct" 
            return Result.Succeeded;
        }

        //my methods below
        internal WallType GetWallTypeByName(Document doc, String typeName)
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


        internal MEPSystemType GetSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));
                        
            foreach (MEPSystemType curType in collector)
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
            string buttonInternalName = "btnModule02Challenge.cs";
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
