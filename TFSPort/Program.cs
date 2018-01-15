using System;

namespace TFSPort
{
    class Program
    {        
        static void Main(string[] args)
        {
            Menu();
        }

        private static void Menu()
        {
            Console.Out.WriteLine("!!!!WELCOME TO THE SUPER DUPER TFS TO OP CONVERTER!!!!");

            string targetProjectIdentifier;
            string sourceProjectName;
            string boardColumn;

doover:

            Console.Out.WriteLine("Type the IDENTIFIER for the target open project");
            targetProjectIdentifier = Console.In.ReadLine();

            Console.Out.WriteLine("Type the NAME for the source tfs project");
            sourceProjectName = Console.In.ReadLine();

            Console.Out.WriteLine("Type the BOARD COLUMN NAME for the source tfs project");
            boardColumn = Console.In.ReadLine();

            /*TESTING REMOVE WHEN COMPLETE*/
            if (string.IsNullOrEmpty(targetProjectIdentifier))
            {
                targetProjectIdentifier = "da-tfs-port";
            }

            if (string.IsNullOrEmpty(sourceProjectName))
            {
                sourceProjectName = "DataGovernance";
            }         
            
            if(String.IsNullOrEmpty(boardColumn))
            {
                boardColumn = "Validation";
            }

            Console.Out.WriteLine("Please review your settings, then press enter to begin migration work items from TFS to open project");
            Console.Out.WriteLine("This tool is read-only to TFS, your tfs work items and structure will not be altered");
            Console.Out.WriteLine(string.Format("\r\nTarget Project:    {0}\r\nSourceProject:   {1}\r\nSourceBoardColumn:   {2}", targetProjectIdentifier, sourceProjectName, boardColumn));

            Console.ReadKey();

            Console.Out.WriteLine("Conversion Started");

            try
            {
                opweb op = new opweb();
                op.Init(targetProjectIdentifier, sourceProjectName);
                //op.StartConversion(boardColumn);
                op.RejectWorkItems(630, 652);
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            
            Console.Out.WriteLine("\r\nConversion Completed, Run Again? y/n");
            var response = Console.In.ReadLine();

            if (response.ToLower() == "y")
            {
                goto doover;
            }            
        }
    }
}
