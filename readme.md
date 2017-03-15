# Readme.md

Sharp-ToolRunner (STR) is a Visual Studio add-in that allows you to transform one file type into another file; in Visual Studio parlance it's a "custom tool" sometimes called a "single file generator". Rather than creating a custom tool for each type of transformer Sharp-ToolRunner allows you to use external programs to do the transformation as long as that program is command line accessible.

### Before I continue ...

There are other tools that do similar things and if you're interest is in ".less -> .css", or ".md -> .html" processing you probably want to use one of those tools. STR does not add editing features or syntax highlighting, it does not even perform the compiling, translation, or transforms on your files itself. STR calls the compiler or translator in the background when you save your file.


".less", and ".md" files are just examples, you can run any application or script that has a command line interface that allows you to perform the actions you require. Normally you will want to take the file you're editing in Visual Studio, pass it through a transformation, and then have Visual Studio save the result as a child file of the source file in your project. If need be you can run multiple commands against the source file, or pipe the results from one program to another until you have the created the result you want.

STR does not include the ".less", ".md", or any other translators/compilers within it's installer. Any programs or scripts you required to perform the actions you require must already be installed and working properly on your system before STR can make use of them.


### Configuration files

STR works by reading a configuration file **which you can easily extend to include your own tools**. It processes target (input) files by looking up their extension, or file name. The configuration file is written in JSON to make for easy editing.

```json
  {
    "Id": ".less",
    "OutputExtension": ".css",
    "Commands": [
      {
        "ExecutableName": "cmd.exe",
        "CmdLine": [ "/C", "lessc.cmd", "-no-color", "\"$in\"", "\"$out\"" ]
      }
    ]
  }
```

The entry above is for less and should be read something like this:

* For a file with the extension ".less"
* Execute "cmd.exe"
* With the command line: /C lessc.cmd -no-color "input_file" "output_file"
* The extension returned to Visual Studio is ".css"

Several things to note. 

* $in and $out are markers to tell STR where to place the names of the input and output files on the command line.
* If no output file is created by the tool being called (you should know this and not use the $out marker) STR will read STDOUT if you have set '"ReadStdOut" : true' for the command.
* The result of the translation is passed to Visual Studio which saves the final output file.

### Additionally

You can use file names in the "Id" member of a rule, this allows for configurations for specific files.

```json
  {
    "Id": "SpecialFile.less",
    "OutputExtension": ".xcss",
    "Commands": [
      {
        "ExecutableName": "cmd.exe",
        "CmdLine": [ "/C", "lessc.cmd", "-no-color", "\"$in\"", "\"$out\"" ]
      }
    ]
  }
```

In this example any files named "SpecialFile.less" would use this configuration and pass the ".xcss" extension (rather than ".css") back to Visual Studio.

There are several other "tricks", and commands you can use which I will document later.

### Configuration file location

You can have multiple configuration files, there locations can be:

1. The directory in which the target file exits.
2. In the targets project directory.
3. In the directory of the solution that the project belongs to.
4. In the a subdirectory of the users "Documents" directory named "Sharp-ToolRunner".
5. The directory in which STR is installed.

The configuration files are read first to last creating a list of rules with the highest precedence being given to the rules "closest" to the target file. So, if you have a rule for ".md" in a configuration file in the target files directory it will be chosen instead of any rule defined in any other configuration file.

The name of the configuration file is always **tool-runner.cfg.json**.

### Registering Sharp-ToolRunner as a Custom Tool for your files

Before you can use STR you must associate it as a "Custom Tool" with the files you wish to work with, you do this with the files properties. Right click on the file in Solution Explorer and select "Properties". On the files property page add "SharpToolRunner" as the "Custom Tool".

Note. If there is already an entry from some tool other than SharpToolRunner you **_probably do not_** want to replace it. If you do decide to replace it make sure you know what you're doing, Visual Studio can sometimes be very opaque about what configuration options it sets and it can be difficult getting back to where you started.

