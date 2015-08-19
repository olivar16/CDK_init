using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Data;
using System.Windows.Forms;

/*
* Author: Paul Ryan Olivar, Windows Engineering Intern
* Date: August 2015
* Program Description: This program is as a deprecation of CDKinit (RIP). It contains genericized functionalities that aid
*                      in the user environment setup process. 
*
* Arguments:
* -config <configFilePath> *Required argument where a valid, existing .XML file path must be specified*
* -log <logFilePath>  *Optional argument where a valid, existing .txt file path must be specified.*
* 
* Execution:
* 1. Iterator object created
* 2. Nodes are parsed from XML file
* 3. ObjectFactory object is created out of each now
* 4. Necessary functions are called for created ObjectFactory object
*
*/

namespace CDKinit
{
    class Program
    {
        static void Main(string[] args)
        {
            //store paths fo config and log files
            int argc = args.Length;
            string configFilePath = "";
            string logFilePath = "";

            //Iterator object to parse through XML
            Iterator test;

            //Get arguments
            for (int i = 0; i < argc; i+=2)
            {
                if (args[i].Substring(0, 1) == "-" && args[i + 1]!=null)
                {
                    switch (args[i])
                    {
                        case "-config":
                            configFilePath = args[i + 1];
                            break;
                        case "-log":
                            logFilePath = args[i + 1];
                            break;
                    }
                }
            }
           
            //Check if required config file path is specified
            if (System.IO.File.Exists(configFilePath))
            {

                //Call iterator constructor depending on if a log path was declared
                if (System.IO.File.Exists(logFilePath))
                {
                    test = new Iterator(configFilePath, logFilePath);
                }
                else
                {
                   //Create Iterator object without logging enabled
                    test = new Iterator(configFilePath);
                }
            }
            else
            {
                MessageBox.Show("ERROR: You must specifiy a valid .XML config file");
                Environment.Exit(1);
            }
            Console.Read();
        }

    }

    /*
    *Name: IniFile
    *Description: Class that represents an IniFile to be modified
    */
    public class IniFile : BaseType
    {

        //Store values parsed from XML
        public string path;
        public string pathRoot;
        public string PathPostRoot;
        public string section;
        public string key;
        public string value;

        //Write to Ini
        public override void create()
        {
            WritePrivateProfileString(this.section, this.key, this.value, this.path);
        }

        //Write to Ini with logging
        public override void create(string logFilepath)
        {
            log(logFilepath, "Setting values in IniFile: " + path);
            WritePrivateProfileString(this.section, this.key, this.value, this.path);
        }

        //unimplemented methods. Add as you please
        public override void update()
        {

        }

        public override void delete()
        {

        }

        public override void printAttrs()
        {
            Console.WriteLine("This IniFile has path " + path);
            Console.WriteLine("And section " + section);
            Console.WriteLine("And key " + key);
            Console.WriteLine("And value " + value);
        }

        //Log execution info to .txt file
        public override void log(string file, string entry)
        {
            StreamWriter writer = new StreamWriter(file, true);
            writer.WriteLine(entry);
            writer.Close();
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        /*
        *Constructor
        *Arguments:
        1. Pathroot - Root of the filepath containing Ini File
        2. Postroot - Last file in the end of filepath
        3.Section - Section of the Ini File
        4. Key - part of Key-value pair
        5. Value = Part of Key-value pair
        */
        public IniFile(string Pathroot, string Postroot, string section, string key, string value)
        {
            string fileRoot = "";

            //Note: this only extracts root paths for Documents and AppData. Add cases as necessary.
            switch (Pathroot)
            {
                case "Documents":
                    fileRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case "AppData":
                    fileRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
            }

            path = fileRoot + @"\" + Postroot;
            this.section = section;
            this.key = key;
            this.value = value;
        }


    }

    /*
    * Name: Registry
    * Description: Class that represents registry value to be modified
    */
    public class Registry : BaseType {

        public string key = "";
        public string name = "";
        public Object value;
        RegistryValueKind regValueKind;
        Boolean hasValueKind;

        public Registry(string key, string name, string value, string valueKindString)
        {
            this.key = key;
            this.name = name;
            this.value = value;
            this.hasValueKind = true;


            if (valueKindString == "Binary")
            {
                regValueKind = RegistryValueKind.Binary;
                this.value = value;
            }
            else if (valueKindString == "DWord")
            {
                regValueKind = RegistryValueKind.DWord;
                this.value = Int32.Parse(value);
            }
            else if (valueKindString == "ExpandString")
            {
                regValueKind = RegistryValueKind.ExpandString;
                this.value = value;
            }
            else if (valueKindString == "MultiString")
            {
                regValueKind = RegistryValueKind.MultiString;
                this.value = value;
            }
            else if (valueKindString == "None")
            {
                regValueKind = RegistryValueKind.None;
                this.value = value;
            }
            else if (valueKindString == "QWord")
            {
                regValueKind = RegistryValueKind.QWord;
                this.value = Int64.Parse(value);
            }
            else if (valueKindString == "String")
            {
                regValueKind = RegistryValueKind.String;
                this.value = value;
            }
            else if (valueKindString == "Unknown")
            {
                regValueKind = RegistryValueKind.Unknown;
                this.value = value;
            }


        }

