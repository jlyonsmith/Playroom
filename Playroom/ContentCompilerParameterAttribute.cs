using System;

namespace Playroom
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ContentCompilerParameterAttribute : Attribute
	{
		#region Instance Constructors
		
		public ContentCompilerParameterAttribute()
		{
			Description = String.Empty;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentCompilerSettingAttribute" />.
		/// </summary>
		/// <param name="description">Description of the content compiler</param>
		public ContentCompilerParameterAttribute(string description) : this()
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
		/// Gets or sets a value indicating whether this <see cref="Playroom.ContentCompilerParameterAttribute"/> is for the compiler 
		/// </summary>
		/// <value><c>true</c> if for the compiler; otherwise, <c>false</c>.</value>
		public bool ForCompiler { get; set; }
		
		/// <summary>
		/// Private resource reader
		/// </summary>
		public Type ResourceReader { get; set; }
		
		#endregion	
	}
}

