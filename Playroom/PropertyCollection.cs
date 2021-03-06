using System;
using System.Collections;
using System.Collections.Generic;
using ToolBelt;

namespace Playroom
{
	public class PropertyCollection : IEnumerable<KeyValuePair<string, string>>
	{
        #region Private Fields
        private Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

		#region Construction
		public PropertyCollection()
		{
		}

		public PropertyCollection(PropertyCollection other)
		{
			IEnumerator<KeyValuePair<string, string>> e = ((IEnumerable<KeyValuePair<string, string>>)other).GetEnumerator();

			while (e.MoveNext())
			{
				KeyValuePair<string, string> pair = e.Current;

				dictionary.Add(pair.Key, pair.Value);
			}
		}

		#endregion

		#region IEnumerable
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)dictionary.GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region Methods
		public string ExpandVariables(string s, bool throwOnUnknown = true)
		{
			return s.ReplaceTags("$(", ")", this.dictionary, throwOnUnknown ? TaggedStringOptions.ThrowOnUnknownTags : TaggedStringOptions.LeaveUnknownTags);
		}
		
		public void AddFromList(IEnumerable<KeyValuePair<string, string>> pairs)
		{
			foreach (var pair in pairs)
			{
				dictionary[pair.Key] = this.ExpandVariables(pair.Value);
			}
		}
		
		public void AddFromEnvironment()
		{
			IDictionary entries = Environment.GetEnvironmentVariables();
			
			foreach (DictionaryEntry entry in entries)
			{
				if (!String.IsNullOrEmpty((string)entry.Key))
				{
					dictionary[(string)entry.Key] = (string)entry.Value;
				}
			}
		}
		
		public void AddFromString(string keyValuePairString)
		{
			if (String.IsNullOrEmpty(keyValuePairString))
				return;
			
			string[] keyValuePairs = keyValuePairString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			
			foreach (string keyValuePair in keyValuePairs)
			{
				string[] keyAndValue = keyValuePair.Split('=');
				
				if (keyAndValue.Length == 2)
				{
					dictionary[keyAndValue[0]] = this.ExpandVariables(keyAndValue[1].Trim());
				}
			}
		}
		
		public string GetRequiredValue(string name)
		{
			string value = null;
			
			if (!dictionary.TryGetValue(name, out value))
				throw new InvalidOperationException("Required property '{0}' not found".CultureFormat(name));
			
			return value;
		}
		
		public void GetOptionalValue(string name, out string value, string defaultValue)
		{
			if (!dictionary.TryGetValue(name, out value))
				value = defaultValue;
		}
		
		public string GetOptionalValue(string name, string defaultValue)
		{
			string value;
			
			GetOptionalValue(name, out value, defaultValue);
			
			return value;
		}
		
		public void GetOptionalValue(string name, out int value, int defaultValue)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s) || !Int32.TryParse(s, out value))
				value = defaultValue;
		}
		
		public void GetRequiredValue(string name, out int value)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
			
			if (!Int32.TryParse(s, out value))
				throw new InvalidOperationException("Property '{0}' value '{1}' is not a valid integer".CultureFormat(name, s));
		}
		
		public void GetRequiredValue(string name, out string value)
		{
			if (!dictionary.TryGetValue(name, out value))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
		}
		
		public void GetRequiredValue(string name, out string[] value)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
			
			value = s.Split(';');
		}
		
		public void Clear()
		{
			dictionary.Clear();
		}

		public bool Contains(string name)
		{
			return dictionary.ContainsKey(name);
		}

		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		public void Set(string key, string value)
		{
			dictionary.Add(key, value);
		}

		public bool Remove(string key)
		{
			return dictionary.Remove(key);
		}

		#endregion
	}
}

