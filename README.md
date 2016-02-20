
# IniFile

IniFile is a single C# file that offers reading from and writing to INI files.

## Basic usage

### Reading an INI file

```csharp
// [foo]
// bar=1
// baz=qux
// [foo2]
// bar=2
		
var iniFile = new IniFile("foo.ini");
iniFile.Section("foo").Get<int>("bar"); // 1
iniFile.Section("foo").Get("baz"); // "qux"
		
foreach (var section in iniFile.Sections)
	Console.WriteLine(section.Name); // foo
								     // foo2
```

### Writing an INI file	

```csharp
var iniFile = new IniFile();
	
iniFile.Section("foo").Comment = "This is foo";
iniFile.Section("foo").Set("bar", "1");
iniFile.Section("foo").Set("baz", "qux", comment: "baz is qux");

iniFile.Section("foo2").Set("bar", "2");

iniFile.Save("foo.ini");

// # This is foo
// [foo]
// bar=1
// # baz is qux
// baz=qux
//
// [foo2]
// bar=2

iniFile.Section("foo").RemoveProperty("bar"); // or iniFile.Section("foo").Set("bar", null);
iniFile.RemoveSection("foo2");
iniFile.Save("foo.ini");

// # This is foo
// [foo]
// # baz is qux
// baz=qux
```