![](./images/FileProperties.png?raw=true)
![](https://github.com/gitsharper/sharp-toolrunner/blob/master/Images/FileProperties.png)

### Configuration file entries

Currently there are several entries in the configuration file. Check the sharp-toolrunner.cfg.json file for the latest configuration as these examples may be out of date.

#### .g4 (Antlr4 compiler)

Antlr4 generates more than one output file which is handled by this configuration.

Before I explain how that works note that (1) this configuration produces C# from the grammar file, (2) java.exe must be in the path, and (2) "antlr-4.5.3-complete.jar" must be given a full path following the "-jar" (the only way I was able to get java.exe to find the antlr4 jar file, note: I speak very little java and there may be a better way to do this). (3) you can put "antlr-4.5.3-complete.jar" anywhere you want, but the path must be set correctly, this is setup for my machine.

For an Antlr4 grammar file named "parameters.g4":

* The output file for parser.g4 will be "parser" (no file extension) and the file will be empty.
* Additional output files will be placed in the same directory as the source file, there is no current way to change this.
* Any existing output files from a previously successful run will be backed up and restored if the current run fails (see "SaveRestoreResultFiles" below).
* All output files will be added as children of the source file if the run is successful **and the run takes place from within Visual Studio** (see "VSAddResultFiles" below). The files will not be added if the run takes place using the command line version of Sharp-ToolRunner.
* * ![](./images/children.png?raw=true)
![](https://github.com/gitsharper/sharp-toolrunner/blob/master/Images/children.png)
* In the "ResultFiles" array any file name that begins with '\*' will have the '\*' replaced by the input file name with extension, in the example "parameters".

```
{
    "Id": ".g4",
    "OutputExtension": "",
    "SaveIntermediateFiles": false,

    "ResultFiles": [
      "*.tokens",
      "*Parser.cs",
      "*Lexer.cs",
      "*Lexer.tokens",
      "*BaseListener.cs",
      "*Listener.cs",
      "*BaseVisitor.cs",
      "*Visitor.cs"
    ],

    "SaveRestoreResultFiles": true,
    "VSAddResultFiles": true,

    "Commands": [
      {
        "ExecutableName": "java.exe",
        "AllowNoOutput": true,
        "CmdLine": [
          "-jar \"c:\\Program Files\\Antlr4\\antlr-4.5.3-complete.jar\"",
          "-o \"$file_path\"",
          "-message-format vs2005",
          "-Dlanguage=CSharp",
          "\"$in\""
        ]
      }

    ]
  }
```

#### .less (less compiler)

```json
  {
    "Id": ".less",
    "OutputExtension": "css",
    "Commands": [
      {
        "ExecutableName": "cmd.exe",
        "CmdLine": [ "/C", "lessc.cmd", "-no-color", "\"$in\"", "\"$out\"" ]
      }
    ]
  },
```


#### .md (markdown via marked, to html file)

```json
  {
    "Id": ".md",
    "OutputExtension": ".html",
    "Commands": [
      {
        "ExecutableName": "cmd.exe",
        "CmdLine": [ "/C", "marked.cmd", "-i \"$in\"", "-o \"$out\"" ]
      },
      {
        "ExecutableName": "internal:replace",
        "CmdLine": [ "--content--", ".assets:simple.outer.html" ]
      }
    ]
  }
```

This translate markdown (.md) to html. There a several things to note about this entry.

* It has multiple commands associated with it.
* The second command is for an STR internal command named "replace".

The "replace" command takes two arguments, "--content--" is the text to replace in the file referenced by the second argument. This text ("--content--") will be replaced by the output of the first command (the "marked" compiler). The second argument (file name) starts with ".assets:" which indicates STR should find it in the ".Assets" folder of the STR installation directory. You can customize the output by changing the .html file, add a reference to your own html source file somewhere in the file system. An absolute file name ("c:\files\some-file.html") will be searched for as is; a file name or name name with a partial path will be searched for relative to the input file's location.


#### .ps1 (powershell)

```json
  {
    "Id": ".ps1",
    "OutputExtension": ".txt",
    "Commands": [
      {
        "ExecutableName": "powershell.exe",
        "ReadStdOut": true,
        "CmdLine": [ "-noprofile", "-file", "\"$in\"" ]
      }
    ]
  },
```

Note "ReadStdOut" is set to true;

#### .btm (jpsoft's 'TCC' and "Take Command" processors)

```json
  {
    "Id": ".btm",
    "OutputExtension": ".txt",
    "Commands": [
      {
        "ExecutableName": "tcc.exe",
        "ReadStdOut": true,
        "CmdLine": [ "/C", "\"$in\"" ]
      }
    ]
  },
```

#### .ts (typescript)

```json
  {
    "Id": ".ts",
    "note" : "typescript",
    "Commands": [
      {
        "ExecutableName": "cmd.exe",
        "CmdLine": [ "/C", "tsc.cmd", "--out \"$out\"", "\"$in\"" ]
      }
    ]
  },
```





### License

[Eclipse Public License](license.txt)