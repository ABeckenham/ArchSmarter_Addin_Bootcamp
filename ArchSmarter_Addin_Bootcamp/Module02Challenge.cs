#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
    public class Module02Challenge : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            //generate revit elements from model lines


            //prompt to select model lines 
            //first select the current UI in revit
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select Elements");

            

            //filter elements for curves
            List<CurveElement> allCurves = new List<CurveElement>(); 
            foreach (Element elem in pickList)
            {
                if (elem is CurveElement) //if the element is of the curveelement class
                {
                    allCurves.Add(elem as CurveElement);
                }
            }
            
            //do i need the above list?

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
                    CurveElement curveElem = elem as CurveElement;
                    if (curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            TaskDialog.Show("Selected Elements", "  Selected in total: " + pickList.Count.ToString() 
                + "Selected ModelLines: " + modelCurves.Count.ToString());

            foreach(CurveElement curCurve in modelCurves)
            {
                //loop through selection and create revit elements based on line styles
                // collect the geometry of each line and store it in variable curve
                Curve curve = curCurve.GeometryCurve;
                //get the start and end point of each line
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);
                //we have to create variables to store the information about the element
                //and we get that information using a method.
                //???we have to case it from an element type to a specific style? Dont understand why????
                GraphicsStyle curStyle = curCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);

                //Create model elements based on the name of the modelline type
                //A-GLAZ - Storefront wall
                //A-WALL - Generic 8" wall
                //M-DUCT - Default duct
                //P-PIPE - Default pipe
            }




            //BONUS - create a custom method to "get wall type by name", "get system type by name",
            //"create wall", "create pipe", "crate duct"


            return Result.Succeeded;
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
