using System;

namespace Epi
{
	/// <summary>
	/// Defines all the settings to be implemented at three levels: Default, Config, Temp
	/// </summary>
	public interface ISettings
	{
        /// <summary>
        /// Property for Background Image (settings)
        /// </summary>
		string BackgroundImage
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Default database format (settings)
        /// </summary>
		int DefaultDatabaseFormat
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Default data format (settings)
        /// </summary>
		int DefaultDataFormat
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Field prompt font (settings)
        /// </summary>
		string EditorFontName
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Field prompt font size (settings)
        /// </summary>
		System.Decimal EditorFontSize
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Field control font (settings)
        /// </summary>
        string ControlFontName
        {
            get;
            set;
        }

        /// <summary>
        /// Property for Field control font size (settings)
        /// </summary>
        System.Decimal ControlFontSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property for Include missing values (settings)
        /// </summary>
		bool? IncludeMissingValues
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Language (settings)
        /// </summary>
		string Language
		{
			get;
			set;
		}

        /// <summary>
        /// Property for MRU Projects count (settings)
        /// </summary>
		int MRUProjectsCount
		{
			get;
			set;
		}

        /// <summary>
        /// Property for MRU Views Count(settings)
        /// </summary>
		int MRUViewsCount
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Project Directory (settings)
        /// </summary>
		string ProjectDirectory
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Record Processing Scope (settings)
        /// </summary>
		int RecordProcessingScope
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Representation of Yes (settings)
        /// </summary>
		string RepresentationOfYes
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Representation of No(settings)
        /// </summary>
		string RepresentationOfNo
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Representation of missing (settings)
        /// </summary>
		string RepresentationOfMissing
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show Complete (settings)
        /// </summary>
		bool? ShowCompletePrompt
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show Graphics(settings)
        /// </summary>
		bool? ShowGraphics
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show HyperLinks (settings)
        /// </summary>
		bool? ShowHyperlinks
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show Percents(settings)
        /// </summary>
		bool? ShowPercents
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show Selection(settings)
        /// </summary>
		bool? ShowSelection
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Show Tables(settings)
        /// </summary>
		bool? ShowTables
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Snap to Grid(settings)
        /// </summary>
		bool? SnapToGrid
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Statisatics Level(settings)
        /// </summary>
		int StatisticsLevel
		{
			get;
			set;
		}

        /// <summary>
        /// Property for Working Directory (settings)
        /// </summary>
		string WorkingDirectory
		{
			get;
			set;
		}
	}
}