using Newtonsoft.Json;
using Epi.Data.REDCap.Interfaces;
using Epi.Data.REDCap.Models;
using Epi.Data.REDCap.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.String;

namespace Epi.Data.REDCap
{
	public class RedcapApi : IRedcap
	{
		private static string _token;
		private static Uri _uri;

		public RedcapApi(string redcapApiUrl)
		{
			_uri = new Uri(redcapApiUrl);
		}

		/// <summary>
		/// API Version 1.0.0+
		/// Export Instruments (Data Entry Forms)
		/// This method allows you to export a list of the data collection instruments for a project. 
		/// This includes their unique instrument name as seen in the second column of the Data Dictionary, as well as each instrument's corresponding instrument label, which is seen on a project's left-hand menu when entering data. The instruments will be ordered according to their order in the project.
		/// </summary>
		/// <remarks>
		/// To use this method, you must have API Export privileges in the project.
		/// </remarks>
		/// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
		/// <param name="content">instrument</param>
		/// <param name="format">csv, json [default], xml</param>
		/// <returns>Instruments for the project in the format specified and will be ordered according to their order in the project.</returns>
		public async Task<string> ExportInstrumentsAsync(string token, Content content = Content.Instrument, ReturnFormat format = ReturnFormat.json)
		{
			try
			{
				/*
                 * Check for presence of token
                 */
				this.CheckToken(token);
				var payload = new Dictionary<string, string>
				{
					{ "token", token },
					{ "content", content.GetDisplayName() },
					{ "format", format.GetDisplayName() }
				};
				// Execute request
				return await this.SendPostRequestAsync(payload, _uri);
			}
			catch (Exception Ex)
			{
				/*
                 * We'll just log the error and return the error message.
                 */
				Log.Error($"{Ex.Message}");
				return Ex.Message;
			}
		}

