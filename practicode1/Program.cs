
#region first
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
#region all
///הערה: עשיתי שהמשתמש יוכל לפתוח את שורת הפקודה הכללית וממנה להקליד מקום ממנו לקרא את התיקיות והקבצים ומקום שבו ישמור את הקובץ המקובץ
var languagesList = new List<string>//in order to chech validation of the lang 
{
    "c#" ,"java", "sql" , "angular" , "react" ,"assembler" , "c" , "c++" , "python" 
};
var fileExtensions = new List<string> {
    ".cs", ".cpp", ".h", ".java", ".py",
   ".js", ".ts", ".html", ".css",
    ".sql", ".json", ".xml", ".yaml", ".md", ".vs",
    ".pl", ".r", ".lua", ".sln"
};

var bundleOptionOutput = new Option<FileInfo>("--output", "File path and name");

var bundleOptionLanguages = new Option<string>( "--language", description: "bundle languages files", getDefaultValue: () => "all");

var pathOption = new Option<string>("--path", description: "Directory path") { IsRequired = true };

var bundleOptionNote = new Option<bool>("--note", description: "add command to file", getDefaultValue: () => false) ;

var bundleOptionSort = new Option<string>("--sort", description: "the order of coping the code", getDefaultValue: () => "name");

var bundleOptionRemoveLines = new Option<bool>("--remove", description: "remove empty lines", getDefaultValue: () => false);

var bundleOptionAuthor = new Option<string>("--author", "writing the author of code");

var bundleCommand = new Command("bundle", "Bundle code files from a directory to a single file");
var createRspCommand = new Command("create-rsp", "create reaponse file");

bundleCommand.AddOption(bundleOptionOutput);
bundleCommand.AddOption(pathOption);
bundleCommand.AddOption(bundleOptionNote);
bundleCommand.AddOption(bundleOptionSort);
bundleCommand.AddOption(bundleOptionRemoveLines);
bundleCommand.AddOption(bundleOptionAuthor);
bundleCommand.AddOption(bundleOptionLanguages);

bundleOptionOutput.AddAlias("-o");
pathOption.AddAlias("-p");
bundleOptionNote.AddAlias("-n");
bundleOptionSort.AddAlias("-s");
bundleOptionRemoveLines.AddAlias("-r");
bundleOptionAuthor.AddAlias("-a");
bundleOptionLanguages.AddAlias("-l");
bundleCommand.SetHandler((string directoryPath, string language, FileInfo output, bool note, string orderType, bool remove, string author) =>
{
    if (Directory.Exists(directoryPath))
    {
        ProcessDirectory(directoryPath, language, output, note, orderType, remove, author);
    }
    else if (directoryPath == null)
    {
        Console.WriteLine("directory is required!!");
    }
    else
    {
        Console.WriteLine("Directory not found.");
    }
    
}, pathOption, bundleOptionLanguages, bundleOptionOutput, bundleOptionNote, bundleOptionSort, bundleOptionRemoveLines, bundleOptionAuthor);



