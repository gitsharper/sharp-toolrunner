### Multiple File Support in Visual Studio

Multiple child file support is enabled by using the following entries in the rule entry in the 
configuration file.

  * "ResultFiles": []
  * "SaveRestoreResultFiles": true,
  * "VSAddResultFiles": true,


```
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
```

When '\*' is the first character of the name then the input file name without extension should replace the '\*'. If there is no '\*' then the name is used "as is".





...