		/// <summary>
		/// API Version 1.0.0++
		/// Export Record
		/// This method allows you to export a single record for a project.
		/// Note about export rights: Please be aware that Data Export user rights will be applied to this API request.For example, if you have 'No Access' data export rights in the project, then the API data export will fail and return an error. And if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then some data fields *might* be removed and filtered out of the data set returned from the API. To make sure that no data is unnecessarily filtered out of your API request, you should have 'Full Data Set' export rights in the project.
		/// </summary>
		/// <remarks>
		/// To use this method, you must have API Export privileges in the project.
		/// </remarks>
		/// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
		/// <param name="content">record</param>
		/// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
		/// <param name="redcapDataType">flat - output as one record per row [default], eav - output as one data point per row. Non-longitudinal: Will have the fields - record*, field_name, value. Longitudinal: Will have the fields - record*, field_name, value, redcap_event_name</param>
		/// <param name="record">a single record specifying specific records you wish to pull (by default, all records are pulled)</param>
		/// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all fields are pulled)</param>
		/// <param name="forms">an array of form names you wish to pull records for. If the form name has a space in it, replace the space with an underscore (by default, all records are pulled)</param>
		/// <param name="events">an array of unique event names that you wish to pull records for - only for longitudinal projects</param>
		/// <param name="rawOrLabel">raw [default], label - export the raw coded values or labels for the options of multiple choice fields</param>
		/// <param name="rawOrLabelHeaders">raw [default], label - (for 'csv' format 'flat' type only) for the CSV headers, export the variable/field names (raw) or the field labels (label)</param>
		/// <param name="exportCheckboxLabel">true, false [default] - specifies the format of checkbox field values specifically when exporting the data as labels (i.e., when rawOrLabel=label) in flat format (i.e., when type=flat). When exporting labels, by default (without providing the exportCheckboxLabel flag or if exportCheckboxLabel=false), all checkboxes will either have a value 'Checked' if they are checked or 'Unchecked' if not checked. But if exportCheckboxLabel is set to true, it will instead export the checkbox value as the checkbox option's label (e.g., 'Choice 1') if checked or it will be blank/empty (no value) if not checked. If rawOrLabel=false or if type=eav, then the exportCheckboxLabel flag is ignored. (The exportCheckboxLabel parameter is ignored for type=eav because 'eav' type always exports checkboxes differently anyway, in which checkboxes are exported with their true variable name (whereas the 'flat' type exports them as variable___code format), and another difference is that 'eav' type *always* exports checkbox values as the choice label for labels export, or as 0 or 1 (if unchecked or checked, respectively) for raw export.)</param>
		/// <param name="onErrorFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
		/// <param name="exportSurveyFields">true, false [default] - specifies whether or not to export the survey identifier field (e.g., 'redcap_survey_identifier') or survey timestamp fields (e.g., instrument+'_timestamp') when surveys are utilized in the project. If you do not pass in this flag, it will default to 'false'. If set to 'true', it will return the redcap_survey_identifier field and also the survey timestamp field for a particular survey when at least one field from that survey is being exported. NOTE: If the survey identifier field or survey timestamp fields are imported via API data import, they will simply be ignored since they are not real fields in the project but rather are pseudo-fields.</param>
		/// <param name="exportDataAccessGroups">true, false [default] - specifies whether or not to export the 'redcap_data_access_group' field when data access groups are utilized in the project. If you do not pass in this flag, it will default to 'false'. NOTE: This flag is only viable if the user whose token is being used to make the API request is *not* in a data access group. If the user is in a group, then this flag will revert to its default value.</param>
		/// <param name="filterLogic">String of logic text (e.g., [age] > 30) for filtering the data to be returned by this API method, in which the API will only return the records (or record-events, if a longitudinal project) where the logic evaluates as TRUE. This parameter is blank/null by default unless a value is supplied. Please note that if the filter logic contains any incorrect syntax, the API will respond with an error message. </param>
		/// <returns>Data from the project in the format and type specified ordered by the record (primary key of project) and then by event id</returns>
		public async Task<string> ExportRecordAsync(string token, Content content, string record, ReturnFormat format = ReturnFormat.json, RedcapDataType redcapDataType = RedcapDataType.flat, string[] fields = null, string[] forms = null, string[] events = null, RawOrLabel rawOrLabel = RawOrLabel.raw, RawOrLabelHeaders rawOrLabelHeaders = RawOrLabelHeaders.raw, bool exportCheckboxLabel = false, OnErrorFormat onErrorFormat = OnErrorFormat.json, bool exportSurveyFields = false, bool exportDataAccessGroups = false, string filterLogic = null)
		{
			try
			{
				this.CheckToken(token);

				var payload = new Dictionary<string, string>
				{
					{ "token", token },
					{ "records", record },
					{ "content", content.GetDisplayName() },
					{ "format", format.GetDisplayName() },
					{ "returnFormat", onErrorFormat.GetDisplayName() },
					{ "type", redcapDataType.GetDisplayName() }
				};

				// Optional
				if (fields?.Length > 0)
				{
					payload.Add("fields", await this.ConvertArraytoString(fields));
				}
				if (forms?.Length > 0)
				{
					payload.Add("forms", await this.ConvertArraytoString(forms));
				}
				if (events?.Length > 0)
				{
					payload.Add("events", await this.ConvertArraytoString(events));
				}
				/*
                 * Pertains to CSV data only
                 */
				var _rawOrLabelValue = rawOrLabelHeaders.ToString();
				if (!IsNullOrEmpty(_rawOrLabelValue))
				{
					payload.Add("rawOrLabel", _rawOrLabelValue);
				}
				// Optional (defaults to false)
				if (exportCheckboxLabel)
				{
					payload.Add("exportCheckboxLabel", exportCheckboxLabel.ToString());
				}
				// Optional (defaults to false)
				if (exportSurveyFields)
				{
					payload.Add("exportSurveyFields", exportSurveyFields.ToString());
				}
				// Optional (defaults to false)
				if (exportDataAccessGroups)
				{
					payload.Add("exportDataAccessGroups", exportDataAccessGroups.ToString());
				}
				if (!IsNullOrEmpty(filterLogic))
				{
					payload.Add("filterLogic", filterLogic);
				}
				return await this.SendPostRequestAsync(payload, _uri);

			}
			catch (Exception Ex)
			{
				/*
                 * We'll just log the error and return the error message.
                 */
				Log.Error($"{Ex.Message}");
				return Ex.Message;
			}

		}