void ProcessDirectory(string directoryPath, string language, FileInfo output, bool note, string orderType, bool remove, string author)
{
    string[] allItems = Directory.GetFileSystemEntries(directoryPath);
    if (!CheckLanguageOk(language))
        Console.WriteLine("your choosing was wrong. all languages will include in the file");
    if (!languagesList.Contains(language))
    {
        Console.WriteLine("the lang isnot of programming");
        fileExtensions.Add(language);
    }
    else
        fileExtensions = finalLang(language, fileExtensions);
    if (orderType.Equals("name"))
    {
        allItems = allItems.OrderBy(item => Path.GetFileName(item)).ToArray();
    }
    else if (orderType.Equals("CodeType"))
    {
        allItems = allItems.OrderBy(item => Path.GetExtension(item)).ToArray();

    }
    foreach (var item in allItems)
    {
        if (fileExtensions.Contains(Path.GetExtension(item)))
        {
            var attributes = File.GetAttributes(item);

            if (attributes.HasFlag(FileAttributes.Directory))
            {
                Console.WriteLine($"Entering directory: {item}");
                ProcessDirectory(item, language, output, note, orderType, remove, author);
            }
            else
            {
                string fileExtension = Path.GetExtension(item);
                if (fileExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"Found file: {item}, Extension: {fileExtension}");
                    AppendFileContentToOutput(item,  output, note,  remove, author);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
        else
        {
            Console.WriteLine("the file does not match to the request");
            Console.WriteLine(Path.GetExtension(item));
        }
    }
}


List<string> finalLang(string s, List<string> fileExtensions)
{
    List<string> result = new List<string>();
    string[] languagesArray = s.Split(' ');

    foreach (string language in languagesArray)
    {
        switch (language.ToLower()) //כדי שתתבצע השוואה שלא תלויה באותיות גדולות או קטנות
        {
            case "all":
                return fileExtensions;

            case "c++":
                result.Add(".cpp");
                result.Add(".h");
                break;

            case "python":
                result.Add(".py");
                break;

            case "c#":
                result.Add(".cs");
                result.Add(".sln");
                break;

            case "java":
                result.Add(".java");
                break;

            case "react":
                result.AddRange(new[] { ".ts", ".js", ".html", ".css", ".tsx", ".jsx" });
                break;

            case "angular":
                result.AddRange(new[] { ".ts", ".js", ".html", ".css" });
                break;

            default:
                Console.WriteLine($"ERROR: {language} is not a valid language the file will include all files in the directory");
                result.AddRange(fileExtensions);
                break;
        }
    }
    return result;
}


void AppendFileContentToOutput(string filePath, FileInfo output, bool includeSoutceNote, bool remove, string author)
{
    if (!filePath.Contains("bin") && !filePath.Contains("debug"))
    {
        try
        {
            string content = File.ReadAllText(filePath);
            if (includeSoutceNote)
            {
                string comment = $"//Source: {filePath} \n";
                content = comment + content;
            }
            if (author != null)
            {
                string nameAuthor = $"//AuthorName: {author} \n";
                content = nameAuthor + content;
            }

            if (remove)
            {
                string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var nonEmptyLines = Array.FindAll(lines, line => !string.IsNullOrWhiteSpace(line));
                content = String.Join(Environment.NewLine, nonEmptyLines);
            }
            content += "\n\n\n\n\n";
            Console.WriteLine($"Content from {filePath} (without empty lines) added to {output.FullName}");
            File.AppendAllText(output.FullName, content);

            Console.WriteLine($"Content from {filePath} added to {output.FullName}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
        }
    }
}

bool CheckLanguageOk(string language)
{
    string[] languagesArray = language.Split(' ');
    return languagesArray.All(lang => languagesList.Contains(lang));
}
void printing(string type, string msgToPrint)
{
    
    if (type == "error")
     Console.ForegroundColor = ConsoleColor.DarkRed;
    else if(type == "succeed")
        Console.ForegroundColor = ConsoleColor.Yellow;
    else 
        Console.ForegroundColor = ConsoleColor.Cyan;

    Console.WriteLine(msgToPrint);
    Console.ResetColor();
}

createRspCommand.SetHandler(() =>
{
    using var file = new StreamWriter("response.rsp");
    var filePath = Path.GetFullPath("response.rsp");
    printing("succeed", $"Response file created at: {filePath}");
    file.WriteLine("bundle");

    printing("output", "enter the directory for reading files");
    var path = Console.ReadLine()?.Trim();
    if (!Directory.Exists(path))
    {
        printing("error", "the directory not fount the file will save here");
        file.WriteLine("--path " + Environment.CurrentDirectory);
    }
    else
        file.WriteLine("--path " + path);

    printing("output","enter directory for keeping file");
    var output = Console.ReadLine()?.Trim();
    if (output == null)
    {
        printing("error","the directory is required. it will keep here");
        file.WriteLine("--output " + Environment.CurrentDirectory);
    }
    else
        file.WriteLine("--output " + output);

    printing("output" ,"enter programming language");
    string language = Console.ReadLine()?.Trim();
    if (language == null || !CheckLanguageOk(language))
        printing("error", "the lang is empty or invalid . we will read all langs");
    else if (CheckLanguageOk(language))
        printing("succeed", "language is valid");
    file.WriteLine("--language " + language);


    printing("output","adding a note to the file? Y/N");
    var note = Console.ReadLine()?.Trim().ToUpper();
    if (note == "Y")
        file.WriteLine("--note");
    if (note != "N" && note != "Y")
       printing("error","input is invalid. defaulting without comments");

    printing("output","sorting by... ? (name/CodeType");
    var sort = Console.ReadLine()?.Trim();
    if (sort != null)
    {
        if (sort.Equals("name") || sort.Equals("CodeType"))
            file.WriteLine("--sort " + sort);
        else
        {
           printing("error","Invalid sorting option, defaulting to 'name'.");
            file.WriteLine("--sort name");
        }

    }

    printing("output","earasing empty lines? Y/N");
    var remove = Console.ReadLine()?.Trim().ToUpper();
    if (remove == "Y")
    {
        file.WriteLine("--remove");
    }
    if (remove != "N" && remove != "Y")
        printing("error","invalid choosing.");


   printing("output","writing the author name : ");
    var author = Console.ReadLine();
    if (!string.IsNullOrEmpty(author))
        file.WriteLine("--author " + author);
    else
       printing("error" , "No author name provided.");
    file.Close();
    printing("succeed","Response file created: response.rsp");
    printing("output","in order to run this command enter: cli @response.rsp");
});


var rootCommand = new RootCommand("Root command for File Bundle CLI");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
await rootCommand.InvokeAsync(args);

#endregion




#endregion

