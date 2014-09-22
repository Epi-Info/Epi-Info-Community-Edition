using System;

namespace Epi
{

    /// <summary>
    /// Structure for all the settings, all in the public interface
    /// </summary>
	public struct Settings
	{
        /// <summary>Background Image Setting</summary>
        public string BackgroundImage;
        /// <summary>Default Database Format Setting</summary>
        public int DefaultDatabaseFormat;
        /// <summary>Default Data Format Setting</summary>
        public int DefaultDataFormat;
        /// <summary>Editor Font Name Setting</summary>
        public string EditorFontName;
        /// <summary>Editor Font Size Setting</summary>
        public System.Decimal EditorFontSize;
        /// <summary>Control Font Name Setting</summary>
        public string ControlFontName;
        /// <summary>Control Font Size Setting</summary>
        public System.Decimal ControlFontSize;
        /// <summary>Include Missing Values Flag Setting</summary>
        public bool IncludeMissingValues;
        /// <summary>Language Setting</summary>
        public string Language;
        /// <summary>Most Recently Used Projects Count Setting</summary>
        public int MRUProjectsCount;
        /// <summary>Most Recently Used Views Count Setting</summary>
        public int MRUViewsCount;
        /// <summary>Project Directory Setting</summary>
        public string ProjectDirectory;
        /// <summary>Record Processing Scope Setting</summary>
        public int RecordProcessingScope;
        /// <summary>Representation Of Yes Setting</summary>
        public string RepresentationOfYes;
        /// <summary>Representation Of No Setting</summary>
        public string RepresentationOfNo;
        /// <summary>Representation Of Missing Setting</summary>
        public string RepresentationOfMissing;
        /// <summary>Show Complete Prompt Flag Setting</summary>
        public bool ShowCompletePrompt;
        /// <summary>Show Graphics Flag Setting</summary>
        public bool ShowGraphics;
        /// <summary>Show Hyperlinks Flag Setting</summary>
        public bool ShowHyperlinks;
        /// <summary>Show Percents Flag Setting</summary>
        public bool ShowPercents;
        /// <summary>Show Selection Flag Setting</summary>
        public bool ShowSelection;
        /// <summary>Show Tables Flag Setting</summary>
        public bool ShowTables;
        /// <summary>Snap To Grid Flag Setting</summary>
        public bool SnapToGrid;
        /// <summary>Statistics Level Setting</summary>
        public int StatisticsLevel;
        /// <summary>Working Directory Setting</summary>
        public string WorkingDirectory;
	}
}