		public async Task<string> ExportMetaDataAsync(string token, Content content = Content.MetaData, ReturnFormat format = ReturnFormat.json, string[] fields = null, string[] forms = null, OnErrorFormat onErrorFormat = OnErrorFormat.json)
		{
			try
			{
				/*
                 * Check for presence of token
                 */
				this.CheckToken(token);
				var payload = new Dictionary<string, string>
				{
					{ "token", token },
					{ "content", content.GetDisplayName() },
					{ "format", format.GetDisplayName() },
					{ "returnFormat", onErrorFormat.GetDisplayName() }
				};
				// Optional
				if (fields?.Length > 0)
				{
					for (var i = 0; i < fields.Length; i++)
					{
						payload.Add($"fields[{i}]", fields[i].ToString());
					}
				}
				if (forms?.Length > 0)
				{
					for (var i = 0; i < forms.Length; i++)
					{
						payload.Add($"forms[{i}]", forms[i].ToString());
					}
				}
				return await this.SendPostRequestAsync(payload, _uri);
			}
			catch (Exception Ex)
			{
				/*
                 * We'll just log the error and return the error message.
                 */
				Log.Error($"{Ex.Message}");
				return Ex.Message;
			}
		}

		/// <summary>
		/// API Version 1.0.0+
		/// Export Records
		/// This method allows you to export a set of records for a project.
		/// Note about export rights: Please be aware that Data Export user rights will be applied to this API request.For example, if you have 'No Access' data export rights in the project, then the API data export will fail and return an error. And if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then some data fields *might* be removed and filtered out of the data set returned from the API. To make sure that no data is unnecessarily filtered out of your API request, you should have 'Full Data Set' export rights in the project.
		/// </summary>
		/// <remarks>
		/// To use this method, you must have API Export privileges in the project.
		/// </remarks>
		/// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
		/// <param name="content">record</param>
		/// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
		/// <param name="redcapDataType">flat - output as one record per row [default], eav - output as one data point per row. Non-longitudinal: Will have the fields - record*, field_name, value. Longitudinal: Will have the fields - record*, field_name, value, redcap_event_name</param>
		/// <param name="records">an array of record names specifying specific records you wish to pull (by default, all records are pulled)</param>
		/// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all fields are pulled)</param>
		/// <param name="forms">an array of form names you wish to pull records for. If the form name has a space in it, replace the space with an underscore (by default, all records are pulled)</param>
		/// <param name="events">an array of unique event names that you wish to pull records for - only for longitudinal projects</param>
		/// <param name="rawOrLabel">raw [default], label - export the raw coded values or labels for the options of multiple choice fields</param>
		/// <param name="rawOrLabelHeaders">raw [default], label - (for 'csv' format 'flat' type only) for the CSV headers, export the variable/field names (raw) or the field labels (label)</param>
		/// <param name="exportCheckboxLabel">true, false [default] - specifies the format of checkbox field values specifically when exporting the data as labels (i.e., when rawOrLabel=label) in flat format (i.e., when type=flat). When exporting labels, by default (without providing the exportCheckboxLabel flag or if exportCheckboxLabel=false), all checkboxes will either have a value 'Checked' if they are checked or 'Unchecked' if not checked. But if exportCheckboxLabel is set to true, it will instead export the checkbox value as the checkbox option's label (e.g., 'Choice 1') if checked or it will be blank/empty (no value) if not checked. If rawOrLabel=false or if type=eav, then the exportCheckboxLabel flag is ignored. (The exportCheckboxLabel parameter is ignored for type=eav because 'eav' type always exports checkboxes differently anyway, in which checkboxes are exported with their true variable name (whereas the 'flat' type exports them as variable___code format), and another difference is that 'eav' type *always* exports checkbox values as the choice label for labels export, or as 0 or 1 (if unchecked or checked, respectively) for raw export.)</param>
		/// <param name="onErrorFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
		/// <param name="exportSurveyFields">true, false [default] - specifies whether or not to export the survey identifier field (e.g., 'redcap_survey_identifier') or survey timestamp fields (e.g., instrument+'_timestamp') when surveys are utilized in the project. If you do not pass in this flag, it will default to 'false'. If set to 'true', it will return the redcap_survey_identifier field and also the survey timestamp field for a particular survey when at least one field from that survey is being exported. NOTE: If the survey identifier field or survey timestamp fields are imported via API data import, they will simply be ignored since they are not real fields in the project but rather are pseudo-fields.</param>
		/// <param name="exportDataAccessGroups">true, false [default] - specifies whether or not to export the 'redcap_data_access_group' field when data access groups are utilized in the project. If you do not pass in this flag, it will default to 'false'. NOTE: This flag is only viable if the user whose token is being used to make the API request is *not* in a data access group. If the user is in a group, then this flag will revert to its default value.</param>
		/// <param name="filterLogic">String of logic text (e.g., [age] > 30) for filtering the data to be returned by this API method, in which the API will only return the records (or record-events, if a longitudinal project) where the logic evaluates as TRUE. This parameter is blank/null by default unless a value is supplied. Please note that if the filter logic contains any incorrect syntax, the API will respond with an error message. </param>
		/// <returns>Data from the project in the format and type specified ordered by the record (primary key of project) and then by event id</returns>
		public async Task<string> ExportRecordsAsync(string token, Content content, ReturnFormat format = ReturnFormat.json, RedcapDataType redcapDataType = RedcapDataType.flat, string[] records = null, string[] fields = null, string[] forms = null, string[] events = null, RawOrLabel rawOrLabel = RawOrLabel.raw, RawOrLabelHeaders rawOrLabelHeaders = RawOrLabelHeaders.raw, bool exportCheckboxLabel = false, OnErrorFormat onErrorFormat = OnErrorFormat.json, bool exportSurveyFields = false, bool exportDataAccessGroups = false, string filterLogic = null)
		{
			try
			{
				this.CheckToken(token);

				var payload = new Dictionary<string, string>
				{
					{ "token", token },
					{ "content", content.GetDisplayName() },
					{ "format", format.GetDisplayName() },
					{ "returnFormat", onErrorFormat.GetDisplayName() },
					{ "type", redcapDataType.GetDisplayName() }
				};

				// Optional
				if (records?.Length > 0)
				{
					payload.Add("records", await this.ConvertArraytoString(records));
				}
				if (fields?.Length > 0)
				{
					payload.Add("fields", await this.ConvertArraytoString(fields));
				}
				if (forms?.Length > 0)
				{
					payload.Add("forms", await this.ConvertArraytoString(forms));
				}
				if (events?.Length > 0)
				{
					payload.Add("events", await this.ConvertArraytoString(events));
				}
				/*
                 * Pertains to CSV data only
                 */
				var _rawOrLabelValue = rawOrLabelHeaders.ToString();
				if (!IsNullOrEmpty(_rawOrLabelValue))
				{
					payload.Add("rawOrLabel", _rawOrLabelValue);
				}
				// Optional (defaults to false)
				if (exportCheckboxLabel)
				{
					payload.Add("exportCheckboxLabel", exportCheckboxLabel.ToString());
				}
				// Optional (defaults to false)
				if (exportSurveyFields)
				{
					payload.Add("exportSurveyFields", exportSurveyFields.ToString());
				}
				// Optional (defaults to false)
				if (exportDataAccessGroups)
				{
					payload.Add("exportDataAccessGroups", exportDataAccessGroups.ToString());
				}
				if (!IsNullOrEmpty(filterLogic))
				{
					payload.Add("filterLogic", filterLogic);
				}
				return await this.SendPostRequestAsync(payload, _uri);

			}
			catch (Exception Ex)
			{
				/*
                 * We'll just log the error and return the error message.
                 */
				Log.Error($"{Ex.Message}");
				return Ex.Message;
			}

		}
	}
}
