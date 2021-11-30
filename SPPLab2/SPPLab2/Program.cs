using System;
using System.IO;

namespace SPPLab2
{
    public delegate void TaskDelegate(string file1, string file2);

    static class Program
    {
        delegate void TaskDelegate();

        static void Main(string[] args)
        {
            int countfFiles = 0;
            Console.WriteLine("Введите исходный каталог");
            String sourcePath = Console.ReadLine();
            sourcePath = @"C:\Labs\!Tet\1";
            Console.WriteLine("Введите целевой каталог");
            String targetPath = Console.ReadLine();
            targetPath = @"C:\Labs\!Tet\2";

            TaskQueue task = new TaskQueue(3);

            DirectoryCopy(sourcePath, targetPath, task, ref countfFiles);

            task.CloseTasks();

            Console.WriteLine("Файлов скопировано {0}", countfFiles);
            Console.ReadKey();
        }


        private static void DirectoryCopy(string sourceDirName, string destDirName, TaskQueue task, ref int numberOfFiles)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Каталог не найден: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!File.Exists(temppath))
                {
                    task.EnqueueTask(delegate () { file.CopyTo(temppath, false); });
                    numberOfFiles++;
                }
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, task, ref numberOfFiles);
            }

        }
    }
}
