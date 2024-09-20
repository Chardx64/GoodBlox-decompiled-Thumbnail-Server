using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ThumbServer.My
{
	[DebuggerNonUserCode]
	[CompilerGenerated]
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal sealed class InternalXmlHelper
	{
        private static IEnumerable<XElement> source;
        private static XName name; 
        [DebuggerNonUserCode]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[CompilerGenerated]
		private sealed class RemoveNamespaceAttributesClosure
		{
			private readonly string[] m_inScopePrefixes;

			private readonly XNamespace[] m_inScopeNs;

			private readonly List<XAttribute> m_attributes;

			[EditorBrowsable(EditorBrowsableState.Never)]
			internal RemoveNamespaceAttributesClosure(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes)
			{
				m_inScopePrefixes = inScopePrefixes;
				m_inScopeNs = inScopeNs;
				m_attributes = attributes;
			}

			[EditorBrowsable(EditorBrowsableState.Never)]
			internal XElement ProcessXElement(XElement elem)
			{
				return RemoveNamespaceAttributes(m_inScopePrefixes, m_inScopeNs, m_attributes, elem);
			}

			[EditorBrowsable(EditorBrowsableState.Never)]
			internal object ProcessObject(object obj)
			{
				if (obj is XElement e)
				{
					return RemoveNamespaceAttributes(m_inScopePrefixes, m_inScopeNs, m_attributes, e);
				}
				return obj;
			}
		}

		public static string Value
		{
			get
			{
				using (IEnumerator<XElement> enumerator = source.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						XElement current = enumerator.Current;
						return current.Value;
					}
				}
				return null;
			}
			set
			{
				using IEnumerator<XElement> enumerator = source.GetEnumerator();
				if (enumerator.MoveNext())
				{
					XElement current = enumerator.Current;
					current.Value = value;
				}
			}
		}

        public static string GetAttributeValue()
        {
            using (IEnumerator<XElement> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    XElement current = enumerator.Current;
                    return (string)current.Attribute(name);
                }
            }
            return null;
        }

        public static void SetAttributeValue(string value)
        {
            using IEnumerator<XElement> enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                XElement current = enumerator.Current;
                current.SetAttributeValue(name, value);
            }
        }

        public static string AttributeValue
        {
            get
            {
                return (string)source.FirstOrDefault()?.Attribute(name);
            }
            set
            {
                source.FirstOrDefault()?.SetAttributeValue(name, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
		private InternalXmlHelper()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static XAttribute CreateAttribute(XName name, object value)
		{
			if (value == null)
			{
				return null;
			}
			return new XAttribute(name, RuntimeHelpers.GetObjectValue(value));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static XAttribute CreateNamespaceAttribute(XName name, XNamespace ns)
		{
			XAttribute xAttribute = new XAttribute(name, ns.NamespaceName);
			xAttribute.AddAnnotation(ns);
			return xAttribute;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static object RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, object obj)
		{
			if (obj != null)
			{
				if (obj is XElement e)
				{
					return RemoveNamespaceAttributes(inScopePrefixes, inScopeNs, attributes, e);
				}
				if (obj is IEnumerable obj2)
				{
					return RemoveNamespaceAttributes(inScopePrefixes, inScopeNs, attributes, obj2);
				}
			}
			return obj;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IEnumerable RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, IEnumerable obj)
		{
			if (obj != null)
			{
				if (obj is IEnumerable<XElement> source)
				{
					return source.Select(new RemoveNamespaceAttributesClosure(inScopePrefixes, inScopeNs, attributes).ProcessXElement);
				}
				return obj.Cast<object>().Select(new RemoveNamespaceAttributesClosure(inScopePrefixes, inScopeNs, attributes).ProcessObject);
			}
			return obj;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static XElement RemoveNamespaceAttributes(string[] inScopePrefixes, XNamespace[] inScopeNs, List<XAttribute> attributes, XElement e)
		{
			checked
			{
				if (e != null)
				{
					XAttribute xAttribute = e.FirstAttribute;
					while (xAttribute != null)
					{
						XAttribute nextAttribute = xAttribute.NextAttribute;
						if (xAttribute.IsNamespaceDeclaration)
						{
							XNamespace xNamespace = xAttribute.Annotation<XNamespace>();
							string localName = xAttribute.Name.LocalName;
							if ((object)xNamespace != null)
							{
								if ((inScopePrefixes != null && inScopeNs != null) ? true : false)
								{
									int num = inScopePrefixes.Length - 1;
									int num2 = num;
									int num3 = 0;
									while (true)
									{
										int num4 = num3;
										int num5 = num2;
										if (num4 > num5)
										{
											break;
										}
										string value = inScopePrefixes[num3];
										XNamespace xNamespace2 = inScopeNs[num3];
										if (localName.Equals(value))
										{
											if (xNamespace == xNamespace2)
											{
												xAttribute.Remove();
											}
											xAttribute = null;
											break;
										}
										num3++;
									}
								}
								if (xAttribute != null)
								{
									if (attributes != null)
									{
										int num6 = attributes.Count - 1;
										int num7 = num6;
										int num8 = 0;
										while (true)
										{
											int num9 = num8;
											int num5 = num7;
											if (num9 > num5)
											{
												break;
											}
											XAttribute xAttribute2 = attributes[num8];
											string localName2 = xAttribute2.Name.LocalName;
											XNamespace xNamespace3 = xAttribute2.Annotation<XNamespace>();
											if ((object)xNamespace3 != null && localName.Equals(localName2))
											{
												if (xNamespace == xNamespace3)
												{
													xAttribute.Remove();
												}
												xAttribute = null;
												break;
											}
											num8++;
										}
									}
									if (xAttribute != null)
									{
										xAttribute.Remove();
										attributes.Add(xAttribute);
									}
								}
							}
						}
						xAttribute = nextAttribute;
					}
				}
				return e;
			}
		}
	}
}
