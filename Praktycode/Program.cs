using System.CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Reflection.Metadata;
internal class Program
{
    static int count = 0;
    private static void Main(string[] args)
    {
        var optionOutput = new Option<FileInfo>("--output", "File path and name");
        string[] basicLanguages = { ".cpp", ".cs", ".h", ".py", ".php", ".java", ".c", ".ts", ".js", ".sql", ".sv", ".json" };
        var optionLanguage = new Option<string>(
            "--language",
            "An option that that must be one of the values of a static list: .cpp, .cs, .h,.py, .php,.java, .c,.ts, .js, .sql,.sv,.json ");
        var optionSort = new Option<bool>("--sort", "true if you want to sort ,false otherwise");
        var optionNote = new Option<bool>("--note", "comment of the prev file location");
        var optionREL = new Option<bool>("--remove-empty-line", "remove empty line true/false");
        var optionAuthor = new Option<string>("--author", "authers name ");
        optionOutput.AddAlias("-o");
        optionLanguage.AddAlias("-l");
        optionSort.AddAlias("-s");
        optionNote.AddAlias("-n");
        optionREL.AddAlias("-r");
        optionAuthor.AddAlias("-a");
        var createRsp = new Command("create-rsp", "craete response");
        createRsp.SetHandler(() =>
        {
            string path = Directory.GetCurrentDirectory();
            string nameRsp, output, lan, sort, note, empty, author, writeName, name;
            string content = "";
            Console.Write("write the name of the file response ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            nameRsp = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            string chromePagePath = string.Concat(path, "\\", nameRsp, ".html");
            Console.Write("write a directory for new file");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            output = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            content = String.Concat(content, "-o ", output, " ");
            Console.Write("write the extention of the files you want to include in the new file ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            lan = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            content = String.Concat(content, "-l ", lan, " ");
            Console.Write("whould you like to write the code's source as a comment in the new file? y/n ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            note = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (note == "y")
            {
                content = String.Concat(content, "-n true ");
            }
            Console.Write("whould you like to sort the files in the new file \n(the default is sorting by abc of the file's name? y/n ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            sort = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (sort == "y")
            {
                content = String.Concat(content, "-s true ");
            }
            Console.Write("whould you like to remove empty lines in the new file? y/n ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            empty = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (empty == "y")
            {
                content = String.Concat(content, "-r true ");
            }
           
            Console.Write("whould you like to write your name in the new file? y/n ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            writeName = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (writeName == "y")
            {
                Console.Write("write your name ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                content = String.Concat(content, "-a ", Console.ReadLine());
                Console.ForegroundColor = ConsoleColor.White;
            }
            CreateChromePage(chromePagePath, content);
        });
        var bundleCommand = new Command("bundle", "Bundle code files to a single file");
        bundleCommand.AddOption(optionOutput);
        bundleCommand.AddOption(optionLanguage);
        bundleCommand.AddOption(optionSort);
        bundleCommand.AddOption(optionNote);
        bundleCommand.AddOption(optionREL);
        bundleCommand.AddOption(optionAuthor);
        bundleCommand.SetHandler((output, langauge, rel, note, sort, author) =>
        {
            bool flag = false;
            string[] langauges = langauge.Split(' ');
            foreach (var lan in langauges)
            {
                if (!basicLanguages.Any(l => lan.Equals(l)))
                {
                    Console.WriteLine($"{lan} doesn't have a valid extention");
                    flag = true;
                }
            }
            if (flag)
            {
                return;
            }
            if (langauge.Equals("all"))
            {
                langauges = basicLanguages;
            }
            string path = Directory.GetCurrentDirectory();
            CombineFiles(path, langauges, output.FullName, rel, note, sort, author);
        }, optionOutput, optionLanguage, optionREL, optionNote, optionSort, optionAuthor);
        var rootCommand = new RootCommand("root command");
        rootCommand.AddCommand(bundleCommand);
        rootCommand.AddCommand(createRsp);
        rootCommand.InvokeAsync(args);
    }
    static void CombineFiles(string folderPath, string[] extensions, string outputFilePath, bool rel, bool note, bool sort, string? author)
    {
        // Validate folder path
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Invalid folder path.");
            return;
        }
        // Create a list to store content from selected files
        var combinedContent = new List<string>();
        var groupContent = new List<string>();
        if (author != "")
        {
            groupContent.Add($"File creator name is:{author}");
        }
        // Recursively process files in the folder and its subfolders
        ProcessFilesInFolder(folderPath, extensions, combinedContent, rel);
        if (sort)
        {
            SortFilesByExtension(combinedContent);
        }
        else
            SortFilesByFileName(combinedContent);
        GroupContent(combinedContent, groupContent, note);
        // Combine the content and write it to the output file
        try
        {
            File.WriteAllLines(outputFilePath, groupContent);
            Console.WriteLine($"The content written to {outputFilePath} , {count} files were copied");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error writing to output file: {e.Message}");
        }
    }
    static void ProcessFilesInFolder(string folderPath, string[] extensions, List<string> combinedContent, bool rel)
    {
        // Process files in the current folder
        foreach (string filePath in Directory.GetFiles(folderPath))
        {
            // Check if the file has one of the specified extensions
            if (extensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    // Read the content of the file and append it to the list
                    if (rel)
                    {
                        RemoveEmptyLines(filePath);
                    }
                    combinedContent.Add(filePath);
                    count++;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading file {Path.GetFileName(filePath)}: {e.Message}");
                }
            }
        }
        // Recursively process files in subfolders
        foreach (string subfolder in Directory.GetDirectories(folderPath))
        {
            if (!subfolder.EndsWith("bin") && !subfolder.EndsWith("Debug"))
                ProcessFilesInFolder(subfolder, extensions, combinedContent, rel);
        }
    }
    static void RemoveEmptyLines(string filePath)
    {
        try
        {
            // קריאה לקובץ
            string[] lines = File.ReadAllLines(filePath);
            // סינון של השורות השאלמות
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            // כתיבה מחדש לקובץ
            File.WriteAllLines(filePath, lines);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    static void SortFilesByFileName(List<string> filePaths)
    {
        filePaths.Sort((path1, path2) =>
        {
            string fileName1 = Path.GetFileName(path1);
            string fileName2 = Path.GetFileName(path2);
            return String.Compare(fileName1, fileName2, StringComparison.Ordinal);
        });
    }
    static void SortFilesByExtension(List<string> filePaths)
    {
        filePaths.Sort((path1, path2) =>
        {
            string extension1 = Path.GetExtension(path1);
            string extension2 = Path.GetExtension(path2);
            return String.Compare(extension1, extension2, StringComparison.Ordinal);
        });
    }
    static void GroupContent(List<string> combinedContent, List<string> groupContent, bool note)
    {
        foreach (var filePath in combinedContent)
        {
            string fileContent = File.ReadAllText(filePath);
            if (note)
            {
                groupContent.Add("#" + filePath);
            }
            groupContent.Add(fileContent);
        }
    }
    static void CreateChromePage(string chromePagePath, string content)
    {
        try
        {
            // כתיבת המחרוזת לקובץ דף כרום
            File.WriteAllText(chromePagePath, content);
            Console.WriteLine("The new file were created successfully :" + chromePagePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating Chrome page: {e.Message}");
        }
    }
}
