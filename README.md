
# IniFile

IniFile is a single C# file that offers reading from and writing to INI files.
You can drop it straight into your project without adding any DLL references.

## Basic usage

### Reading an INI file

```csharp
// [foo]
// bar=1
// baz=qux
// [foo2]
// bar=2
		
var iniFile = new IniFile("foo.ini");
iniFile.Section("foo").Get("bar"); // "1"
iniFile.Section("foo").Get("baz"); // "qux"
		
foreach (var section in iniFile.Sections())
	Console.WriteLine(section.Name); // foo
								     // bar
```

### Writing an INI file	

```csharp
var iniFile = new IniFile();
	
iniFile.Section("Foo").Comment = "This is foo";
iniFile.Section("Foo").Set("bar", "1");
iniFile.Section("Foo").Set("baz", "qux", comment: "baz is qux");

iniFile.Section("Foo2").Set("bar", "2");

iniFile.Save("foo.ini");

// ;This is foo
// [foo]
// bar=1
// ;baz is qux
// baz=qux
// [foo2]
// bar=2
```

