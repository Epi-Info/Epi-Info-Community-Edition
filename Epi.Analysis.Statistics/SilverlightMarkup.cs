using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace Epi
{
    public class NumericDataValue
    {
        public decimal DependentValue { get; set; }
        public decimal IndependentValue { get; set; }
    }

    public class SilverlightMarkup
    {
        // this format is only used to write and read the inputParams
        const string _dateTimeFormat = "MM/dd/yyyy·hh:mm:ss.fff·tt";

        public string Graph(string chartType, string chartTitle, string dependentLabel, string independentLabel, string independentValueFormat, string dependentValueFormat, string interval, string intervalUnit, object startFrom, DataTable regressionTable)
        {
            string inputParams = string.Empty;
            inputParams = BuildInputParams(chartType, chartTitle, dependentLabel, independentLabel, independentValueFormat, dependentValueFormat, interval, intervalUnit, startFrom, regressionTable);

            if (string.IsNullOrEmpty(inputParams))
            {
                return string.Empty;
            }

            return Graph(inputParams);
        }

        public string Graph(string inputParams)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<br clear=""all"">");
            sb.AppendLine(@"<div id=""silverlightControlHost"">");
            sb.AppendLine(@"<object data=""data:application/x-silverlight-2,""");
            sb.AppendLine(@"type=""application/x-silverlight-2"" style=""width:820; height: 615px"">");
            sb.AppendLine(@"<param name=""initparams"" value=""" + inputParams + @"""/>");
            sb.AppendLine(@"<param name=""source"" value=""SilverlightApplication.xap"" />");
            sb.AppendLine(@"<param name=""minRuntimeVersion"" value=""4.0.50826.0"" />");
            sb.AppendLine(@"<param name=""autoUpgrade"" value=""true"" />");
            sb.AppendLine(@"<a href=""http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50826.0"" style=""text-decoration: none"">");
            sb.AppendLine(@"<img src=""http://go.microsoft.com/fwlink/?LinkId=161376"" alt=""Get Microsoft Silverlight""");
            sb.AppendLine(@"style=""border-style: none"" />");
            sb.AppendLine(@"</a>");
            sb.AppendLine(@"</object>");
            sb.AppendLine(@"</div>");
            sb.AppendLine(@"<br />");
            return sb.ToString();
        }

        public string BuildInputParams(
            string chartType, 
            string chartTitle, 
            string independentTitle, 
            string dependentTitle, 
            string independentValueFormat,
            string dependentValueFormat,
            string interval,
            string intervalUnit,
            object startFrom,
            DataTable regressionTable)
        {
            StringBuilder chartLineSeries = new StringBuilder();

            string independentValueType = "";
            string dependentValueType = "";

            chartType = chartType.Replace("\"", "");
            chartTitle = chartTitle.Replace("\"", "");
            independentTitle = independentTitle.Replace("\"", "");
            dependentTitle = dependentTitle.Replace("\"", "");

            switch (chartType.ToUpper().Replace(" ", ""))
            {
                case SilverlightStatics.Area:
                    chartType = SilverlightStatics.Area;
                    break;

                case SilverlightStatics.Column:
                    chartType = SilverlightStatics.Bar;
                    break;

                case SilverlightStatics.Bubble:
                    chartType = SilverlightStatics.Bubble;
                    break;

                case SilverlightStatics.EpiCurve:
                    chartType = SilverlightStatics.Histogram;
                    break;

                case SilverlightStatics.Bar:
                    chartType = SilverlightStatics.RotatedBar;
                    break;

                case SilverlightStatics.Line:
                    chartType = SilverlightStatics.Line;
                    break;

                case SilverlightStatics.Pie:
                    chartType = SilverlightStatics.Pie;
                    break;

                case SilverlightStatics.Scatter:
                    chartType = SilverlightStatics.Scatter;
                    break;

                case SilverlightStatics.Stacked:
                    chartType = SilverlightStatics.Stacked;
                    break;

                case SilverlightStatics.TreeMap:
                    chartType = SilverlightStatics.TreeMap;
                    break;

                default:
                    chartType = SilverlightStatics.Bar;
                    break;
            }

            chartLineSeries.Append("chartLineSeries=");

            DataTable distinct = regressionTable.DefaultView.ToTable(true, "SeriesName");

            if (regressionTable.Rows.Count == 0)
            {
                return string.Empty;
            }

            foreach (DataRow row in distinct.Rows)
            {
                string seriesName = row["SeriesName"].ToString();
                string filter = string.Format("SeriesName = '{0}'", seriesName);

                DataRow[] regressionData = regressionTable.Select(filter);

                if (chartLineSeries.ToString().EndsWith("chartLineSeries=") == false)
                {
                    chartLineSeries.Append(SilverlightStatics.SeparateLineSeries);
                }

                chartLineSeries.Append("(");
                chartLineSeries.Append("LineSeriesTitle");
                chartLineSeries.Append(seriesName);
                chartLineSeries.Append("LineSeriesTitle");

                Type indepValueType = typeof(string);
                indepValueType = regressionData[0]["Predictor"].GetType();
                independentValueType = indepValueType.ToString();
                                     
                chartLineSeries.Append(SilverlightStatics.IndependentValueFormat);
                chartLineSeries.Append(independentValueFormat);
                chartLineSeries.Append(SilverlightStatics.IndependentValueFormat);

                chartLineSeries.Append(SilverlightStatics.DependentValueFormat);
                chartLineSeries.Append(dependentValueFormat);
                chartLineSeries.Append(SilverlightStatics.DependentValueFormat);

                chartLineSeries.Append(SilverlightStatics.LineSeriesDataString);

                foreach (DataRow regression in regressionData)
                {
                    if (regression["Predictor"] is DateTime)
                    {
                        string dateTimeString = ((DateTime)regression["Predictor"]).ToString(_dateTimeFormat);
                        chartLineSeries.Append(dateTimeString);
                    }
                    else
                    {
                        chartLineSeries.Append(regression["Predictor"].ToString());
                    }

                    chartLineSeries.Append(SilverlightStatics.SeparateIndDepValues);
                    chartLineSeries.Append(regression["Response"].ToString());
                    chartLineSeries.Append(SilverlightStatics.SeparateDataPoints);
                }

                if (chartLineSeries.ToString().EndsWith(SilverlightStatics.SeparateDataPoints))
                {
                    string series = chartLineSeries.ToString().Substring(0, chartLineSeries.Length - SilverlightStatics.SeparateDataPoints.Length);
                    chartLineSeries = new StringBuilder();
                    chartLineSeries.Append(series);
                }

                chartLineSeries.Append(SilverlightStatics.LineSeriesDataString);
            }

            if (chartType == SilverlightStatics.Scatter)
            {
                List<NumericDataValue> dataValues = new List<NumericDataValue>();
                NumericDataValue minValue = null;
                NumericDataValue maxValue = null;
                foreach (DataRow row in regressionTable.Rows)
                {
                    if (row["Response"].Equals(DBNull.Value) || row["Predictor"].Equals(DBNull.Value))
                    {
                        continue;
                    }
                    NumericDataValue currentValue = new NumericDataValue() { DependentValue = Convert.ToDecimal(row["Response"]), IndependentValue = Convert.ToDecimal(row["Predictor"]) };
                    dataValues.Add(currentValue);
                    if (minValue == null)
                    {
                        minValue = currentValue;
                    }
                    else
                    {
                        if (currentValue.IndependentValue < minValue.IndependentValue)
                        {
                            minValue = currentValue;
                        }
                    }
                    if (maxValue == null)
                    {
                        maxValue = currentValue;
                    }
                    else
                    {
                        if (currentValue.IndependentValue > maxValue.IndependentValue)
                        {
                            maxValue = currentValue;
                        }
                    }
                }

                StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("Response", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("p", "0.95");
                inputVariableList.Add("Predictor", "unsorted");

                StatisticsRepository.LinearRegression.LinearRegressionResults regresResults = linearRegression.LinearRegression(inputVariableList, regressionTable);

                object[] result = new object[] { dataValues, regresResults, maxValue, minValue };

                decimal coefficient = Convert.ToDecimal(regresResults.variables[0].coefficient);
                decimal constant = Convert.ToDecimal(regresResults.variables[1].coefficient);

                NumericDataValue newMaxValue = new NumericDataValue();
                newMaxValue.IndependentValue = maxValue.IndependentValue + 1;
                newMaxValue.DependentValue = (coefficient * maxValue.IndependentValue) + constant;
                NumericDataValue newMinValue = new NumericDataValue();
                newMinValue.IndependentValue = minValue.IndependentValue - 1;
                newMinValue.DependentValue = (coefficient * minValue.IndependentValue) + constant;

                chartLineSeries.Append("LineSeriesDataString)^((LineSeriesTitleLinear RegressionLineSeriesTitleIndependentValueFormatIndependentValueFormatDependentValueFormatDependentValueFormatLineSeriesDataString");
                chartLineSeries.Append(newMinValue.IndependentValue.ToString());
                chartLineSeries.Append("^sidv^");
                chartLineSeries.Append(newMinValue.DependentValue.ToString());
                chartLineSeries.Append("^&^");
                chartLineSeries.Append(newMaxValue.IndependentValue.ToString());
                chartLineSeries.Append("^sidv^");
                chartLineSeries.Append(newMaxValue.DependentValue.ToString());
                chartLineSeries.Append("LineSeriesDataString");
            }

            chartLineSeries.Append(")");

            string inputParams = string.Empty;

            if (chartType.Length > 0) inputParams += string.Format("chartType={0},", chartType);
            if (chartTitle.Length > 0) inputParams += string.Format("chartTitle={0},", chartTitle);

            if (independentTitle.Length > 0) inputParams += string.Format("independentLabel={0},", independentTitle);
            if (dependentTitle.Length > 0) inputParams += string.Format("dependentLabel={0},", dependentTitle);

            if (independentValueFormat.Length > 0) inputParams += string.Format("independentValueFormat={0},", independentValueFormat);
            if (dependentValueFormat.Length > 0) inputParams += string.Format("dependentValueFormat={0},", dependentValueFormat);

            if (independentValueType.Length > 0) inputParams += string.Format("independentValueType={0},", independentValueType);
            if (dependentValueType.Length > 0) inputParams += string.Format("dependentValueType={0},", dependentValueType);

            if (interval.Length > 0) inputParams += string.Format("interval={0},", interval);
            if (intervalUnit.Length > 0) inputParams += string.Format("intervalUnit={0},", intervalUnit);

            if (startFrom is DateTime)
            {
                startFrom = ((DateTime)startFrom).ToString(_dateTimeFormat);
            }
            else
            {
                startFrom = startFrom.ToString();
            }
            
            
            if (((string)startFrom).Length > 0) inputParams += string.Format("startFrom={0},", ((string)startFrom));

            inputParams += chartLineSeries.Replace(",", SilverlightStatics.Comma);
            return inputParams;
        }
    }
}
