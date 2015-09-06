# Readme.md

Sharp-ToolRunner (STR) is a Visual Studio add-in that allows you to transform one file type into another file type; in Visual Studio parlance it's a "single file generator". An example would be converting a ".less" file into a ".css" file, or a ".md" file into an ".html" file.

### Before I continue ...

There are other tools that do similar things and if you're interest is in ".less -> .css", or ".md -> .html" processing you probably want to use one of those tools. STR does not add editing features or syntax highlighting, it does not even perform the compiling, translation, or transforms on your files itself. STR calls the compiler or translator in the background when you save your file.


".less", and ".md" files are examples, you can run any application or script that has a command line interface that allows you to perform the actions you require. Normally you will want to take the file you're editing in Visual Studio, pass it through a transformation, and then have Visual Studio save the result to a different file in your project. If need be you can run multiple commands against the source file, or pipe the results from one program to another until you have the created the result you want.

STR does not include the ".less", ".md", or any other translators/compilers within it's installer. Any programs or scripts you required to perform the actions you require must already be installed and working properly on your system before STR can make use of them.


### Configuration files

STR works by reading a configuration file **which you can easily extend to include your own tools**. It processes target (input) files by looking up their extension, or file name. The configuration file is written in JSON which (might) make for easy editing.

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

For any files named "SpecialFile.less" STR would invoke the normal compilation for a ".less" file, but would pass the ".xcss" extension (rather than ".css") back to Visual Studio.

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

Currently there are several entries in the configuration file.

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

The "replace" command takes two arguments, "--content--" which is the text "replace" will try to locate within the file referenced by the second argument. This text ("--content--") will be replaced by the output of the first command (the "marked" compiler). The second argument (file name) starts with ".assets:" which indicates STR should find it in the ".Assets" folder of the STR installation directory. You can customize the output by changing the .html file used; add a configuration higher up the chain and create a new entry for ".md", add a reference to your own html source file somewhere in the file system. An absolute file name ("c:\files\some-file.html") will be searched for as is. A file name or name with a partial path will be searched for relative to the input file's location.


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

#### .nmp4 (Net Macro Processor)

```json
{
		"Id": ".nmp4",
		"OutputExtension": "",
		"Commands": [
			{
				"ExecutableName": "nmp4host.exe",
				"CmdLine": [ "-d:#VSCustomTool=1", "-out:\"$out\"", "\"$in\"" ]
			}
		]
	},
```

This is something of my own invention but is unavailable because of it's horrible error tracking and reporting.

The only interesting thing to note here is that "OutputExtension is empty (""). This is because the macro processor has a convention where the name of the input file name contains the output extension embedded within it, e.g. "translate.txt.nmp4". The ".nmp4" gets stripped off and we're left with "translate.txt". You might or might not find this technique useful.



### License

[Eclipse Public License](license.txt)