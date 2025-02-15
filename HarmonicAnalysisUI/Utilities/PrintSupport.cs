using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HarmonicAnalysisUI.Utilities;

// copied from C:\Users\philip\source\repos\PitchTypes\Staff\Utilities\PrintSupport.cs

/// <summary>
/// Print to XPS, then use a free XPS to SVG converter to save to SVG
/// </summary>
public class PrintSupport
{
    // https://stackoverflow.com/questions/40600087/get-a-list-of-every-printqueue-in-c-sharp-for-printdialog
    public static void Print(Window MyMainWindow)
    {
        PrintQueue printQueue = GetAllPrinterQueues().First();
        var printDialog = new PrintDialog { PrintQueue = printQueue };
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(MyMainWindow, "description");
        }
    }
    public static void Print(Page MyMainWindow)
    {
        PrintQueue printQueue = GetAllPrinterQueues().First();
        var printDialog = new PrintDialog { PrintQueue = printQueue };
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(MyMainWindow, "description");
        }
    }
    public static IEnumerable<PrintQueue> GetAllPrinterQueues()
    {
        using (var printServer = new LocalPrintServer())
        {
            foreach (string printerName in PrinterSettings.InstalledPrinters.OfType<string>())
            {
                var match = Regex.Match(printerName, @"(?<machine>\\\\.*?)\\(?<queue>.*)");
                //var match = Regex.Match(printerName, @"XPS");
                yield return match.Success ?
                    new PrintServer(match.Groups["machine"].Value).GetPrintQueue(match.Groups["queue"].Value) :
                    printServer.GetPrintQueue(printerName);
            }
        }
    }
    public static void PrintXPS()
    {
        // Create print server and print queue.
        LocalPrintServer localPrintServer = new LocalPrintServer();
        PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();

        // Prompt user to identify the directory, and then create the directory object.
        Console.Write("Enter the directory containing the XPS files: ");
        String directoryPath = Console.ReadLine();
        DirectoryInfo dir = new DirectoryInfo(directoryPath);

        // If the user mistyped, end the thread and return to the Main thread.
        if (!dir.Exists)
        {
            Console.WriteLine("There is no such directory.");
        }
        else
        {
            // If there are no XPS files in the directory, end the thread
            // and return to the Main thread.
            if (dir.GetFiles("*.xps").Length == 0)
            {
                Console.WriteLine("There are no XPS files in the directory.");
            }
            else
            {
                Console.WriteLine("\nJobs will now be added to the print queue.");
                Console.WriteLine("If the queue is not paused and the printer is working, jobs will begin printing.");

                // Batch process all XPS files in the directory.
                foreach (FileInfo f in dir.GetFiles("*.xps"))
                {
                    String nextFile = directoryPath + "\\" + f.Name;
                    Console.WriteLine("Adding {0} to queue.", nextFile);

                    try
                    {
                        // Print the Xps file while providing XPS validation and progress notifications.
                        PrintSystemJobInfo xpsPrintJob = defaultPrintQueue.AddJob(f.Name, nextFile, false);
                    }
                    catch (PrintJobException e)
                    {
                        Console.WriteLine("\n\t{0} could not be added to the print queue.", f.Name);
                        if (e.InnerException.Message == "File contains corrupted data.")
                        {
                            Console.WriteLine("\tIt is not a valid XPS file. Use the isXPS Conformance Tool to debug it.");
                        }
                        Console.WriteLine("\tContinuing with next XPS file.\n");
                    }
                }// end for each XPS file
            }//end if there are no XPS files in the directory
        }//end if the directory does not exist

        Console.WriteLine("Press Enter to end program.");
        Console.ReadLine();
    }// end PrintXPS method
}