        public Registry(string key, string name, string value) {
            this.key = key;
            this.name = name;
            this.value = value;
            this.hasValueKind = false;
        }

        public override void create()
        {
            if (hasValueKind == true)
            {
                Microsoft.Win32.Registry.SetValue(this.key, this.name, this.value, regValueKind);

            }
            else
            {
                Microsoft.Win32.Registry.SetValue(this.key, this.name, this.value);
            }
        }

        public override void create(string logFilePath)
        {

            log(logFilePath, "Setting Registry value at " + this.key);

            if (hasValueKind == true)
            {     
                try {
                    Microsoft.Win32.Registry.SetValue(this.key, this.name, this.value, regValueKind);
                }
                catch (Exception)
                {
                    log(logFilePath, "ERROR: Unable to set registry value at " + this.key);
                }

            }
            else
            {

                try {
                    Microsoft.Win32.Registry.SetValue(this.key, this.name, this.value);
                }
                catch (Exception)
                {
                    log(logFilePath, "ERROR: Unable to set registry value at " + this.key);
                }
            }
        }

        public override void update()
        {

        }

        public override void delete()
        {

        }

        public override void printAttrs()
        {
            Console.WriteLine("This Registry has key " + key);
            Console.WriteLine("With name " + name);
        }

        //Log execution info to .txt file
        public override void log(string file, string entry)
        {
            StreamWriter writer = new StreamWriter(file, true);
            writer.WriteLine(entry);
            writer.Close();
        }


        


    }

    /*
    *Name: File
    *Description: Class that represents a file in the user's file system to be modified
    */
    public class File : BaseType {
        string currDir = Directory.GetCurrentDirectory();
        string destPath = "";
        string backupPath = "";
        string userContainingDirectory = "";
        string userFileRoot = "";
        string fileName = "";
        string logFilePath = "";

        public File(string root, string name, string userContainingDirectory, string BackupContainingDirectory)
        {
            string fileRoot = "";
            Console.WriteLine("The given name is " + name);

            //Note: this only extracts root paths for Documents and AppData. Add cases as necessary.
            switch (root)
            {
                case "Documents":
                    fileRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case "AppData":
                    fileRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
            }
            Console.WriteLine("Dest path is with name " + name);
            this.destPath = fileRoot + @"\" + userContainingDirectory + @"\" + name;
            this.backupPath = currDir + @"\" + BackupContainingDirectory + @"\" + name;
            this.userContainingDirectory = userContainingDirectory;
            this.userFileRoot = fileRoot;
            this.fileName = name;
        }

