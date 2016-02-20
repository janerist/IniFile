using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YourNamespace
{
    public class IniProperty
    {
        /// <summary>
        /// Property name (key).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Property value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Set the comment to display above this property.
        /// </summary>
        public string Comment { get; set; }
    }

    public class IniSection
    {
        private readonly IDictionary<string, IniProperty> _properties;

        /// <summary>
        /// Section name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Set the comment to display above this section.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Get the properties in this section.
        /// </summary>
        public IniProperty[] Properties
        {
            get { return _properties.Values.ToArray(); }
        }

        /// <summary>
        /// Create a new IniSection.
        /// </summary>
        /// <param name="name"></param>
        public IniSection(string name)
        {
            Name = name;
            _properties = new Dictionary<string, IniProperty>();
        }

        /// <summary>
        /// Get a property value.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <returns>Value of the property or null if it doesn't exist.</returns>
        public string Get(string name)
        {
            if (_properties.ContainsKey(name))
                return _properties[name].Value;

            return null;
        }

        public T Get<T>(string name)
        {
            if (_properties.ContainsKey(name))
                return (T)Convert.ChangeType( _properties[name].Value, typeof(T));

            return default(T);
        }

        /// <summary>
        /// Set a property value.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        /// <param name="comment">A comment to display above the property.</param>
        public void Set(string name, string value, string comment = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                RemoveProperty(name);
                return;
            }

            if (!_properties.ContainsKey(name))
                _properties.Add(name, new IniProperty { Name = name, Value = value, Comment = comment });
            else
            {
                _properties[name].Value = value;
                if (comment != null)
                    _properties[name].Comment = comment;
            }
        }

        /// <summary>
        /// Remove a property from this section.
        /// </summary>
        /// <param name="propertyName">The property name to remove.</param>
        public void RemoveProperty(string propertyName)
        {
            if (_properties.ContainsKey(propertyName))
                _properties.Remove(propertyName);
        }
    }

    public class IniFile
    {
        private readonly IDictionary<string, IniSection> _sections;

        /// <summary>
        /// If True, writes extra spacing between the property name and the property value.
        /// (foo=bar) vs (foo = bar)
        /// </summary>
        public bool WriteSpacingBetweenNameAndValue { get; set; }

        /// <summary>
        /// The character a comment line will begin with. Default '#'.
        /// </summary>
        public char CommentChar { get; set; }

        /// <summary>
        /// Get the sections in this IniFile.
        /// </summary>
        public IniSection[] Sections => _sections.Values.ToArray();

        /// <summary>
        /// Create a new IniFile instance.
        /// </summary>
        public IniFile()
        {
            _sections = new Dictionary<string, IniSection>();
            CommentChar = '#';
        }

        /// <summary>
        /// Load an INI file from the file system.
        /// </summary>
        /// <param name="path">Path to the INI file.</param>
        public IniFile(string path) : this()
        {
            Load(path);
        }

        /// <summary>
        /// Load an INI file.
        /// </summary>
        /// <param name="reader">A TextReader instance.</param>
        public IniFile(TextReader reader) : this()
        {
            Load(reader);
        }

        private void Load(string path)
        {
            using (var file = new StreamReader(path))
                Load(file);
        }

        private void Load(TextReader reader)
        {
            IniSection section = null;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // skip empty lines
                if (line == string.Empty)
                    continue;

                // skip comments
                if (line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var sectionName = line.Substring(1, line.Length - 2);
                    if (!_sections.ContainsKey(sectionName))
                    {
                        section = new IniSection(sectionName);
                        _sections.Add(sectionName, section);
                    }
                    continue;
                }

                if (section != null)
                {
                    var keyValue = line.Split(new[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length != 2)
                        continue;

                    section.Set(keyValue[0].Trim(), keyValue[1].Trim());
                }
            }
        }

        /// <summary>
        /// Get a section by name. If the section doesn't exist, it is created.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        /// <returns>A section. If the section doesn't exist, it is created.</returns>
        public IniSection Section(string sectionName)
        {
            IniSection section;
            if (!_sections.TryGetValue(sectionName, out section))
            {
                section = new IniSection(sectionName);
                _sections.Add(sectionName, section);
            }

            return section;
        }

        /// <summary>
        /// Remove a section.
        /// </summary>
        /// <param name="sectionName">Name of the section to remove.</param>
        public void RemoveSection(string sectionName)
        {
            if (_sections.ContainsKey(sectionName))
                _sections.Remove(sectionName);
        }

        /// <summary>
        /// Create a new INI file.
        /// </summary>
        /// <param name="path">Path to the INI file to create.</param>
        public void Save(string path)
        {
            var txt = BuildIni();
            File.AppendAllText(path, txt);  
        }

        /// <summary>
        /// Create a new INI file.
        /// </summary>
        private string BuildIni()
        {
            var sb = new StringBuilder();
            foreach (var section in _sections.Values)
            {
                if (section.Properties.Length == 0)
                    continue;
                if (sb.Length > 0)
                    sb.AppendLine();

                if (section.Comment != null)
                    sb.AppendLine($"{CommentChar} {section.Comment}");

                sb.AppendLine($"[{section.Name}]");

                foreach (var property in section.Properties)
                {
                    if (property.Comment != null)
                        sb.AppendLine($"{CommentChar} {property.Comment}");

                    var format = WriteSpacingBetweenNameAndValue ? "{0} = {1}" : "{0}={1}";
                    sb.AppendLine(string.Format(format, property.Name, property.Value));
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildIni();
        }
    }
}
