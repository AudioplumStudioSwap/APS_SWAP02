using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class LoggerLayout
	//!
	//! @brief layout for a log message
	//! @details 	class that formats a log message according to a string with identifiers.
	//!				example : %filename (%fileline) [%verbosity] : %message
	//!
	//!				list of available identifiers :
	//!					- %message		: the log message
	//!					- %verbosity 	: the verbosity of the message
	//!					- %block		: the block name and indentation
	//!					- %time			: the time since the start of the game
	//!					- %deltatime	: the delta time of the current frame
	//!					- %frame		: the frame counter since the start of the game
	//!					- %filename		: the name of the file that has triggered the log request
	//!					- %function		: the name of the function that has triggered the log request
	//!					- %fileline		: the number of the line in the file that has triggered the log request
	internal class LoggerLayout
	{
		internal bool Configure(System.Xml.XmlNodeList parameters)
		{
			foreach(System.Xml.XmlElement element in parameters)
			{
				if(element.Name == "value")
				{
					m_value = element.InnerText;
				}
				else
				{
					string identifier = "%" + element.Name;
					IdentifierParameter parameter = CreateIdentifierParameter(identifier, element.Attributes);
					m_identifierParameters.Add(identifier, parameter);
				}
			}

			return true;
		}

		internal string ApplyLayout(Log.Verbosity verbosity, string message, uint blockCount, uint callStackIndex)
		{
			string result = m_value;
			
			int layoutCharIndex = 0;
			while(layoutCharIndex < result.Length)
			{
				if(result[layoutCharIndex] == '%')
				{
					int identifierStart = layoutCharIndex;
					int identifierEnd = layoutCharIndex + 1;
					string identifier = "%";
					while(identifierEnd < result.Length  &&  result[identifierEnd] >= 'a'  &&  result[identifierEnd] <= 'z')
					{
						identifier += result[identifierEnd];
						++identifierEnd;
					}
					
					if(identifier.Length > 1)
					{
						string identifierValue = GetLayoutIdentifierValue(identifier, verbosity, message, blockCount, callStackIndex + 1);
						ApplyIdentifierFormat(ref identifierValue, identifier);
						result = result.Remove(identifierStart, identifier.Length);
						result = result.Insert(identifierStart, identifierValue);
						layoutCharIndex += identifierValue.Length;
					}
					else
					{
						layoutCharIndex = identifierEnd + 1;
					}
				}
				else
				{
					++layoutCharIndex;
				}
			}
			
			return result;
		}

#region Private
		private string GetLayoutIdentifierValue(string identifier, Log.Verbosity verbosity, string message, uint blockCount, uint callStackIndex)
		{
			string result = identifier;
			
			// message
			if(identifier == "%message")
			{
				result = message;
			}
			// verbosity
			else if(identifier == "%verbosity")												{ result = (verbosity <= Log.Verbosity.Fatal)? verbosity.ToString() : ""; }
			// block
			else if(identifier == "%block")
			{
				IdentifierParameter idParams;
				if(m_identifierParameters.TryGetValue(identifier, out idParams))
				{
					BlockIdentifierParameter blockParams = idParams as BlockIdentifierParameter;
					switch(verbosity)
					{
						case Log.Verbosity.BlockBegin: 	result = blockParams.m_begin; break;
						case Log.Verbosity.BlockEnd:	result = blockParams.m_end; break;
						default: 						result = ""; break;
					}

					for(uint blockIndex = 0; blockIndex < blockCount; ++blockIndex)
					{
						result = result.Insert(0, blockParams.m_contentPrefix);
					}
				}
				else
				{
					result = "";
				}
			}
			// time
			else if(identifier == "%time") 													{ result = Time.time.ToString(); }
			else if(identifier == "%deltatime")												{ result = Time.deltaTime.ToString(); }
			// frame
			else if(identifier == "%frame")													{ result = Time.frameCount.ToString(); }

			else if(Debug.isDebugBuild)
			{
				// file
				if(identifier == "%filename")												{ result = new System.Diagnostics.StackFrame((int)callStackIndex, true).GetFileName(); }
				else if(identifier == "%function")											{ result = new System.Diagnostics.StackFrame((int)callStackIndex, true).GetMethod().Name; }
				else if(identifier == "%fileline")											{ result = new System.Diagnostics.StackFrame((int)callStackIndex, true).GetFileLineNumber().ToString(); }
			}

			Assertion.Check(result != null, "Invalid message with identifier : " + identifier);

			return result;
		}

		private void ApplyIdentifierFormat(ref string value, string identifier)
		{
			IdentifierParameter parameter;
			if(m_identifierParameters.TryGetValue(identifier, out parameter))
			{
				if(value.Length > parameter.max)
				{
					int diff = value.Length - (int)parameter.max;

					switch(parameter.trim)
					{
						case Trim.left:
						{
							int nbDots = Mathf.Clamp(diff, 1, 3);
							value = value.Substring(value.Length - (int)parameter.max + nbDots);
							for(int dotIndex = 0; dotIndex < nbDots; ++dotIndex)
							{
								value = value.Insert(0, ".");
							}
						}
						break;
						case Trim.right:
						{
							int nbDots = Mathf.Clamp(diff, 1, 3);
							value = value.Substring(0, (int)parameter.max - nbDots);

							for(int dotIndex = 0; dotIndex < nbDots; ++dotIndex)
							{
								value+= ".";
							}
						}
						break;
						case Trim.center:
						{							
							int midStringIndex = value.Length / 2;
							int nbDots = Mathf.Clamp(diff - 2, 1, 3);

							int firstPartLastIndex = midStringIndex - diff / 2 - nbDots / 2;
							int secondPartFirstIndex = firstPartLastIndex + nbDots + diff;

							string temp = value.Substring(0, firstPartLastIndex);
							for(int dotIndex = 0; dotIndex < nbDots; ++dotIndex)
							{
								temp += ".";
							}
							temp += value.Substring(secondPartFirstIndex, value.Length - secondPartFirstIndex);
							value = temp;
						}
						break;
					}
				}

				if(value.Length < parameter.min)
				{
					switch(parameter.align)
					{
						case Align.left:
						{
							while(value.Length < parameter.min)
							{
								value += " ";
							}
						}
						break;
						case Align.right:
						{
							while(value.Length < parameter.min)
							{
								value = value.Insert(0, " ");
							}
						}
						break;
						case Align.center:
						{
							int nbSpaceAdded = 0;
							while(value.Length < parameter.min)
							{
								if((nbSpaceAdded % 2) == 0)
								{
									value += " ";
								}
								else
								{
									value = value.Insert(0, " ");
								}
								++nbSpaceAdded;
							}
						}
						break;
					}
				}
			}
		}

		private IdentifierParameter CreateIdentifierParameter(string identifier, System.Xml.XmlAttributeCollection attributes)
		{
			IdentifierParameter parameter = null;
			if(identifier == "%block")
			{
				parameter = new BlockIdentifierParameter();
			}
			else
			{
				parameter = new IdentifierParameter();
			}

			foreach(System.Xml.XmlAttribute attribute in attributes)
			{
				parameter.Fill(attribute);
			}
			
			return parameter;
		}
		
		private enum Align { left, center, right }
		private enum Trim { left, center, right }

		private class IdentifierParameter
		{
			internal uint min = 0;
			internal uint max = uint.MaxValue;
			internal Align align = Align.left;
			internal Trim trim = Trim.center;

			internal virtual void Fill(System.Xml.XmlAttribute attribute)
			{
				if(attribute.Name == "min") 				{ min = (uint)attribute.Value.ToInteger(); }
				else if(attribute.Name == "max")			{ max = (uint)attribute.Value.ToInteger(); }
				else if(attribute.Name == "align")			{ align = attribute.Value.ToEnum<Align>(); }
				else if(attribute.Name == "trim")			{ trim = attribute.Value.ToEnum<Trim>(); }
			}
		}

		private class BlockIdentifierParameter : IdentifierParameter
		{
			internal string m_begin = "";
			internal string m_contentPrefix = "";
			internal string m_end = "";

			internal override void Fill(System.Xml.XmlAttribute attribute)
			{
				base.Fill(attribute);

				if(attribute.Name == "begin") 				{ m_begin = attribute.Value; }
				else if(attribute.Name == "content-prefix")	{ m_contentPrefix = attribute.Value; }
				else if(attribute.Name == "end")			{ m_end = attribute.Value; }
			}
		}

		//! value
		string m_value = "%message";
		SortedDictionary<string, IdentifierParameter> m_identifierParameters = new SortedDictionary<string, IdentifierParameter>();
#endregion
	}
}