        public override void create()
        {
            //create containing directory in the path regardless of whether or not it exists
            createDirectory(userFileRoot + @"\" + userContainingDirectory);

            Console.WriteLine("Checking if " + destPath + " exists");
            if (System.IO.File.Exists(destPath))
            {
                //it exists!
                Console.WriteLine(destPath + " already exists");
            }
            else
            {
                Console.WriteLine("Copying File now");
                copyFile(backupPath, userFileRoot + @"\" + userContainingDirectory+@"\"+fileName);
            }
        }

        public override void create(string logFilePath)
        {

            this.logFilePath = logFilePath;
            //create containing directory in the path regardless of whether or not it exists
            createDirectory(userFileRoot + @"\" + userContainingDirectory);
            log(logFilePath, "Created a Directory at " + userFileRoot + @"\" + userContainingDirectory);

            Console.WriteLine("Checking if " + destPath + " exists");
            if (System.IO.File.Exists(destPath))
            {
                //it exists!
                Console.WriteLine(destPath + " already exists");
                log(logFilePath, destPath + " exists");
            }
            else
            {
                
                Console.WriteLine("Copying File now");
               if( copyFile(backupPath, userFileRoot + @"\" + userContainingDirectory + @"\" + fileName) == true)
                {
                    log(logFilePath,"Successfully copied " + backupPath + " to " + userFileRoot + @"\" + userContainingDirectory + @"\" + fileName);
                }
               else
                {
                    log(logFilePath, "Failed to copy " + backupPath + " to " + userFileRoot + @"\" + userContainingDirectory + @"\" + fileName);
                }
            }
        }

        public override void update()
        {

        }

        public override void delete()
        {

        }

        public override void printAttrs()
        {
            Console.WriteLine("This File has destPath " + destPath);
            Console.WriteLine("And backupPath " + backupPath);

        }

        //Log execution info to .txt file
        public override void log(string file, string entry)
        {
            StreamWriter writer = new StreamWriter(file, true);
            writer.WriteLine(entry);
            writer.Close();
        }


        
        //Copy file from given source to given destination
        private bool copyFile(string sourcePath, string destPath)
        {
            
            if (System.IO.File.Exists(sourcePath))
            {
                try
                {
                    System.IO.File.Copy(sourcePath, destPath);
                    Console.WriteLine(sourcePath + "successfully copied to " + destPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
                return true;
            }
            else
            {
                MessageBox.Show("File/s not found");
                return false;
            }

        }

        //Creates directory in desired path
        public void createDirectory(string dirPath)
        {
            DirectoryInfo inf = Directory.CreateDirectory(dirPath);

            //If directory was successfully created, log it
            if (inf.Exists == true)
            {
                log(this.logFilePath, inf.FullName + " created or already exists");
            }
            else
            {
                 log(this.logFilePath, "Unable to create " + inf.FullName);
            }

        }

    }

   /*
   * Name: BaseType
   * Description: Parent class from which all modular classes inherit from.
   *              Contains all minimal generic functionalities to be implemented for proper User Environment setup 
   */
    public abstract class BaseType {
        abstract public void create();
        abstract public void create(string logFilePath);
        abstract public void update();
        abstract public void delete();
        abstract public void printAttrs();
        abstract public void log(string file, string entry);
    }

    /*
    * Name: Iterator
    * Description: Upon instantiation, parses through XML file, creates BaseType objects with information extracted from XMLnodes,
    *              and respective functions
    */
    public class Iterator{

        string filePath = "";

        public Iterator(string XMLfilePath)
        {
            this.filePath = XMLfilePath;
  
            //parse through XML
            XmlDocument doc = new XmlDocument();

            //Load XML from file path 
            doc.Load(this.filePath);

            ObjectFactory OF = new ObjectFactory();

            XmlNode root = doc.LastChild;

            //Iterate through config's child nodes
            foreach (XmlNode node in root.ChildNodes)
            {
                Console.WriteLine("name is " + node.Name);
                BaseType nodeObj = OF.getNodeObject(node);
                nodeObj.printAttrs();
                nodeObj.create();
            }
            

        }

        public Iterator(string XMLfilePath, string logFilePath)
        {
            this.filePath = XMLfilePath;

            //parse through XML
            XmlDocument doc = new XmlDocument();

            //Load XML from file path 
            doc.Load(@"C:\Users\olivarp\Desktop\XML_files\config.xml");

            ObjectFactory OF = new ObjectFactory();

            XmlNode root = doc.LastChild;

            //Iterate through config's child nodes
            foreach (XmlNode node in root.ChildNodes)
            {
                Console.WriteLine("name is " + node.Name);
                BaseType nodeObj = OF.getNodeObject(node);
                nodeObj.printAttrs();
                nodeObj.create(logFilePath);
            }


        }

    }

 
    /*
    * Name: ObjectFactory
    * Description: Produces appropriate instantiated BaseType object from a given XML node
    */
    public class ObjectFactory
    {

        XmlNode factoryNode;
        string nodeType = "";

        //Extract information from node and return object with all extracted information
        public BaseType getNodeObject(XmlNode node)
        {
            this.factoryNode = node;
            nodeType = node.Name;
            switch (nodeType)
            {
                case "Registry":
                    Console.WriteLine("The node type is Registry");

                    if (factoryNode.Attributes["valueKind"] != null)
                    {
                         return new Registry(factoryNode.Attributes["key"].Value, factoryNode.Attributes["name"].Value, factoryNode.Attributes["value"].Value, factoryNode.Attributes["valueKind"].Value);
                    }
                    else
                    {
                        return new Registry(factoryNode.Attributes["key"].Value, factoryNode.Attributes["name"].Value, factoryNode.Attributes["value"].Value);
                    }
                case "File":
                    Console.WriteLine("The node type is File WITH NAME " + factoryNode.Attributes["Name"].Value);
                    return new File(factoryNode.Attributes["rootLocation"].Value, factoryNode.Attributes["Name"].Value, factoryNode.Attributes["UserContainingDirectory"].Value, factoryNode.Attributes["BackupContainingDirectory"].Value);
                case "Ini":
                    Console.WriteLine("The node type is Ini");
                    return new IniFile(factoryNode.Attributes["PathRoot"].Value, factoryNode.Attributes["PathPostRoot"].Value, factoryNode.Attributes["Section"].Value, factoryNode.Attributes["Key"].Value, factoryNode.Attributes["Value"].Value);
                default:
                    return null;
            }

        }

    }


}
