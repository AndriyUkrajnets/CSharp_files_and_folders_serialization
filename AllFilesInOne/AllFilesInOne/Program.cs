using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace AllFilesInOne
{
    [Serializable]
    class FileRecord
    {
        public string filePath;
        public byte[] fileContent;
    }

    // Application as input takes folder (User can select folder) - baseDirectoryPath
    // Application saves selected folder with files and sub folders into one file - serialized.dat
    // binary serialization is used

    // Application as input takes file with folder structure from previous point and selects folder to deserialize - newDirectoryPath
    // System unpacks the folders and files into the selected folder. 
    // File/Folder/Sub Folder is kept

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("What do you want to do?");
            DialogResult dialogResult = MessageBox.Show("Serialize ?", "What do you want to do?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Console.WriteLine("Choose folder for serialization:");
                FolderBrowserDialog fbd_1 = new FolderBrowserDialog();
                DialogResult result = fbd_1.ShowDialog();
                string baseDirectoryPath = fbd_1.SelectedPath;

                string outputSerializedFilePath = Path.GetDirectoryName(baseDirectoryPath) + @"\serialized.dat";

                FilesAndFolderSerializer(baseDirectoryPath, outputSerializedFilePath);

                Console.WriteLine("We had saved selected folder with files and sub folders into one file");
                Console.WriteLine("Input Folder: {0}", baseDirectoryPath);
                Console.WriteLine("Serialized file: {0}", outputSerializedFilePath);
                Console.WriteLine("Ready!");
                Console.ReadLine();
            }
            else if (dialogResult == DialogResult.No)
            {
                Console.WriteLine("What do you want to do?");
                DialogResult dialogResult_2 = MessageBox.Show("Deserialize ?", "What do you want to do?", MessageBoxButtons.YesNo);
                if (dialogResult_2 == DialogResult.Yes)
                {
                    Console.WriteLine("Choose folder where we will put unpacked folders and files:");
                    FolderBrowserDialog fbd_2 = new FolderBrowserDialog();
                    DialogResult result_2 = fbd_2.ShowDialog();
                    string newDirectoryPath = fbd_2.SelectedPath;

                    Console.WriteLine("Choose serialized file:");
                    OpenFileDialog openFileDialog1 = new OpenFileDialog();
                    DialogResult result_3 = openFileDialog1.ShowDialog();
                    string outputSerializedFilePath = openFileDialog1.InitialDirectory + openFileDialog1.FileName;

                    FilesAndFolderDeserializer(outputSerializedFilePath, newDirectoryPath);
                    Console.WriteLine("Output Folder: {0}", newDirectoryPath);
                    Console.WriteLine("Serialized file: {0}", outputSerializedFilePath);
                    Console.WriteLine("Ready!");
                    Console.ReadLine();
                }
                else if (dialogResult_2 == DialogResult.No)
                {
                    Console.WriteLine("So, threre is nothing to do!");
                    Console.ReadLine();
                }
            }
        }

        public static List<string> GetAllFiles(string Dir)
        {
            List<string> filesWithPath = new List<string>();
            try
            {
                foreach (string fl in Directory.GetFiles(Dir))
                {
                    filesWithPath.Add(fl);
                }
                foreach (string f in Directory.GetDirectories(Dir))
                {
                    filesWithPath.AddRange(GetAllFiles(f));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return filesWithPath;
        }

        private static void FilesAndFolderSerializer(string baseDirectoryPath, string outputSerializedFilePath)
        {
            List<string> filesOnly = GetAllFiles(baseDirectoryPath);
            List<FileRecord> filesRead = new List<FileRecord>();
            foreach (var path in filesOnly)
            {
                var bytes = File.ReadAllBytes(path);
                var pathShort = path.Substring(baseDirectoryPath.Length);
                var record = new FileRecord { fileContent = bytes, filePath = pathShort };
                filesRead.Add(record);
            }

            FileStream fs = new FileStream(outputSerializedFilePath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, filesRead);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        static void FilesAndFolderDeserializer(string outputSerializedFilePath, string newDirectoryPath)
        {
            List<FileRecord> files_deserialized = null;
            FileStream fs = new FileStream(outputSerializedFilePath, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                files_deserialized = (List<FileRecord>)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

            foreach (var fileRecord in files_deserialized)
            {
                string outPath = newDirectoryPath + fileRecord.filePath;
                string outDir = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                File.WriteAllBytes(outPath, fileRecord.fileContent);
            }
        }
    }
}