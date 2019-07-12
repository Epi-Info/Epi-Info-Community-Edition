
using Newtonsoft.Json;
using Epi.Data.REDCap.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;
using static System.String;

namespace Epi.Data.REDCap.Utilities
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TestPriorityAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="priority"></param>
		public TestPriorityAttribute(int priority)
		{
			Priority = priority;
		}
		/// <summary>
		/// 
		/// </summary>
		public int Priority { get; private set; }
	}
	/// <summary>
	/// 
	/// </summary>
	public class PriorityOrderer : ITestCaseOrderer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TTestCase"></typeparam>
		/// <param name="testCases"></param>
		/// <returns></returns>
		public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
		{
			var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

			foreach (TTestCase testCase in testCases)
			{
				int priority = 0;

				foreach (IAttributeInfo attr in testCase.TestMethod.Method.GetCustomAttributes((typeof(TestPriorityAttribute).AssemblyQualifiedName)))
					priority = attr.GetNamedArgument<int>("Priority");

				GetOrCreate(sortedMethods, priority).Add(testCase);
			}

			foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
			{
				list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
				foreach (TTestCase testCase in list)
					yield return testCase;
			}
		}

		static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			TValue result;

			if (dictionary.TryGetValue(key, out result)) return result;

			result = new TValue();
			dictionary[key] = result;

			return result;
		}
	}
	/// <summary>
	/// Utility class for extension methods
	/// </summary>
	public static class Utils
	{

		/// <summary>
		/// Method gets the display string for an enum
		/// </summary>
		/// <param name="enumString"></param>
		/// <returns></returns>
		public static string GetDisplayName(this Enum enumString)
		{
			var returnvalue = enumString
				.GetType()
				.GetMember(enumString.ToString())
				.First()
				.GetCustomAttribute<DisplayAttribute>()
				.GetName();

			return returnvalue;
		}
		/// <summary>
		/// Extension method reads a stream and saves content to a local file.
		/// </summary>
		/// <param name="httpContent"></param>
		/// <param name="fileName"></param>
		/// <param name="path"></param>
		/// <param name="overwrite"></param>
		/// <param name="fileExtension"></param>
		/// <returns>HttpContent</returns>
		public static Task ReadAsFileAsync(this HttpContent httpContent, string fileName, string path, bool overwrite, string fileExtension = "pdf")
		{

			if (!overwrite && File.Exists(Path.Combine(fileName + fileExtension?.SingleOrDefault(), path)))
			{
				throw new InvalidOperationException($"File {fileName} already exists.");
			}
			FileStream filestream = null;
			try
			{
				fileName = fileName.Replace("\"", "");
				/*
                 * Add extension
                 */
				if (!string.IsNullOrEmpty(fileExtension))
				{
					fileName = fileName + "." + fileExtension;
				}
				filestream = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write, FileShare.None);
				return httpContent.CopyToAsync(filestream).ContinueWith(
					(copyTask) =>
					{
						filestream.Flush();
						filestream.Dispose();
					}
				);
			}
			catch (Exception Ex)
			{
				Log.Error(Ex.Message);
				if (filestream != null)
				{
					filestream.Flush();
				}
				throw new InvalidOperationException($"{Ex.Message}");
			}
		}
		/// https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp
		/// <summary>Indicates whether the specified array is null or has a length of zero.</summary>
		/// <param name="array">The array to test.</param>
		/// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
		public static bool IsNullOrEmpty<T>(this T[] array)
		{
			return (array == null || array.Length == 0);
		}
		/// <summary>
		/// This method converts string[] into string. For example, given string of "firstName, lastName, age"
		/// gets converted to "["firstName","lastName","age"]" 
		/// This is used as optional arguments for the Redcap Api
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="inputArray"></param>
		/// <returns>string[]</returns>
		public static async Task<string> ConvertArraytoString<T>(this RedcapApi redcapApi, T[] inputArray)
		{
			try
			{
				if (inputArray.IsNullOrEmpty())
				{
					throw new ArgumentNullException("Please provide a valid array.");
				}
				StringBuilder builder = new StringBuilder();
				foreach (T v in inputArray)
				{

					builder.Append(v);
					// We do not need to append the , if less than or equal to a single string
					if (inputArray.Length <= 1)
					{
						return await Task.FromResult(builder.ToString());
					}
					builder.Append(",");
				}
				// We trim the comma from the string for clarity
				return await Task.FromResult(builder.ToString().TrimEnd(','));

			}
			catch (Exception Ex)
			{
				Log.Error($"{Ex.Message}");
				return await Task.FromResult(String.Empty);
			}
		}
		/// <summary>
		/// This method converts int[] into a string. For example, given int[] of "[1,2,3]"
		/// gets converted to "["1","2","3"]" 
		/// This is used as optional arguments for the Redcap Api
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="inputArray"></param>
		/// <returns>string</returns>
		public static async Task<string> ConvertIntArraytoString(this RedcapApi redcapApi, int[] inputArray)
		{
			try
			{
				StringBuilder builder = new StringBuilder();
				foreach (var v in inputArray)
				{

					builder.Append(v);
					// We do not need to append the , if less than or equal to a single string
					if (inputArray.Length <= 1)
					{
						return await Task.FromResult(builder.ToString());
					}
					builder.Append(",");
				}
				// We trim the comma from the string for clarity
				return await Task.FromResult(builder.ToString().TrimEnd(','));

			}
			catch (Exception Ex)
			{
				Log.Error($"{Ex.Message}");
				return await Task.FromResult(String.Empty);
			}
		}
		/// <summary>
		///The method hands the return content from a request, the response.
		/// The method allows the calling method to choose a return type.
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="returnContent"></param>
		/// <returns>string</returns>
		public static async Task<string> HandleReturnContent(this RedcapApi redcapApi, ReturnContent returnContent = ReturnContent.count)
		{
			try
			{
				var _returnContent = returnContent.ToString();
				switch (returnContent)
				{
					case ReturnContent.ids:
						_returnContent = ReturnContent.ids.ToString();
						break;
					case ReturnContent.count:
						_returnContent = ReturnContent.count.ToString();
						break;
					default:
						_returnContent = ReturnContent.count.ToString();
						break;
				}
				return await Task.FromResult(_returnContent);
			}
			catch (Exception Ex)
			{
				Log.Error(Ex.Message);
				return await Task.FromResult(String.Empty);
			}
		}
		/// <summary>
		/// Tuple that returns both inputFormat and redcap returnFormat
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="format">csv, json[default], xml , odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
		/// <param name="onErrorFormat"></param>
		/// <param name="redcapDataType"></param>
		/// <returns>tuple, string, string, string</returns>
		public static async Task<(string format, string onErrorFormat, string redcapDataType)> HandleFormat(this RedcapApi redcapApi, ReturnFormat? format = ReturnFormat.json, OnErrorFormat? onErrorFormat = OnErrorFormat.json, RedcapDataType? redcapDataType = RedcapDataType.flat)
		{
			// default
			var _format = ReturnFormat.json.ToString();
			var _onErrorFormat = OnErrorFormat.json.ToString();
			var _redcapDataType = RedcapDataType.flat.ToString();

			try
			{

				switch (format)
				{
					case ReturnFormat.json:
						_format = ReturnFormat.json.ToString();
						break;
					case ReturnFormat.csv:
						_format = ReturnFormat.csv.ToString();
						break;
					case ReturnFormat.xml:
						_format = ReturnFormat.xml.ToString();
						break;
					default:
						_format = ReturnFormat.json.ToString();
						break;
				}

				switch (onErrorFormat)
				{
					case OnErrorFormat.json:
						_onErrorFormat = OnErrorFormat.json.ToString();
						break;
					case OnErrorFormat.csv:
						_onErrorFormat = OnErrorFormat.csv.ToString();
						break;
					case OnErrorFormat.xml:
						_onErrorFormat = OnErrorFormat.xml.ToString();
						break;
					default:
						_onErrorFormat = OnErrorFormat.json.ToString();
						break;
				}

				switch (redcapDataType)
				{
					case RedcapDataType.flat:
						_redcapDataType = RedcapDataType.flat.ToString();
						break;
					case RedcapDataType.eav:
						_redcapDataType = RedcapDataType.eav.ToString();
						break;
					case RedcapDataType.longitudinal:
						_redcapDataType = RedcapDataType.longitudinal.ToString();
						break;
					case RedcapDataType.nonlongitudinal:
						_redcapDataType = RedcapDataType.nonlongitudinal.ToString();
						break;
					default:
						_redcapDataType = RedcapDataType.flat.ToString();
						break;
				}

				return await Task.FromResult((_format, _onErrorFormat, _redcapDataType));
			}
			catch (Exception Ex)
			{
				Log.Error(Ex.Message);
				return await Task.FromResult((_format, _onErrorFormat, _redcapDataType));
			}
		}

		/// <summary>
		/// Method gets the overwrite behavior type and converts into string
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="overwriteBehavior"></param>
		/// <returns>string</returns>
		public static async Task<string> ExtractBehaviorAsync(this RedcapApi redcapApi, OverwriteBehavior overwriteBehavior)
		{
			try
			{
				var _overwriteBehavior = OverwriteBehavior.normal.ToString();
				switch (overwriteBehavior)
				{
					case OverwriteBehavior.overwrite:
						_overwriteBehavior = OverwriteBehavior.overwrite.ToString();
						break;
					case OverwriteBehavior.normal:
						_overwriteBehavior = OverwriteBehavior.normal.ToString();
						break;
					default:
						_overwriteBehavior = OverwriteBehavior.overwrite.ToString();
						break;
				}
				return await Task.FromResult(_overwriteBehavior);
			}
			catch (Exception Ex)
			{
				Log.Error($"{Ex.Message}");
				return await Task.FromResult(String.Empty);
			}
		}
		/// <summary>
		/// This method extracts and converts an object's properties and associated values to redcap type and values.
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="input">Object</param>
		/// <returns>Dictionary of key value pair.</returns>
		public static async Task<Dictionary<string, string>> GetProperties(this RedcapApi redcapApi, object input)
		{
			try
			{
				if (input != null)
				{
					// Get the type
					var type = input.GetType();
					var obj = new Dictionary<string, string>();
					// Get the properties
					var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
					PropertyInfo[] properties = input.GetType().GetProperties();
					foreach (var prop in properties)
					{
						// get type of column
						// The the type of the property
						Type columnType = prop.PropertyType;

						// We need to set lower case for REDCap's variable nameing convention (lower casing)
						string propName = prop.Name.ToLower();
						// We check for null values
						var propValue = type.GetProperty(prop.Name).GetValue(input, null)?.ToString();
						if (propValue != null)
						{
							var t = columnType.GetGenericArguments();
							if (t.Length > 0)
							{
								if (columnType.GenericTypeArguments[0].FullName == "System.DateTime")
								{
									var dt = DateTime.Parse(propValue);
									propValue = dt.ToString();
								}
								if (columnType.GenericTypeArguments[0].FullName == "System.Boolean")
								{
									if (propValue == "True")
									{
										propValue = "1";
									}
									else
									{
										propValue = "0";
									}
								}
							}
							obj.Add(propName, propValue);
						}
						else
						{
							// We have to make sure we handle for null values.
							obj.Add(propName, null);
						}
					}
					return await Task.FromResult(obj);
				}
				return await Task.FromResult(new Dictionary<string, string> { });
			}
			catch (Exception Ex)
			{
				Log.Error($"{Ex.Message}");
				return await Task.FromResult(new Dictionary<string, string> { });
			}
		}
		/// <summary>
		/// Method extracts events into list from string
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="events"></param>
		/// <param name="delimiters">char[] e.g [';',',']</param>
		/// <returns>List of string</returns>
		public static async Task<List<string>> ExtractEventsAsync(this RedcapApi redcapApi, string events, char[] delimiters)
		{
			if (!String.IsNullOrEmpty(events))
			{
				try
				{
					var formItems = events.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
					List<string> eventsResult = new List<string>();
					foreach (var form in formItems)
					{
						eventsResult.Add(form);
					}
					return await Task.FromResult(eventsResult);
				}
				catch (Exception Ex)
				{
					Log.Error($"{Ex.Message}");
					return await Task.FromResult(new List<string> { });
				}
			}
			return await Task.FromResult(new List<string> { });
		}
		/// <summary>
		/// Method gets / extracts fields into list from string
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="fields"></param>
		/// <param name="delimiters">char[] e.g [';',',']</param>
		/// <returns>List of string</returns>
		public static async Task<List<string>> ExtractFieldsAsync(this RedcapApi redcapApi, string fields, char[] delimiters)
		{
			if (!String.IsNullOrEmpty(fields))
			{
				try
				{
					var fieldItems = fields.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
					List<string> fieldsResult = new List<string>();
					foreach (var field in fieldItems)
					{
						fieldsResult.Add(field);
					}
					return await Task.FromResult(fieldsResult);
				}
				catch (Exception Ex)
				{
					Log.Error($"{Ex.Message}");
					return await Task.FromResult(new List<string> { });
				}
			}
			return await Task.FromResult(new List<string> { });
		}
		/// <summary>
		/// Method gets / extract records into list from string
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="records"></param>
		/// <param name="delimiters">char[] e.g [';',',']</param>
		/// <returns>List of string</returns>
		public static async Task<List<string>> ExtractRecordsAsync(this RedcapApi redcapApi, string records, char[] delimiters)
		{
			if (!String.IsNullOrEmpty(records))
			{
				try
				{
					var recordItems = records.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
					List<string> recordResults = new List<string>();
					foreach (var record in recordItems)
					{
						recordResults.Add(record);
					}
					return await Task.FromResult(recordResults);
				}
				catch (Exception Ex)
				{
					Log.Error($"{Ex.Message}");
					return await Task.FromResult(new List<string> { });
				}
			}
			return await Task.FromResult(new List<string> { });
		}
		/// <summary>
		/// Method gets / extracts forms into list from string
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="forms"></param>
		/// <param name="delimiters">char[] e.g [';',',']</param>
		/// <returns>A list of string</returns>
		public static async Task<List<string>> ExtractFormsAsync(this RedcapApi redcapApi, string forms, char[] delimiters)
		{
			if (!String.IsNullOrEmpty(forms))
			{
				try
				{
					var formItems = forms.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
					List<string> formsResult = new List<string>();
					foreach (var form in formItems)
					{
						formsResult.Add(form);
					}
					return await Task.FromResult(formsResult);
				}
				catch (Exception Ex)
				{
					Log.Error($"{Ex.Message}");
					return await Task.FromResult(new List<string> { });
				}
			}
			return await Task.FromResult(new List<string> { });
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="payload">data</param>
		/// <param name="uri">URI of the api instance</param>
		/// <returns>Stream</returns>
		public static async Task<Stream> GetStreamContentAsync(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri)
		{
			try
			{
				Stream stream = null;
				using (var client = new HttpClient())
				{
					// Encode the values for payload
					var content = new FormUrlEncodedContent(payload);
					using (var response = await client.PostAsync(uri, content))
					{
						if (response.IsSuccessStatusCode)
						{
							stream = await response.Content.ReadAsStreamAsync();
							return stream;
						}
					}

				}
				return null;
			}
			catch (Exception Ex)
			{
				Log.Error(Ex.Message);
				return null;
			}
		}
		/// <summary>
		/// Method to send http request using MultipartFormDataContent
		/// Requests with attachments
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="payload">data</param>
		/// <param name="uri">URI of the api instance</param>
		/// <returns>string</returns>
		public static async Task<string> SendRequestAsync(this RedcapApi redcapApi, MultipartFormDataContent payload, Uri uri)
		{
			try
			{
				using (var client = new HttpClient())
				{
					using (var response = await client.PostAsync(uri, payload))
					{
						if (response.IsSuccessStatusCode)
						{
							return await response.Content.ReadAsStringAsync();
						}
					}
				}
				return Empty;
			}
			catch (Exception Ex)
			{
				Log.Error(Ex.Message);
				return Empty;
			}
		}

		/// <summary>
		/// Sends request using http
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="payload">data</param>
		/// <param name="uri">URI of the api instance</param>
		/// <param name="isLargeDataset">Requests size > 32k chars </param>
		/// <returns></returns>
		public static async Task<string> SendPostRequestAsync(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri, bool isLargeDataset = false)
		{
			try
			{
				string _responseMessage = Empty;

				using (var client = new HttpClient())
				{

					// extract the filepath
					var pathValue = payload.Where(x => x.Key == "filePath").FirstOrDefault().Value;
					var pathkey = payload.Where(x => x.Key == "filePath").FirstOrDefault().Key;

					if (!string.IsNullOrEmpty(pathkey))
					{
						// the actual payload does not contain a 'filePath' key
						payload.Remove(pathkey);
					}

					/*
                     * Encode the values for payload
                     * Add in ability to process large data set, using StringContent
                     * Thanks to Ibrahim for pointing this out.
                     * https://stackoverflow.com/questions/23703735/how-to-set-large-string-inside-httpcontent-when-using-httpclient/23740338
                     */
					if (isLargeDataset)
					{
						/*
                         * Send request with large data set
                         */

						var serializedPayload = JsonConvert.SerializeObject(payload);
						using (var content = new StringContent(serializedPayload, Encoding.UTF8, "application/json"))
						{
							using (var response = await client.PostAsync(uri, content))
							{
								if (response.IsSuccessStatusCode)
								{
									// Get the filename so we can save with the name
									var headers = response.Content.Headers;
									var fileName = headers.ContentType.Parameters.Select(x => x.Value).FirstOrDefault();
									if (!string.IsNullOrEmpty(fileName))
									{
										var contentDisposition = response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
										{
											FileName = fileName
										};
									}

									if (!string.IsNullOrEmpty(pathValue))
									{
										// save the file to a specified location using an extension method
										await response.Content.ReadAsFileAsync(fileName, pathValue, true);
										_responseMessage = fileName;
									}
									else
									{
										_responseMessage = await response.Content.ReadAsStringAsync();
									}

								}
								else
								{
									_responseMessage = await response.Content.ReadAsStringAsync();
								}
							}

						}
						return _responseMessage;
					}
					else
					{
						/*
                        * Maximum character limit of 32,000 using FormUrlEncodedContent
                        * Send request using small data set
                        */
						using (var content = new FormUrlEncodedContent(payload))
						{
							using (var response = await client.PostAsync(uri, content))
							{
								if (response.IsSuccessStatusCode)
								{
									// Get the filename so we can save with the name
									var headers = response.Content.Headers;
									var fileName = headers.ContentType.Parameters.Select(x => x.Value).FirstOrDefault();
									if (!string.IsNullOrEmpty(fileName))
									{
										var contentDisposition = response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
										{
											FileName = fileName
										};
									}


									if (!string.IsNullOrEmpty(pathValue))
									{
										var fileExtension = payload.Where(x => x.Key == "content" && x.Value == "pdf").SingleOrDefault().Value;
										if (!string.IsNullOrEmpty(fileExtension))
										{
											// pdf 
											fileName = payload.Where(x => x.Key == "instrument").SingleOrDefault().Value;
											// to do , make extensions for various types
											// save the file to a specified location using an extension method
											await response.Content.ReadAsFileAsync(fileName, pathValue, true, fileExtension);

										}
										else
										{
											await response.Content.ReadAsFileAsync(fileName, pathValue, true, fileExtension);

										}
										_responseMessage = fileName;
									}
									else
									{
										_responseMessage = await response.Content.ReadAsStringAsync();
									}
								}
								else
								{
									_responseMessage = await response.Content.ReadAsStringAsync();
								}
							}
						}

					}
					return _responseMessage;
				}
			}
			catch (Exception Ex)
			{
				Log.Error($"{Ex.Message}");
				return Empty;
			}
		}
		/// <summary>
		/// Sends http request to api
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="payload">data </param>
		/// <param name="uri">URI of the api instance</param>
		/// <returns>string</returns>
		public static async Task<string> SendPostRequest(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri)
		{
			string responseString;
			using (var client = new HttpClient())
			{
				// Encode the values for payload
				using (var content = new FormUrlEncodedContent(payload))
				{
					using (var response = await client.PostAsync(uri, content))
					{
						// check the response and make sure its successful
						response.EnsureSuccessStatusCode();
						responseString = await response.Content.ReadAsStringAsync();
					}
				}
			}
			return responseString;
		}
		/// <summary>
		/// Method obtains list of string from comma seperated strings
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="redcapApi"></param>
		/// <param name="arms"></param>
		/// <param name="delimiters"></param>
		/// <returns>List of string</returns>
		public static async Task<List<string>> ExtractArmsAsync<T>(this RedcapApi redcapApi, string arms, char[] delimiters)
		{
			if (!String.IsNullOrEmpty(arms))
			{
				try
				{
					var _arms = arms.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
					List<string> armsResult = new List<string>();
					foreach (var arm in _arms)
					{
						armsResult.Add(arm);
					}
					return await Task.FromResult(armsResult);
				}
				catch (Exception Ex)
				{
					Log.Error($"{Ex.Message}");
					return await Task.FromResult(new List<string> { });
				}
			}
			return await Task.FromResult(new List<string> { });

		}
		/// <summary>
		/// Checks if the string passed is null or empty.
		/// </summary>
		/// <param name="redcapApi"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void CheckToken(this RedcapApi redcapApi, string token)
		{
			/*
             * Check the required parameters for empty or null
             */
			if (string.IsNullOrEmpty(token))
			{
				throw new ArgumentNullException("Please provide a valid Redcap token.");
			}
		}

	}
}
