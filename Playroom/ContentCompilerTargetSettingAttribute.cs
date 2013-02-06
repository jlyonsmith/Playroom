using System;

namespace Playroom
{
	/// <summary>
	/// Defines a content compiler target setting property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class ContentCompilerTargetSettingAttribute : Attribute
	{
		#region Instance Constructors
		
		public ContentCompilerTargetSettingAttribute()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentCompilerSettingAttribute" />.
		/// </summary>
		/// <param name="description">Description of the content compiler</param>
		public ContentCompilerTargetSettingAttribute(string description)
		{
			Description = description;
		}
		
		#endregion
		
		#region Instance Properties
		/// <summary>
		/// Gets or sets the description of the command line tool.
		/// </summary>
		public string Description { get; set; }
		
		/// <summary>
		/// Gets or sets the description of the command line tool.
		/// </summary>
		public bool Optional { get; set; }
		
		/// <summary>
		/// Gets or sets the default value.
		/// </summary>
		/// <value>The default.</value>
		public string Default { get; set; }
		
		/// <summary>
		/// Private resource reader
		/// </summary>
		public Type ResourceReader { get; set; }
		
		#endregion	
	}
}

