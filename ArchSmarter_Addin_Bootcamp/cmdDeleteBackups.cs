#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Windows.Forms;
using System.IO; 

#endregion

namespace ArchSmarter_Addin_Bootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDeleteBackups : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //set variables
            int counter = 0;
            string logPath = "";


            //create list for logfile 
            List<string> deletedFileLog = new List<string>();
            deletedFileLog.Add("The following backup files have been deleted:");

            FolderBrowserDialog selectfolder = new FolderBrowserDialog();
            selectfolder.ShowNewFolderButton = false;

            //Open folder dialog and only run code if a folder is selected
            if (selectfolder.ShowDialog() == DialogResult.OK)
            {
                // get the selected folder path
                string directory = selectfolder.SelectedPath;

                // get all files from selected folder

                string[] files = Directory.GetFiles(directory,"*.*", SearchOption.AllDirectories);

                //loop through files
                foreach (string file in files)
                { 
                    // check if file is a revit file 
                    if (Path.GetExtension(file)== ".rvt" || Path.GetExtension(file) == ".rfa")
                    {
                        string checkString = file.Substring(file.Length - 9, 9);
                        if(checkString.Contains(".00")== true)
                        {
                            // add file name to list
                            deletedFileLog.Add(file);

                            //delete file
                            File.Delete(file);

                            //increment counter
                            counter++;
                        }
                    }
                }
                //output logfile
                if (counter>0)
                {
                    logPath = WriteListToTxt(deletedFileLog, directory);
                }
            }

            //alert the user
            TaskDialog td = new TaskDialog("Complete");
            td.MainInstruction = "Deleted " + counter.ToString() + "Backup files.";
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Click to view log file");
            td.CommonButtons = TaskDialogCommonButtons.Ok;

            TaskDialogResult result = td.Show();
            
            if(result == TaskDialogResult.CommandLink1)
            {
                Process.Start(logPath); 
            }

            return Result.Succeeded;
        }

        internal string WriteListToTxt(List<string> stringList, string filePath)
        {
            string fileName = "_Delete Backup Files.text";
            string fullPath = filePath + @"\" + fileName;

            File.WriteAllLines(fullPath, stringList);

            return fullPath; 
        }
    }
}
