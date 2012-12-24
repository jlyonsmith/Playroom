using System;
using System.Collections.Generic;

namespace Playroom
{
	public class GraphNode<T>
	{
		public GraphNode(T item)
		{
			this.Item = item;
		}

		public T Item { get; private set; }

		public override int GetHashCode()
		{
			return Item.GetHashCode();
		}
	}

	public class Graph<T>
	{
		private Dictionary<GraphNode<T>, HashSet<T>> graph;

		public Graph()
		{
		}
	}
}

