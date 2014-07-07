using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Epi.Collections;
using System.ComponentModel.Design;

namespace Epi.Analysis
{
    /// <summary>
    /// Analysis engine class
    /// </summary>
    public class AnalysisEngine : ModuleBase, IProjectHost
    {

        #region Private Attributes
        //private Epi.Analysis.AnalysisCommandProcessor processor = null;
        private Project currentProject;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public AnalysisEngine()
        {
        }

        /// <summary>
        /// Creates an analysis engine instance
        /// </summary>
        /// <param name="moduleManager"></param>
        /// <param name="commandLine"></param>
        public AnalysisEngine(IModuleManager moduleManager, ICommandLine commandLine)
        {
            Load(moduleManager, commandLine);
        }

        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Return a reference to the current project
        /// </summary>
        public Project CurrentProject
        {
            get
            {
                return currentProject;
            }
            set
            {
                currentProject = value;
            }

        }

        /*
        /// <summary>
        /// Command processor reference
        /// </summary>
        public AnalysisCommandProcessor Processor
        {
            get
            {
                return this.processor;
            }
        }*/
        #endregion Public Properties

        #region Protected Properties
        /// <summary>
        /// Return the name of the module
        /// </summary>
        protected override string ModuleName
        {
            get { return "Analysis Engine"; }
        }
        #endregion Protected Properties

        #region Protected Methods

        /// <summary>
        /// Load()
        /// </summary>
        /// <param name="moduleManager"></param>
        /// <param name="commandLine"></param>
        protected override void Load(IModuleManager moduleManager, ICommandLine commandLine)
        {
            base.Load(moduleManager, commandLine);
            //processor = new AnalysisCommandProcessor(this);
            // base.OnLoaded();
        }
        /// <summary>
        /// Unload method
        /// </summary>
        protected override void Unload()
        {
            base.OnUnloaded();
        }
        #endregion Protected Methods

        #region Public Methods
        /// <summary>
        /// <para> Returns an object that represents a service provided by the
        /// <see cref="T:System.ComponentModel.Component" /> or by its <see cref="T:System.ComponentModel.Container" />.
        /// </para>
        /// </summary>
        /// <param name="serviceType">A service provided by the <see cref="T:System.ComponentModel.Component" />. </param>
        /// <returns>
        /// <para> An <see cref="T:System.Object" /> that represents a service provided by the
        /// <see cref="T:System.ComponentModel.Component" />.
        /// </para>
        /// <para> This value is <see langword="null" /> if the <see cref="T:System.ComponentModel.Component" /> does not provide the
        /// specified service.</para>
        /// </returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public override object GetService(Type serviceType)
        {
            if (serviceType == this.GetType())
            {
                return this;
            }
            else if (serviceType == typeof(IProjectHost))
            {
                return this;
            }
                /*
            else if (serviceType == typeof(Session))
            {
                return this.Processor.Session;
            }
            else if (serviceType == typeof(AnalysisCommandProcessor) || serviceType == typeof(ICommandProcessor))
            {
                return this.Processor;
            }*/
            else
            {
                return base.GetService(serviceType);
            }
        }

        /// <summary>
        /// Returns a new instance of a project by reading it from a project file.
        /// </summary>
        /// <param name="projectPath">Path of project.</param>
        /// <returns>New Epi Info Project</returns>
        public Project CreateProject(string projectPath)
        {
            Project project = null;

            if (FileExtensions.HasExtension(projectPath, FileExtensions.MDB))
            {
                throw new NotImplementedException(SharedStrings.EPI3_PROJECTS);
            }
            else
            {
                project = new Project(projectPath);
                if (project == null)
                {
                    throw new GeneralException(SharedStrings.INVALID_PROJECT_FILE);
                }
            }
            return project;
        }

        #endregion Public Methods

    }
}