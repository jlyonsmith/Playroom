using System;
using System.Collections;
using System.Collections.Generic;
using ToolBelt;

namespace Playroom
{
	public class ItemGroup : IDictionary<string, IList<ParsedPath>>
	{
		#region Fields
		private Dictionary<string, IList<ParsedPath>> dictionary;

		#endregion

		#region Construction
		public ItemGroup()
		{
			dictionary = new Dictionary<string, IList<ParsedPath>>(StringComparer.OrdinalIgnoreCase);
		}

		public ItemGroup(ItemGroup itemGroup) : this()
		{
			IEnumerator<KeyValuePair<string, IList<ParsedPath>>> e = ((IEnumerable<KeyValuePair<string, IList<ParsedPath>>>)itemGroup).GetEnumerator();

			while (e.MoveNext())
			{
				KeyValuePair<string, IList<ParsedPath>> pair = e.Current;

				dictionary.Add(pair.Key, pair.Value);
			}
		}

		#endregion

		#region Methods
		public void ExpandAndAdd(List<ContentFileV2.Item> items, PropertyGroup propGroup)
		{
			foreach (var item in items)
			{
				IList<ParsedPath> pathList;

				if (!dictionary.TryGetValue(item.Name, out pathList))
				{
					pathList = new List<ParsedPath>();
					dictionary.Add(item.Name, pathList);
				}

				string[] parts = item.Include.Split(';');

				foreach (var part in parts)
				{
					ParsedPath pathSpec = new ParsedPath(propGroup.ReplaceVariables(part), PathType.File);
					IList<ParsedPath> paths;

					if (pathSpec.HasWildcards)
					{
						paths = DirectoryUtility.GetFiles(pathSpec, SearchScope.DirectoryOnly);

						foreach (var path in paths)
						{
							pathList.Add(path);
						}
					}
					else
					{
						pathList.Add(pathSpec);
					}
				}

				if (!String.IsNullOrEmpty(item.Exclude))
				{
					parts = item.Exclude.Split(';');

					foreach (var part in parts)
					{
						ParsedPath path = new ParsedPath(propGroup.ReplaceVariables(part), PathType.File);

						pathList.Remove(path);
					}
				}
			}
		}

		#endregion

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, IList<ParsedPath>>> IEnumerable<KeyValuePair<string, IList<ParsedPath>>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region ICollection Implementation
		public void Add(KeyValuePair<string, IList<ParsedPath>> item)
		{
			dictionary.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			dictionary.Clear();
		}

		public bool Contains(KeyValuePair<string, IList<ParsedPath>> item)
		{
			return dictionary.ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<string, IList<ParsedPath>>[] array, int arrayIndex)
		{
			throw new System.NotImplementedException();
		}

		public bool Remove(KeyValuePair<string, IList<ParsedPath>> item)
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

		#region IDictionary implementation
		public void Add(string key, IList<ParsedPath> value)
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

		public bool TryGetValue(string key, out IList<ParsedPath> value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		public IList<ParsedPath> this[string key]
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

		public ICollection<IList<ParsedPath>> Values
		{
			get
			{
				return dictionary.Values;
			}
		}
		#endregion

	}
}

