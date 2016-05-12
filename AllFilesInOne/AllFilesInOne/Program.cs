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

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Оберіть папку, вміст якої буде серіалізовано:");
            FolderBrowserDialog fbd_1 = new FolderBrowserDialog();
            DialogResult result = fbd_1.ShowDialog();
            string DirectoryPath = fbd_1.SelectedPath;

            Console.WriteLine("Оберіть папку, в яку будуть внесені файли після серіалізації:");
            FolderBrowserDialog fbd_2 = new FolderBrowserDialog();
            DialogResult result_2 = fbd_2.ShowDialog();
            string newDirectoryPath = fbd_2.SelectedPath;

            string outputSerializedFilePath = Path.GetDirectoryName(DirectoryPath) + @"\serialized.dat";

            Console.WriteLine("Input Folder: {0}", DirectoryPath);
            Console.WriteLine("Output Folder: {0}", newDirectoryPath);
            Console.WriteLine("Serialized file: {0}", outputSerializedFilePath);

            Serialize(DirectoryPath, outputSerializedFilePath, newDirectoryPath);
            Deserialize(outputSerializedFilePath, newDirectoryPath);

            Console.WriteLine("Готово");
            Console.ReadLine();
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

        private static void Serialize(string DirectoryPath, string outputSerializedFilePath, string newDirectoryPath)
        {
            List<string> filesOnly = GetAllFiles(DirectoryPath);
            List<FileRecord> filesRead = new List<FileRecord>();
            foreach (var path in filesOnly)
            {
                var bytes = File.ReadAllBytes(path);
                var pathShort = path.Substring(DirectoryPath.Length);
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

        static void Deserialize(string outputSerializedFilePath, string newDirectoryPath)
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
