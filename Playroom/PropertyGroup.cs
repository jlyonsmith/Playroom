using System;
using System.Collections;
using System.Collections.Generic;
using ToolBelt;

namespace Playroom
{
	public class PropertyGroup : IDictionary<string, string>
	{
        #region Private Fields
        private Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

		#region Construction
		public PropertyGroup()
		{
		}

		public PropertyGroup(PropertyGroup other)
		{
			IEnumerator<KeyValuePair<string, string>> e = ((IEnumerable<KeyValuePair<string, string>>)other).GetEnumerator();

			while (e.MoveNext())
			{
				KeyValuePair<string, string> pair = e.Current;

				dictionary.Add(pair.Key, pair.Value);
			}
		}

		#endregion

		#region Properties
		public ReadOnlyDictionary<string, string> AsReadOnlyDictionary()
		{
			return new ReadOnlyDictionary<string, string>(this);
		}

		#endregion

		#region Methods
        public string ReplaceVariables(string s)
        {
            return s.ReplaceTags("$(", ")", AsReadOnlyDictionary());
        }

        public void AddWellKnownProperties(
            ParsedPath buildContentInstallDir,
            ParsedPath contentFileDir)
        {
            this["BuildContentInstallDir"] = buildContentInstallDir.ToString();
            this["InputRootDir"] = contentFileDir.ToString();
            this["OutputRootDir"] = contentFileDir.ToString();
        }

        public void ExpandAndAdd(List<Tuple<string, string>> pairs, PropertyGroup propGroup)
        {
            foreach (var pair in pairs)
            {
                this[pair.Item1] = propGroup.ReplaceVariables(pair.Item2);
            }
        }

        public void AddFromEnvironment()
        {
            IDictionary entries = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry entry in entries)
            {
                if (!String.IsNullOrEmpty((string)entry.Key))
                    this[(string)entry.Key] = (string)entry.Value;
            }
        }

        public void AddFromPropertyString(string keyValuePairString)
        {
            if (String.IsNullOrEmpty(keyValuePairString))
                return;

            string[] keyValuePairs = keyValuePairString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string keyValuePair in keyValuePairs)
            {
                string[] keyAndValue = keyValuePair.Split('=');

                if (keyAndValue.Length == 2)
                {
                    this[keyAndValue[0]] = keyAndValue[1];
                }
            }
        }

		public string GetRequiredValue(string name)
		{
			return this[name];
		}
		
		public string GetOptionalValue(string name, string defaultValue)
		{
			string value;

			if (!this.TryGetValue(name, out value))
				value = defaultValue;

			return value;
		}
		
		public void GetOptionalValue(string name, out int value, int defaultValue)
		{
			string s;
			
			if (!this.TryGetValue(name, out s) || !Int32.TryParse(s, out value))
				value = defaultValue;
		}
		
		public void GetRequiredValue(string name, out int value)
		{
			string s;
			
			if (!this.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
			
			if (!Int32.TryParse(s, out value))
				throw new InvalidOperationException("Property '{0}' value '{1}' is not a valid integer".CultureFormat(name, s));
		}

		public void GetRequiredValue(string name, out string[] value)
		{
			string s;
			
			if (!this.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));

			value = s.Split(';');
		}

		#endregion

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region ICollection Implementation
		public void Add(KeyValuePair<string, string> item)
		{
			dictionary.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			dictionary.Clear();
		}

		public bool Contains(KeyValuePair<string, string> item)
		{
			return dictionary.ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			throw new System.NotImplementedException();
		}

		public bool Remove(KeyValuePair<string, string> item)
		{
			return dictionary.Remove(item.Key);
		}

		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region IDictionary Implementation
		public void Add(string key, string value)
		{
			dictionary.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return dictionary.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return dictionary.Remove(key);
		}

		public bool TryGetValue(string key, out string value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		public string this[string key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				dictionary[key] = value;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return dictionary.Keys;
			}
		}

		public ICollection<string> Values
		{
			get
			{
				return dictionary.Values;
			}
		}
		#endregion
	}
}

